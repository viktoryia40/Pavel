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
    public sealed class AppliancePartsHq
    {
        private const string Source = "appliancepartshq.ca";
        private const string ClassSource = "AppliancePartsHq";

        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.appliancepartshq.ca/search?hq={search}", Source = "Appliancepartshq.ca", Additional = "📦$13.99" };
            var prices = new List<Prices>();

            string redirect;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.appliancepartshq.ca/search?hq={search}");

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
                totalUrl = redirect;
            }
            else
            {
                string searchResult = null;

                try

                {
                    searchResult = CustomHttpClass.GetToString(@$"https://www.appliancepartshq.ca/search?hq={search}", acceptencoding: "none");
                }
                catch (Exception ex)
                {
                    mpr.NoAnswerOrError = true;
                    mpr.ErrorMessage = ex.Message.ToString();
                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                }
                var productBlockRegex = Regex.Matches(searchResult, @"(?<=<div class=""productListing"">)[\w\W]*?(?=<form action=)");
                if (productBlockRegex.Count > 0)
                {
                    if (productBlockRegex.Count > 1) mpr.MultiChoice = true;

                    List<Prices> tmpPriceSearch = new();

                    for (int i = 0; i < productBlockRegex.Count; i++)
                    {
                        decimal tempPrice = 0;
                        string tempUrl = null;

                        var tempPriceRegex = Regex.Matches(productBlockRegex[i].Value, @"\$\d+(?:\.\d+)?");
                        if (tempPriceRegex.Count > 0) tempPrice = decimal.Parse(tempPriceRegex.First().Value.Trim().Replace("$", ""), CultureInfo.InvariantCulture);
                        var hrefRegex = Regex.Matches(searchResult, @"(?<=href="").*?(?="")");
                        if (hrefRegex.Count > 0) tempUrl = hrefRegex.First().Value.Trim();

                        tmpPriceSearch.Add(new Prices
                        {
                            Price = tempPrice,
                            Url = tempUrl
                        });
                    }
                    tmpPriceSearch = tmpPriceSearch.Where(x => !string.IsNullOrEmpty(x.Url)).Where(x => x.Price > 0).OrderBy(x => x.Price).ToList();

                    if (tmpPriceSearch.Count > 0)
                        totalUrl = tmpPriceSearch.FirstOrDefault().Url;
                }
                else //If there are no results
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
                    string resp = CustomHttpClass.GetToString(totalUrl);

                    var price_block_regex = Regex.Matches(resp, @"(?<=<div class=""productPrice"">)[\w\W]*?(?=</div>)");

                    if (price_block_regex.Count() > 0)
                    {
                        string taken_price_first = price_block_regex.First().Value.Trim();

                        var taken_price_regex = Regex.Matches(taken_price_first, @"\d{1,}");

                        if (taken_price_regex.Count() > 0)
                        {
                            string price_ready = string.Join('.', taken_price_regex.ToList());
                            decimal total_price = decimal.Parse(price_ready, CultureInfo.InvariantCulture);


                            var title_regex = Regex.Matches(resp, @"(?<=>).*?(?=</h1>)");
                            if (title_regex.Count() > 0)
                            {
                                string title = title_regex.First().Value.Trim().Split(':').Last().Trim();


                                var avaibility_block_regex = Regex.Matches(resp, @"(?<=<div class=""productStock"">)[\w\W]*?(?=</div>)");
                                if (avaibility_block_regex.Count() > 0)
                                {
                                    string taken_avaibility_block = avaibility_block_regex.First().Value.Trim();
                                    var avaibility_regex = Regex.Matches(taken_avaibility_block, @"(?<=alt="").*?(?="")");
                                    if (avaibility_regex.Count() > 0)
                                    {
                                        string avaibility = avaibility_regex.First().Value.Trim().Replace('-', ' ');


                                        prices.Add(new()
                                        {
                                            Availability = avaibility,
                                            DeliveryDays = null,
                                            Price = total_price,
                                            Title = title,
                                            Url = totalUrl
                                        });

                                        prices = prices.OrderBy(x => x.Price).ToList();
                                        decimal lowest_price = prices.Select(x => x.Price).First();
                                        mpr.LowestPrice = lowest_price;

                                        mpr.PricesList = prices;

                                        mainPriceResponsesList.Add(mpr);

                                        await ResponseCreator.MakeResponseLog(con: con,
                                            mpr: mpr,
                                            request: request);
                                    }
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
                        stage: 1,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }
            }
            else //If you were unable to generate a search link
            {
                mpr.NoAnswerOrError = true;
                mpr.ErrorMessage = "Unprocessed exception at the formation stage.";
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
                return;
            }
        }
    }
}

