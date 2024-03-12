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
    public sealed class ApwagnerCa
    {
        private const string Source = "apwagner.ca";
        private const string ClassSource = "ApwagnerCa";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.apwagner.ca/search/{search}", Source = "Apwagner.ca" };
            var prices = new List<Prices>();

            string redirect;
            try
            {
                redirect = CustomHttpClass.CheckRedirectGet($"https://www.apwagner.ca/search/{search}");
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

            if (!string.IsNullOrEmpty(redirect))
            {
                totalUrl = $"https://www.apwagner.ca{redirect}";
            }
            else
            {
                string searchResult = null;
                try
                {
                    searchResult = CustomHttpClass.GetToString($"https://www.apwagner.ca/search/{search}", acceptencoding: "none");
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

                if (searchResult != null)
                {
                    List<Prices> tmpPriceList = new();
                    var searchResultRegex = Regex.Matches(searchResult, @"(?<=<li class=""item jenn-border-bottom"">)[\w\W]*?(?=</li>)");
                    if (searchResultRegex.Count > 0)
                    {
                        mpr.MultiChoice = true;
                        List<string> tempHtmlResultBlocks = new();
                        for (int i = 0; i < searchResultRegex.Count; i++) tempHtmlResultBlocks.Add(searchResultRegex[i].Value);

                        tempHtmlResultBlocks = tempHtmlResultBlocks.Where(x => !x.Contains("DISCONTINUED")).ToList();
                        if (tempHtmlResultBlocks.Count == 0)
                        {
                            mpr.NothingFoundOrOutOfStock = true;
                            mainPriceResponsesList.Add(mpr);

                            await ResponseCreator.MakeResponseLog(con: con,
                                mpr: mpr,
                                request: request);

                            return;
                        }
                        else
                        {
                            var hrefRegex = Regex.Matches(tempHtmlResultBlocks.First().ToString(), @"(?<= href="").*?(?="")");
                            if (hrefRegex.Count > 0)
                                totalUrl = $"https://www.apwagner.ca{hrefRegex.First().Value.Trim()}";
                            else
                            {
                                mpr.NoAnswerOrError = true;
                                mpr.ErrorMessage = "Regex href error.";
                                mainPriceResponsesList.Add(mpr);

                                await ResponseCreator.MakeResponseLog(con: con,
                                    mpr: mpr,
                                    request: request);

                                return;
                            }
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

            if (totalUrl != null)
            {
                try
                {
                    string response = CustomHttpClass.GetToString(totalUrl, acceptencoding: "none");

                    var titleRegex = Regex.Matches(response, @"(?<=<h1 itemprop=""name"">)[\w\W]*?(?=</h1>)");
                    if (titleRegex.Count > 0)
                    {
                        var title = titleRegex[0].Value.Trim();
                        var availabilityRegex = Regex.Matches(response, @"(?<=<span class=""pdetail_availability"">).*?(?=</span>)");
                        if (availabilityRegex.Count > 0)
                        {
                            string avaibility = availabilityRegex[0].Value.Trim();

                            if (avaibility.Equals("Out Of Stock"))
                            {
                                mpr.NothingFoundOrOutOfStock = true;
                                mainPriceResponsesList.Add(mpr);

                                await ResponseCreator.MakeResponseLog(con: con,
                                    mpr: mpr,
                                    request: request);
                                return;
                            }

                            var priceBlockRegex = Regex.Matches(response, @"(?<=<span class=""regular-price"" id=""product-price-"">)[\w\W]*?(?=</span>)");
                            if (priceBlockRegex.Count > 0)
                            {
                                string firstPrice = priceBlockRegex[0].Value.Trim();

                                var priceRegex = Regex.Matches(firstPrice, @"\$\d+(?:\.\d+)?");
                                if (priceRegex.Count > 0)
                                {
                                    decimal price = decimal.Parse(priceRegex[0].Value.Replace("$", ""), CultureInfo.InvariantCulture);

                                    prices.Add(new Prices
                                    {
                                        Availability = avaibility,
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
