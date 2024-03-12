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
    public sealed class BulbsPro
    {
        private const string Source = "bulbspro.com";
        private const string ClassSource = "BulbsPro";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {

            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.bulbspro.com/catalogsearch/result/?q={search}&x=0&y=0", Source = "Bulbspro.com" };
            var prices = new List<Prices>();

            string searchResult;

            try
            {
                searchResult = CustomHttpClass.GetToString(@$"https://www.bulbspro.com/catalogsearch/result/?q={search}&x=0&y=0");
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

            string firstProductUrl = null;

            if (searchResult != null)
            {
                var resultDataBlock = Regex.Matches(searchResult, @"(?<=<ol class=""products-list"" id=""products-list"">)[\w\W]*?(?=</ol>)");
                if (resultDataBlock.Count > 0)
                {
                    if (resultDataBlock.Count > 1) mpr.MultiChoice = true;
                    string takenBlock = resultDataBlock.First().Value;

                    var productUrlRegex = Regex.Matches(takenBlock, @"(?<=<h2 class=""product-name""><a href="").*?(?="" title="")");
                    if (productUrlRegex.Count > 0)
                    {
                        firstProductUrl = productUrlRegex.First().Value.Trim();
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

            if (!string.IsNullOrEmpty(firstProductUrl))
            {
                try
                {
                    string productResult = CustomHttpClass.GetToString(firstProductUrl);

                    var titleRegex = Regex.Matches(productResult, @"(?<=<title>).*?(?=</title>)");
                    if (titleRegex.Count > 0)
                    {
                        string title = titleRegex.First().Value.Trim();

                        var availabilityRegex = Regex.Matches(productResult, @"(?<=Availability: <span>).*?(?=</span>)");
                        if (availabilityRegex.Count > 0)
                        {
                            string availability = availabilityRegex.First().Value.Trim();

                            var priceBlockRegex = Regex.Matches(productResult, @"(?<=<span class=""price"">).*?(?=</span>)");
                            if (priceBlockRegex.Count > 0)
                            {
                                var priceRegex = Regex.Matches(priceBlockRegex.First().Value.Trim(), @"\d.");

                                decimal price = decimal.Parse(string.Join('.', priceRegex.ToList()), CultureInfo.InvariantCulture);

                                prices.Add(new Prices
                                {
                                    Availability = availability,
                                    Price = price,
                                    Title = title,
                                    Url = firstProductUrl
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
            }


        }

    }
}
