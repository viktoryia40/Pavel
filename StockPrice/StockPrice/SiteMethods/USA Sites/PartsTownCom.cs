using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.CF_works;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using AngleSharp;
using AngleSharp.Dom;
using System.Globalization;

namespace StockPrice.SiteMethods.USA_Sites
{
    internal class PartsTownCom
    {
        private const string Source = "partstown.com";
        private const string ClassSource = "PartsTownCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.partstown.com/parts?q={request.Request}",
                Source = "Partstown.com"
            };
            var prices = new List<Prices>();

            int cfDataId = -1;


            // Adding a request for CF in DB
            try
            {
                cfDataId = await CfRequestMaker.MakeCfRequest(mpr.SearchUrl);
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

            string searchResult = null;

            try
            {
                searchResult = await CfRequestGetting.GetCfRequestResponse(cfDataId);
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

                string totalUrl = null;
                string title = null;
                string availability = null;
                string priceText = null;


                //Check no results
                try
                {
                    var noResultDataSelector = document.QuerySelector(".no-search__headline");
                    string noResultDataContent = noResultDataSelector.TextContent;
                    if (noResultDataContent != null && !string.IsNullOrEmpty(noResultDataContent))
                    {
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

                //Check all results
                try
                {
                    var countSearchData = document.QuerySelector(".product-tabs-item__count");
                    string countSearch = countSearchData.TextContent;
                    if (countSearch != null && !string.IsNullOrEmpty(countSearch)) //MultiChoice accepted
                    {
                        mpr.MultiChoice = true;


                        try
                        {
                            var nameSelector = document.QuerySelector(
                                ".name > a");
                            title = nameSelector.TextContent.Trim();


                            

                        }
                        catch
                        {
                            //ignored
                        }

                        try
                        {
                            var priceSelector = document.QuerySelector(
                                ".product__listing > .product-item > .js-multi-product-item-plp > .details > .price-panel > .price > span");
                            priceText = Regex.Match(priceSelector.TextContent, @"\d{1,}.\d{1,}").Value.Trim();

                        }
                        catch 
                        {
                           //ignored
                        }

                        try
                        {
                            var availabilitySelector = document.QuerySelector(
                                ".status-description");
                            availability = availabilitySelector.TextContent.Trim();
                        }
                        catch
                        {
                            //ignored
                        }

                        try
                        {
                            var totalUrlSelector = document.QuerySelector(
                                ".name > a");
                            totalUrl = "https://www.partstown.com" + totalUrlSelector.GetAttribute("href").Trim();
                        }
                        catch
                        {
                            //ignored
                        }


                    }
                }
                catch // If not multi choice
                {
                    // span[itemprop='price'] - pricetext
                    // .name - title
                    // .js-pdp-main-qty-value - quantity
                    // meta[name='be:norm_url'] - url
                    // span[itemprop='price'] - pricetext
                    // .product-ship-type__label__in-stock-status - inStock status

                    try
                    {
                        var nameSelector = document.QuerySelector(
                            ".name");
                        title = nameSelector.TextContent.Trim();




                    }
                    catch
                    {
                        //ignored
                    }

                    try
                    {
                        var priceSelector = document.QuerySelector(
                            "span[itemprop='price']");
                        priceText = priceSelector.TextContent.Trim();
                    }
                    catch
                    {
                        //ignored
                    }

                    try
                    {
                        var availabilitySelector = document.QuerySelector(
                            ".product-ship-type__label__in-stock-status");
                        availability = availabilitySelector.TextContent.Trim();
                    }
                    catch
                    {
                        try
                        {
                            var availabilitySelector = document.QuerySelector(
                                ".js-product-ship-type__label__time-container-MKT");
                            availability = availabilitySelector.TextContent.Trim();
                        }
                        catch 
                        {
                            //ignored
                        }
                    }

                    try
                    {
                        var totalUrlSelector = document.QuerySelector(
                            "meta[name='be:norm_url']");
                        totalUrl = totalUrlSelector.GetAttribute("content").Trim();
                    }
                    catch
                    {
                        //ignored
                    }

                    try
                    {
                        var quantitySelector = document.QuerySelector(
                            "meta[name='be:norm_url']");
                        string quantityText = quantitySelector.TextContent;

                        if (int.TryParse(quantityText, out int quantityCount))
                        {
                            if (quantityCount <= 0)
                            {
                                mpr.NothingFoundOrOutOfStock = true;
                                mainPriceResponsesList.Add(mpr);

                                con.Open();
                                string type = "Price";
                                string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(mpr));
                                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                                con.Close();

                                return;
                            }
                        }
                    }
                    catch
                    {
                        //ignored
                    }

                }

                if (title != null)
                {
                    decimal price_dec = decimal.Parse(priceText, CultureInfo.InvariantCulture);

                    prices.Add(new()
                    {
                        Availability = availability,
                        Price = price_dec,
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
