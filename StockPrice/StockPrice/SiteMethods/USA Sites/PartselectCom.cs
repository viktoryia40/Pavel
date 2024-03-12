using System.Globalization;
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
    public sealed class PartselectCom
    {
        private const string Source = "allvikingparts.com";
        private const string ClassSource = "AllVikingParts";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            

            var mpr = new MainPriceResponse()
            {
                SearchUrl = @$"https://www.partselect.com/Search.ashx?SearchTerm={search}&SearchMethod=standard", 
                Source = "Partselect.com", 
                Additional = "📦$11.45"
            };
            var prices = new List<Prices>();

            string redirect;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.partselect.com/Search.ashx?SearchTerm={search}&SearchMethod=standard");

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
                if (redirect.Contains("Search-Suggestions.aspx?") || redirect.Contains("PartSearchResult.aspx?")) //If you've been bounced to a multicarrier.
                {
                    string searchResult = null;
                    mpr.NothingFoundOrOutOfStock = true;
                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);

                    return;


                }
                else
                    totalUrl = redirect;
            }
            else//If there is no redirect at all - error.
            {
                mpr.NoAnswerOrError = true;
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);

                return;
            }

            if (totalUrl != null)
            {
                string response = null;

                try
                {
                    response = CustomHttpClass.GetToString(totalUrl);

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

                if (response != null)
                {
                    string title = null;
                    string availability = null;
                    string priceStr = null;
                    decimal price = 0;

                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(response)).Result;

                    try
                    {
                        title = document.QuerySelector(".title-lg").TextContent;

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

                    try
                    {
                        availability = document.QuerySelector(@"[itemprop=""availability""]").TextContent.Trim();
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
                        priceStr = document.QuerySelector("#mainAddToCart .js-partPrice").TextContent;
                        price = decimal.Parse(priceStr, CultureInfo.InvariantCulture);
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



                    if (title != null && availability != null && priceStr != null)
                    {
                        prices.Add(new()
                        {
                            Availability = availability,
                            Price = price,
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
                        return;
                    }
                    else
                    {
                        await ResponseCreator.MakeErrorLog(con: con,
                            mpr: mpr,
                            mainPriceResponsesList: mainPriceResponsesList,
                            request: request,
                            base64ErrorData: "Some data not parsed",
                            stage: 5,
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
}
