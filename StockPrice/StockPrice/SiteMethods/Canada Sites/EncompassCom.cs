using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;


namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class EncompassCom
    {
        private const string Source = "encompass.com";
        private const string ClassSource = "EncompassCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request.ToUpper();

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            await using var con = new MySqlConnection(cs);

            await con.OpenAsync();
            var settings = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={request.ChatID}");
            await con.CloseAsync();


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://encompass.com/search?searchTerm={search}", Source = "Encompass.com" };
            var prices = new List<Prices>();


            string redirect = null;



            try
            {

                redirect = CustomHttpClass.CheckRedirectGet(@$"https://encompass.com/search?searchTerm={search}",
                    acceptencoding: "none",
                    use_chrome_random_ua: true);

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


            List <string> totalUrlList = new();

            if (redirect != null)
            {

                totalUrlList.Add(@$"https://encompass.com{redirect}");
            }
            else
            {


                mpr.MultiChoice = true;

                string searchResult = null;

                try
                {
                    searchResult = CustomHttpClass.GetToString(@$"https://encompass.com/search?searchTerm={search}",
                    acceptencoding: "none",
                    use_chrome_random_ua: true);
                }
                catch(Exception ex)
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

                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(searchResult)).Result;

                    try
                    {
                        var rows = document.QuerySelectorAll("tbody > tr");

                        foreach (var row in rows)
                        {
                            string href = row.QuerySelector(@"td > a").GetAttribute("href");
                            try
                            {
                                var inStockCheck = row.QuerySelector(@"td.text-center > span");
                                totalUrlList.Add(@$"https://encompass.com{href}");
                            }
                            catch
                            {
                                continue;
                            }
                        }

                    }
                    catch
                    {
                        mpr.PricesList = prices;
                        mpr.NothingFoundOrOutOfStock = true;

                        mainPriceResponsesList.Add(mpr);

                        await ResponseCreator.MakeResponseLog(con: con,
                            mpr: mpr,
                            request: request);
                        return;
                    }


                }
                else
                {
                    mpr.PricesList = prices;
                    mpr.NothingFoundOrOutOfStock = true;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }
            }


            if (totalUrlList.Count > 0)
            {

                foreach (var total_url in totalUrlList)
                {
                    string result = null;

                    try
                    {
                        result = CustomHttpClass.GetToString(total_url,
                        acceptencoding: "none",
                        use_chrome_random_ua: true);
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
                        var config = Configuration.Default;
                        using var context = BrowsingContext.New(config);
                        using var document = context.OpenAsync(req => req.Content(result)).Result;

                        string availability = null;
                        try
                        {
                            IElement availabilityElement = document.QuerySelector(@"[aria-label='Availability By Location'] > span");
                            availability = availabilityElement.TextContent;
                        }
                        catch
                        {
                            mpr.NothingFoundOrOutOfStock = true;
                            mainPriceResponsesList.Add(mpr);

                            await ResponseCreator.MakeResponseLog(con: con,
                                mpr: mpr,
                                request: request);
                            return;

                        }


                            
                        string title = document.QuerySelector(@"h1").TextContent;
                        string stringPrice = document.QuerySelector(@"p.price > span.amount").TextContent;

                        prices.Add(new()
                        {
                            Title = title.Trim(),
                            Availability = availability.Trim(),
                            DoublePrice = $@"${stringPrice.Trim()} usd",
                            Url = total_url,
                            Price = decimal.Parse(stringPrice.Trim().Replace(",", "."), CultureInfo.InvariantCulture)

                        });
                        
                    }
                    catch(Exception ex)
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

                }


                if (prices.Count > 0)
                {
                    prices = new() { prices.OrderBy(x => x.Price).ToList().First() };
                    mpr.LowestPrice = prices.First().Price;
                    prices.First().Price = 0;
                    mpr.PricesList = prices;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }
                else
                {
                    mpr.PricesList = prices;
                    mpr.NothingFoundOrOutOfStock = true;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }
            }

            else
            {
                mpr.PricesList = prices;
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
