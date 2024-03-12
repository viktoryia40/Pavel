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
using AngleSharp.Html.Dom;
using StockPrice.Methods;
using AngleSharp;
using AngleSharp.Dom;
using Dapper;
using Newtonsoft.Json;

namespace StockPrice.SiteMethods.USA_Sites
{
    internal class CashWellsCom
    {
        private const string Source = "cashwells.com";
        private const string ClassSource = "CashWellsCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://cashwells.com/?exact_match=yes&keywords={search}+&B1=+Search+",
                Source = "Cashwells.com"
            };
            var prices = new List<Prices>();

            string searchResult = null;
            string totalUrl = null;
            bool isNeedReplace = false;
            try
            {
                searchResult = CustomHttpClass.GetToString(
                    url: $@"https://cashwells.com/?exact_match=yes&keywords={search}+&B1=+Search+",
                    acceptencoding: "none", ignoreErrors: true);
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

            string response = null;
            
            if (searchResult != null)
            {
                try
                {
                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(searchResult)).Result;

                    var checkNothingFoundData = document.QuerySelector(".no_items_found > h2");
                    string checkNothingFoundText = checkNothingFoundData?.TextContent;
                    if (checkNothingFoundText is "There were no items found that match your request.")
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
                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(searchResult)).Result;

                    var checkReplaceElement = document.QuerySelector(".inventory > a");
                    string checkReplaceAttribute = checkReplaceElement.GetAttribute("href");
                    if (checkReplaceAttribute != null)
                    {
                        totalUrl = $"https://cashwells.com/{checkReplaceAttribute}";
                        isNeedReplace = true;
                    }

                }
                catch
                {
                    isNeedReplace = false;
                    totalUrl = $@"https://cashwells.com/?exact_match=yes&keywords={search}+&B1=+Search+";
                    response = searchResult;
                }
            }

            if (isNeedReplace)
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
            string price = null;

            if (response != null && totalUrl != null)
            {

                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(response)).Result;


                try
                {
                    var inventoryElement = document.QuerySelector(".inv_header");
                    string inventoryText = inventoryElement.TextContent;
                    var spl = inventoryText.Split(':');
                    if (spl.Count() > 1)
                    {
                        string count = spl[1];
                        int counInt = int.Parse(count);
                        if (counInt < 1)
                        {
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
                        throw new Exception("Can't found inventory element.");
                    }
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
                    var productTitleElement = document.QuerySelector(".product-title");
                    title = productTitleElement.TextContent;
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
                    var priceElement = document.QuerySelector(".product-price");
                    string priceNotParsed = priceElement.TextContent;
                    var priceRegex = Regex.Matches(priceNotParsed, "\\d{1,}\\.\\d{1,2}");
                    if (priceRegex.Any())
                    {
                        price = priceRegex.First().Value;
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
                    var availabilityElement = document.QuerySelector("meta[itemprop='availability']");
                    string hrefData = availabilityElement.GetAttribute("href");
                    Uri href = new Uri(hrefData);
                    string path = href.AbsolutePath;
                    availability =
                        string.Join("",
                            path.Substring(1).ToCharArray().Select(x => char.IsUpper(x) ? " " + x : "" + x).ToList());

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

            if (title != null && price != null && availability != null)
            {
                decimal priceDec = Math.Round(
                    decimal.Parse(price, CultureInfo.InvariantCulture),
                    2);

                prices.Add(new()
                {
                    Availability = availability,
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
