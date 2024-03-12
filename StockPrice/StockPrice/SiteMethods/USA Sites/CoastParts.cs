using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using Dapper;
using Google.Apis.Sheets.v4.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;

namespace StockPrice.SiteMethods.USA_Sites
{
    public class CoastParts
    {
        private const string Source = "coastparts.com";
        private const string ClassSource = "CoastParts";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.coastparts.com/search?q={search}", Source = "Coastparts.com"
            };
            var prices = new List<Prices>();

            string searchResult;
            try
            {
                searchResult = CustomHttpClass.CheckRedirectGet($@"https://www.coastparts.com/search?q={search}");
            }
            catch(Exception ex)
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

            if (searchResult != null) totalUrl = $@"https://www.coastparts.com{searchResult}";

            else
            {
                try
                {
                    searchResult = CustomHttpClass.GetToString($@"https://www.coastparts.com/search?q={search}", acceptencoding: "none");
                }
                catch (Exception ex)
                {
                    // ignored
                }

                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(searchResult)).Result;


                // Checking for missing results
                try
                {
                    var nothingFoundSelector = document.QuerySelector(".contentMain .container > h2");
                    if (nothingFoundSelector is { TextContent: "No results found" })
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



                // Making a sample of all the output results
                try
                {
                    var resultsCollection = document.QuerySelectorAll(".space20");

                    foreach (var resultElement in resultsCollection)
                    {
                        var titleElement = resultElement.QuerySelector(".product_box > .product_img > .product_txt > h3");

                        if (titleElement != null && titleElement.TextContent.Contains(search))
                        {
                            var aElement = titleElement.QuerySelector("a");
                            totalUrl = aElement.GetAttribute("href");
                            break;
                        }
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


            {
                string? response;
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

                if (response != null)
                {
                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(response)).Result;

                    //.centerAlign

                    string title = null;
                    try
                    {
                        var title_data = document.QuerySelector(".centerAlign");
                        title = title_data.TextContent;

                    }
                    catch(Exception ex)
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
                    }

                    string price = null;
                    try
                    {
                        var price_data = document.QuerySelector(".text-primary");
                        string taken_price = price_data.TextContent;
                        price = Regex.Match(taken_price, @"\d+.\d+").Value;
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
                    }

                    string availability = null;

                    try
                    {
                        var availabilityData = document.QuerySelector(".productStock > span > strong");
                        try
                        {

                            availability = availabilityData.TextContent;
                        }
                        catch
                        {
                            availability = "Special Order";
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
                    }

                    


                    try
                    {
                        List<string> locations = new List<string>();

                        var locationsNames = document.QuerySelectorAll(".list-group-item > a");
                        var locationsCount = document.QuerySelectorAll(".list-group-item > span");
                        for (int i = 0; i < locationsNames.Length; i++)
                        {
                            if (int.TryParse(locationsCount[i].TextContent, out int count))
                            {
                                if (count != 0)
                                {
                                    string taken_name = locationsNames[i].TextContent;
                                    locations.Add($@"{taken_name} {count}");
                                }
                            }
                        }

                        if (locations.Count > 0) mpr.Locations = locations;
                    }
                    catch
                    {
                        // ignored
                    }

                    if (title != null && availability != null)
                    {
                        decimal price_dec = decimal.Parse(price, CultureInfo.InvariantCulture);

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


                }


                else
                {
                    mpr.NoAnswerOrError = true;
                    mpr.ErrorMessage = "response is null or empty";
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
