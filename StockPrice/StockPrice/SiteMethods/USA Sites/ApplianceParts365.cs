using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockPrice.Methods;
using StockPrice.SiteMethods.Classes;
using Dapper;
using AngleSharp;
using AngleSharp.Dom;

namespace StockPrice.SiteMethods.USA_Sites
{
    public class ApplianceParts365
    {
        private const string Source = "applianceparts365.com";
        private const string ClassSource = "ApplianceParts365";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://applianceparts365.com/search?adv=true&cid=0&q={search}&sid=true&isc=true",
                Source = "Applianceparts365.com",
                Additional = "📦$9.95"
            };
            var prices = new List<Prices>();

            string? searchResult = null;
            try
            {
                searchResult = CustomHttpClass.GetToString(
                    url: $@"https://applianceparts365.com/instantSearchFor?q={search}",
                    use_chrome_random_ua: true,
                    acceptencoding: "none");
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

            if (searchResult != null)
            {
                
                var jRes = JsonConvert.DeserializeObject<List<ApplianceParts365Classes.ApplianceParts365Result>>(searchResult);
                if (jRes.Count == 0)
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
                    if (jRes.Count > 1)
                    {
                        mpr.MultiChoice = true;
                    }


                    totalUrl = $"https://applianceparts365.com{jRes.First().CustomProperties.Url}";

                }
                

            }

            string response = null;

            if (totalUrl != null)
            {
                try
                {
                    response = CustomHttpClass.GetToString(url: totalUrl,
                        use_chrome_random_ua: true,
                        acceptencoding: "none");

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

            string title = null;
            string availability = null;
            string priceStr = null;
            decimal price = 0;

            if (response != null)
            {
                try
                {

                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(response)).Result;


                    try
                    {
                        title = document.QuerySelector(".product-name > h1").TextContent.Trim();
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
                        availability = document.QuerySelector(".stock > .value").TextContent.Split('-').First().Trim();
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
                        priceStr = document.QuerySelector(@".product-price > [itemprop=""price""]")
                            .GetAttribute("content");
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
                    stage: 0,
                    source: Source,
                    classSource: ClassSource,
                    base64WrongData: null,
                    url: null);
                return;
            }

        }
    }
}
