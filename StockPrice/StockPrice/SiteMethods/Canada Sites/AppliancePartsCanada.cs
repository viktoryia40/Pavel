using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class AppliancePartsCanada
    {
        private const string Source = "xpartsupply.ca";
        private const string ClassSource = "AppliancePartsCanada";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.xpartsupply.ca/search?type=product&q={search}", Source = "Xpartsupply.ca" };
            var prices = new List<Prices>();

            string searchResult;
            try
            {
                searchResult = CustomHttpClass.GetToString(@$"https://xpartsupply.ca/search?type=product&q={search}&view=json");
            }
            catch (Exception ex)
            {
                await ResponseCreator.MakeErrorLog(con: con,
                    mpr: mpr,
                    mainPriceResponsesList: mainPriceResponsesList,
                    request: request,
                    base64ErrorData: ex.Message.ToString(),
                    stage: 0,
                    source: Source,
                    classSource: ClassSource,
                    base64WrongData: null,
                    url: null);
                return;
            }

            if (!string.IsNullOrEmpty(searchResult))
            {
                dynamic _j_res;

                try
                {
                    _j_res = JsonConvert.DeserializeObject(searchResult);
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message.ToString(),
                        stage: 1,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }

                if (_j_res.results.Count > 0)
                {
                    if (_j_res.results.Count > 1) mpr.MultiChoice = true;
                    string title = _j_res.results[0].title;
                    string url = $@"https://www.xpartsupply.ca{_j_res.results[0].url.ToString()}";

                    try

                    {
                        string productResp = CustomHttpClass.GetToString(url);

                        if (!string.IsNullOrEmpty(productResp))
                        {
                            var priceRegex = Regex.Matches(productResp, @"(?<= <meta property=""og:price:amount"" content="").*?(?="">)");
                            if (priceRegex.Count > 0)
                            {
                                decimal price = decimal.Parse(priceRegex.First().Value.Trim(), CultureInfo.InvariantCulture);

                                prices.Add(new()
                                {
                                    Price = price,
                                    Title = title,
                                    Url = url
                                });


                                prices = prices.OrderBy(x => x.Price).ToList();
                                decimal lowest_price = prices.Select(x => x.Price).First();
                                mpr.LowestPrice = lowest_price;
                                mpr.PricesList = prices;




                                mainPriceResponsesList.Add(mpr);

                                await ResponseCreator.MakeResponseLog(con: con,
                                    mpr: mpr,
                                    request: request);
                                return;
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        await ResponseCreator.MakeErrorLog(con: con,
                            mpr: mpr,
                            mainPriceResponsesList: mainPriceResponsesList,
                            request: request,
                            base64ErrorData: ex.Message.ToString(),
                            stage: 2,
                            source: Source,
                            classSource: ClassSource,
                            base64WrongData: null,
                            url: null);
                        return;
                    }
                }
                else
                {
                    mpr.NothingFoundOrOutOfStock = true;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }
            }
        }

    }
}
