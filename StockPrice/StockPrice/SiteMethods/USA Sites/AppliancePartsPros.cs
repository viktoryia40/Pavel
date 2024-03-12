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
using StockPrice.Methods;
using AngleSharp;
using AngleSharp.Dom;
using System.Text.RegularExpressions;
using Dapper;
using Newtonsoft.Json;

namespace StockPrice.SiteMethods.USA_Sites
{
    public class AppliancePartsPros
    {
        private const string Source = "appliancepartspros.com";
        private const string ClassSource = "AppliancePartsPros";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.appliancepartspros.com/search.aspx?q={search}",
                Source = "Appliancepartspros.com",
                Additional = "📦$11.00"
            };
            var prices = new List<Prices>();

            string? redirect = null;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(
                    url: $"https://www.appliancepartspros.com/search.aspx?q={search}",
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

            string total_url = null;

            if (redirect != null)
            {
                total_url = $"https://www.appliancepartspros.com{redirect}";
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

            string response = null;

            if (total_url != null)
            {
                response = CustomHttpClass.GetToString(url: total_url,
                    acceptencoding: "none",
                    use_chrome_random_ua: true);
            }

            string title = null;
            string priceStr = null;
            decimal price = 0;
            string availability = null;

            
            
                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(response)).Result;


                try
                {
                    title = document.QuerySelector(".col-1-2-1 .h2").TextContent;
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

                try
                {
                    priceStr = document.QuerySelector("[itemprop=\"price\"]").GetAttribute("content");
                    price = decimal.Parse(priceStr, CultureInfo.InvariantCulture);
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
                    var availabilityData =
                        document.QuerySelectorAll(@"[itemprop=""offers""] > .pdct-add-inf > div > p > span");
                    availability = availabilityData.Last().TextContent;
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
            


            if (title != null && availability != null && priceStr != null)
            {
                

                prices.Add(new()
                {
                    Availability = availability,
                    Price = price,
                    Title = title,
                    Url = total_url
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
                    base64ErrorData: "Not parsed data",
                    stage: 4,
                    source: Source,
                    classSource: ClassSource,
                    base64WrongData: null,
                    url: null);
                
                return;
            }
        }
    }
}
