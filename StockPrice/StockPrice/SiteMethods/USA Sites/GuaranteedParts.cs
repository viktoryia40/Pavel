using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StockPrice.Methods;
using AngleSharp;
using Dapper;
using Newtonsoft.Json;
using System.Globalization;

namespace StockPrice.SiteMethods.USA_Sites
{
    public class GuaranteedParts
    {
        private const string Source = "guaranteedparts.com";
        private const string ClassSource = "GuaranteedParts";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.guaranteedparts.com/SRCH.html?Search={search}",
                Source = "Guaranteedparts.com",
                Additional = "📦$24.99"
            };
            var prices = new List<Prices>();

            string? searchResult = null;
            try
            {
                searchResult = CustomHttpClass.GetToString(
                    url: $@"https://www.guaranteedparts.com/SRCH.html?Search={search}",
                    acceptencoding: "none",
                    use_chrome_random_ua: true
                );
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

            var config = Configuration.Default;
            using var context = BrowsingContext.New(config);
            using var document = context.OpenAsync(req => req.Content(searchResult)).Result;

            string totalUrl = null;
            string title = null;

            if (searchResult != null)
            {

                var resultsData = document.QuerySelectorAll(".category-product-name > a");
                
                if (resultsData.Length > 0)
                {
                    totalUrl = resultsData.First().GetAttribute("href");
                    title = resultsData.First().TextContent;
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

            string takenPrice = null;

            try
            {
                var gotPrice = document.QuerySelector(".category-product-price").TextContent;
                var priceRegex = Regex.Match(gotPrice, @"\d+.\d+");
                takenPrice = priceRegex.Value;
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

            if (title != null )
            {
                decimal priceDec = decimal.Parse(takenPrice, CultureInfo.InvariantCulture);

                prices.Add(new()
                {
                    Availability = "In stock",
                    Price = priceDec,
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
