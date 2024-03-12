using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Dapper;
using Google.Apis.Sheets.v4.Data;
using Leaf.xNet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;

namespace StockPrice.SiteMethods.USA_Sites
{
    internal class DlPartscoCom
    {
        private const string Source = "dlpartsco.com";
        private const string ClassSource = "DlPartscoCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.dlpartsco.com/",
                Source = "Dlpartsco.com",
                EndAdditional = "(main page)"
            };
            var prices = new List<Prices>();

            string searchResult = null;
            try
            {
                RequestParams data = new()
                {
                    ["search"] = $"{search}",
                    ["type"] = "fulltext",
                    ["x"] = "8",
                    ["y"] = "11"
                };
                searchResult = CustomHttpClass.PostToString($@"https://www.dlpartsco.com/search", 
                    data: data,
                    acceptencoding:"none");

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

            if (searchResult != null)
            {
                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(searchResult)).Result;

                string title = null;
                string availability = null;
                string price = null;

                try
                {
                    var nothingFoundTest = document.QuerySelector(".content > strong");
                    if (nothingFoundTest == null) throw new Exception("founded");
                        string nothingFoundText = nothingFoundTest.TextContent;
                    if (nothingFoundText is "No Results Found")
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
                catch 
                {
                    // ignored
                }
               

                try
                {
                    var titleData = document.QuerySelector("input[name*='dscs']");
                    title = titleData.GetAttribute("value");
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

                try
                {
                    var priceData = document.QuerySelector("input[name*='prices']");
                    price = priceData.GetAttribute("value");
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
                    var availabilityData = document.QuerySelector(".productLocations > tbody > tr > td > table > tbody > tr > td > img");
                    availability = availabilityData.GetAttribute("alt");
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

                if (title != null && availability != null && price != null)
                {
                    decimal priceDec = Math.Round(
                        decimal.Parse(price, CultureInfo.InvariantCulture),
                        2);

                    prices.Add(new Prices
                    {
                        Availability = availability,
                        Price = priceDec,
                        Title = title,
                        Url = "https://www.dlpartsco.com/search"
                    });

                    prices = prices.OrderBy(x => x.Price).ToList();
                    decimal lowestPrice = prices.Select(x => x.Price).First();
                    mpr.LowestPrice = lowestPrice;

                    // Add '(main page)'
                    //prices[0].Price = 0;
                    //prices[0].DoublePrice = $@"{priceDec.ToString(CultureInfo.InvariantCulture)} (main page)";

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

        }
    }
}
