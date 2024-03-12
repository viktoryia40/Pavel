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
    public sealed class MajorApplianceParts
    {
        private const string Source = "majorapplianceparts.ca";
        private const string ClassSource = "MajorApplianceParts";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://majorapplianceparts.ca/?s={search}&post_type=product&type_aws=true", Source = "Majorapplianceparts.ca" };
            var prices = new List<Prices>();

            string redirect;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://majorapplianceparts.ca/?s={search}&post_type=product&type_aws=true");

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

            string totalUrl = null;

            if (redirect != null)
            {
                totalUrl = redirect;
            }
            else
            {
                string searchResult = null;
                mpr.MultiChoice = true;
                try
                {
                    searchResult = CustomHttpClass.GetToString(@$"https://majorapplianceparts.ca/shop/?swoof=1&post_type=product&type_aws=true&woof_text={search}&orderby=price", acceptencoding: "none");
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


                var hrefRegex = Regex.Matches(searchResult, @"(?<=<h2><a href="")https://majorapplianceparts.ca/product.*?(?="">)");
                if (hrefRegex.Any())
                {
                    totalUrl = hrefRegex.First().ToString();
                }
                else //If you can't find the product links
                {
                    mpr.NothingFoundOrOutOfStock = true;
                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);

                    return;
                }
            }

            if (totalUrl != null)
            {
                try
                {
                    string resp = CustomHttpClass.GetToString(totalUrl, acceptencoding: "none");

                    var titleRegex = Regex.Matches(resp, @"(?<=<title>).*(?=</title>)");
                    if (titleRegex.Any())
                    {
                        string title = titleRegex.First().Value;

                        var priceRegex = Regex.Matches(resp, @"(?<=""price"":"").*?(?="")");
                        if (priceRegex.Count > 0)
                        {
                            decimal price = decimal.Parse(priceRegex.First().Value, CultureInfo.InvariantCulture);

                            prices.Add(new()
                            {
                                Price = price,
                                Title = title,
                                Url = totalUrl
                            });
                            prices = prices.OrderBy(x => x.Price).ToList();
                            decimal lowestPrice = prices.Select(x => x.Price).First();
                            mpr.LowestPrice = lowestPrice;

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

        }

    }
}
