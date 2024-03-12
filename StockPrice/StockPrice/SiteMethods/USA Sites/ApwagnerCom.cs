using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;

namespace StockPrice.SiteMethods.USA_Sites
{
    public sealed class ApwagnerCom
    {
        private const string Source = "apwagner.com";
        private const string ClassSource = "ApwagnerCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = @$"https://www.apwagner.com/search/{search}", 
                Source = "Apwagner.com"
            };
            var prices = new List<Prices>();

            string redirect;
            try
            {
                redirect = CustomHttpClass.CheckRedirectGet($"https://www.apwagner.com/search/{search}");
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
                totalUrl = $"https://www.apwagner.com{redirect}";
            }
            else
            {
                string searchResult = null;
                try
                {
                    searchResult = CustomHttpClass.GetToString($"https://www.apwagner.com/search/{search}", acceptencoding: "none");
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

            string response = null;

            if (totalUrl != null)
            {
                try
                {
                    response = CustomHttpClass.GetToString(totalUrl, acceptencoding: "none");

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

            string title = null;
            string availability = null;
            string priceStr = null;
            decimal price = 0;


            //Check RPL
            if (response != null)
            {
                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(response)).Result;

                try
                {
                    string availabilityTest = document.QuerySelector(".pdetail_availability").TextContent.Trim();

                    if (availabilityTest.Equals("Out Of Stock"))
                    {
                        bool needReturn = true;
                        try
                        {
                            var isHaveRplSelector = document.QuerySelector(".product_details > h1 > a");
                            if (isHaveRplSelector != null)
                            {
                                string newHref = isHaveRplSelector.GetAttribute("href").Trim();
                                try
                                {
                                    totalUrl = $"https://www.apwagner.com{newHref}";
                                    response = CustomHttpClass.GetToString(totalUrl, acceptencoding: "none");
                                }
                                catch (Exception ex)
                                {
                                    await ResponseCreator.MakeErrorLog(con: con,
                                        mpr: mpr,
                                        mainPriceResponsesList: mainPriceResponsesList,
                                        request: request,
                                        base64ErrorData: ex.Message.ToString(),
                                        stage: 10,
                                        source: Source,
                                        classSource: ClassSource,
                                        base64WrongData: null,
                                        url: null);
                                }
                            }
                        }
                        catch
                        {
                            // ignored
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
                        stage: 11,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);

                    return;
                }

            }

            if (response != null)
            {
                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(response)).Result;

                try
                {
                    title = document.QuerySelector(".product_details > h1").TextContent.Trim();
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message.ToString(),
                        stage: 3,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);

                    return;
                }

                try
                {
                    availability = document.QuerySelector(".pdetail_availability").TextContent.Trim();

                    if (availability.Equals("Out Of Stock"))
                    {
                       
                        
                            mpr.NothingFoundOrOutOfStock = true;
                            mainPriceResponsesList.Add(mpr);

                            await ResponseCreator.MakeResponseLog(con: con,
                                mpr: mpr,
                                request: request);
                            return;
                        
                        
                    }
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message.ToString(),
                        stage: 4,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);

                    return;
                }

                try
                {
                    priceStr = document.QuerySelector(@"[itemprop=""price""]").TextContent;
                    var priceReg = Regex.Match(priceStr, @"\d+.\d+");
                    price = decimal.Parse(priceReg.Value, CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message.ToString(),
                        stage: 5,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);

                    return;
                }
            }


            if (title != null && availability != null && priceStr != null)
            {
                prices.Add(new Prices
                {
                    Availability = availability,
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
            else
            {
                await ResponseCreator.MakeErrorLog(con: con,
                    mpr: mpr,
                    mainPriceResponsesList: mainPriceResponsesList,
                    request: request,
                    base64ErrorData: "Not parsed data",
                    stage: 6,
                    source: Source,
                    classSource: ClassSource,
                    base64WrongData: null,
                    url: null);
                return;
            }
        }
    }
}
