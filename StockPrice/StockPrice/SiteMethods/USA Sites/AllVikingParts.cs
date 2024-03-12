using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using AngleSharp;
using AngleSharp.Dom;

namespace StockPrice.SiteMethods.USA_Sites
{
    internal class AllVikingParts
    {
        private const string Source = "allvikingparts.com";
        private const string ClassSource = "AllVikingParts";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.allvikingparts.com/searchresults.asp?Search={search}",
                Source = "Allvikingparts.com",
                Additional = "Free Shipping"
            };
            var prices = new List<Prices>();

            string? searchResult = null;
            try
            {
                searchResult = CustomHttpClass.GetToString(
                    url: $"https://www.allvikingparts.com/searchresults.asp?Search={search}",
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

            string title = null;
            string availability = null;
            string priceStr = null;


            if (searchResult != null)
            {
               

                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(searchResult)).Result;


                try
                {
                    var foundDataSelector = document.QuerySelector(".matching_results_text");
                    string foundDataText = foundDataSelector.TextContent;
                    string foundDataCountText = Regex.Match(foundDataText, @"\d{1,}").Value;
                    if (int.TryParse(foundDataCountText, out int count))
                    {
                        if (count == 0)
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
                catch { /*ignored*/ }


                try
                {
                    var titleSelector = document.QuerySelector(@".v-product > p");
                    title = titleSelector.TextContent.Trim();

                } catch { /*ignored*/ }

                try
                {
                    var urlSelector = document.QuerySelector(@".v-product > a[class='v-product__title productnamecolor colors_productname']");
                    totalUrl = urlSelector.GetAttribute("href").Trim();

                }
                catch { /*ignored*/ }

                try
                {
                    var priceSelector = document.QuerySelector(@".product_productprice");
                    priceStr = Regex.Match(priceSelector.TextContent, @"\d{1,}.\d{1,}").Value.Trim();

                }
                catch { /*ignored*/ }

                availability = "In Stock";

            }

            if (title != null && availability != null && priceStr != null)
            {
                decimal price = decimal.Parse(priceStr, CultureInfo.InvariantCulture);


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
