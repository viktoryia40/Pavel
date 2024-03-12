using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StockPrice.Methods;
using AngleSharp;
using AngleSharp.Dom;
using Dapper;
using Newtonsoft.Json;

namespace StockPrice.SiteMethods.USA_Sites
{
    public class Partsdr
    {
        private const string Source = "partsdr.com";
        private const string ClassSource = "Partsdr";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://partsdr.com/search?query={search}",
                Source = "Partsdr.com"
                //Additional = "📦$9.99"
            };
            var prices = new List<Prices>();

            string? redirect = null;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(url: $"https://partsdr.com/search?query={search}",
                    acceptencoding: "none",
                    use_chrome_random_ua: true);
            }
            catch (Exception ex)
            {
                await ResponseCreator.MakeErrorLog(con: con,
                    mpr: mpr,
                    mainPriceResponsesList: mainPriceResponsesList,
                    request: request,
                    base64ErrorData: ex.Message,
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
                totalUrl = $"https://partsdr.com{redirect}";
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

            string resposne = null;

            if (totalUrl != null)
            {
                try
                {
                    resposne = CustomHttpClass.GetToString(url: totalUrl,
                        acceptencoding: "none",
                        use_chrome_random_ua: true);
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message,
                        stage: 1,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }
                
            }

            if (resposne != null)
            {
                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(resposne)).Result;

                string title = null;
                string price = null;
                string availability = null;

                try
                {
                    title = document.QuerySelector(".product-form > h1").TextContent.Trim();
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message,
                        stage: 2,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }

                try
                {
                    price = document.QuerySelector(".main").TextContent;
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message,
                        stage: 3,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }

                try
                {
                    availability = document.QuerySelector(".notice").TextContent.Trim();
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message,
                        stage: 4,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }

                decimal deliveryPrice = 9.99M;
                var priceRegex = Regex.Matches(price, @"\d+.\d+");
                if (priceRegex.Count() > 0)
                {
                    decimal priceDecimal = decimal.Parse(priceRegex.First().Value, CultureInfo.InvariantCulture);

                    prices.Add(new()
                    {
                        Availability = availability,
                        Price = priceDecimal,
                        Title = title,
                        Url = totalUrl,
                        DeliveryPrice = deliveryPrice
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
                        base64ErrorData: "Can't parse price",
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
}
