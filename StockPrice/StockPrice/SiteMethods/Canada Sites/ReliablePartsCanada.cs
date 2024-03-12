using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.Methods.Authorization;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.Classes;
using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;


namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class ReliablePartsCanada
    {
        private const string Source = "reliableparts.net";
        private const string ClassSource = "ReliablePartsCanada";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://reliableparts.net/ca/en/content/#/search/{search}", Source = "Reliableparts.net", Additional = "📦$10.95" };
            var prices = new List<Prices>();

            var authHeader = new List<CustomHttpAdditionals.Headers>() {
                new(){ Name = "Authorization", Value = $"Bearer {ReliablePartsAuthCanada.ReliablePartsBearerToken}" }
            };

            string searchResult = null;
            try
            {
                searchResult = CustomHttpClass.GetToString($@"https://prodapi.reliableparts.net/ca/navapp/v1/search/modelProduct?q={search}", headers: authHeader);
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
                var searchResultDeserialized = JsonConvert.DeserializeObject<ReliablePartsClasses.Results>(searchResult);

                if (searchResultDeserialized.Products.Count > 0)
                {
                    if (searchResultDeserialized.Products.Count > 1) mpr.MultiChoice = true;
                    var firstProduct = searchResultDeserialized.Products.First();

                    string selectedName = firstProduct.Name;
                    string selectedManufacturer = firstProduct.Manufacturer;
                    string selectedProductNumber = firstProduct.ProductNumber;

                    string productDetails = null;

                    try
                    {
                        productDetails = CustomHttpClass.GetToString($@"https://prodapi.reliableparts.net/ca/navapp/v1/product/detail/{selectedName}?mfc={selectedManufacturer}", headers: authHeader);

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

                    if (productDetails != null)
                    {
                        var productDetailDeserialized = JsonConvert.DeserializeObject<ReliablePartsClasses.ProductDetail>(productDetails);


                        decimal retail_price = 0;
                        decimal your_price = 0;
                        string title = null;
                        string availability = null;

                        title = productDetailDeserialized.Description;
                        retail_price = decimal.Parse(productDetailDeserialized.RetailPrice, CultureInfo.InvariantCulture);
                        your_price = decimal.Parse(productDetailDeserialized.PartnerPrice, CultureInfo.InvariantCulture);

                        if (productDetailDeserialized.InStock) availability = "In Stock";
                        else if (productDetailDeserialized.SpecialOrder) availability = "Special Order";
                        else if (!productDetailDeserialized.InStock && !productDetailDeserialized.SpecialOrder)
                        {
                            mpr.NothingFoundOrOutOfStock = true;

                            await ResponseCreator.MakeResponseLog(con: con,
                                mpr: mpr,
                                request: request);

                            return;
                        }

                        prices.Add(new()
                        {
                            Availability = availability,
                            DoublePrice = $@"${your_price.ToString().Replace(',', '.')} / ${retail_price.ToString().Replace(',', '.')}",
                            Title = title,
                            Url = $@"https://reliableparts.net/ca/en/content/#/part/{selectedProductNumber}"
                        });
                        mpr.LowestPrice = Math.Min(your_price, retail_price);

                        if (productDetailDeserialized.OemProductManufacturer != null && productDetailDeserialized.OemProductNumber != null)
                        {
                            mpr.Source = "Reliableparts.net->[NON OEM]";

                            MakeOEMorAlternative(mainPriceResponsesList, productDetailDeserialized, request);
                        }
                        else if (productDetailDeserialized.AlternateProductNumber != null && productDetailDeserialized.AlternateProductManufacturer != null)
                        {
                            mpr.Source = "Reliableparts.net->[OEM]";

                            MakeOEMorAlternative(mainPriceResponsesList, productDetailDeserialized, request);
                        }

                        if (productDetailDeserialized.Warehouses.Count > 0)
                        {
                            List<string> locations = new();
                            foreach (var warehouseData in productDetailDeserialized.Warehouses)
                            {
                                string locationName = warehouseData.Description.Replace("PICK UP ONLY", "pickup").ToLower();
                                locationName = char.ToUpper(locationName[0]) + locationName[1..];
                                locations.Add($"{locationName} ({warehouseData.Quantity})");
                            }
                            if (locations.Count > 0) mpr.Locations = locations;
                        }

                        MakeAlternativeLinkFromReliablePartsCa(request, productDetailDeserialized, mpr);
                        mpr.PricesList = prices;
                        mainPriceResponsesList.Add(mpr);

                        await ResponseCreator.MakeResponseLog(con: con,
                            mpr: mpr,
                            request: request);
                    }
                }
            }

        }

        private static async void MakeOEMorAlternative(List<MainPriceResponse> mainPriceResponsesList, ReliablePartsClasses.ProductDetail sourceProduct, DatabaseTotalResults request)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            string search = request.Request;

            var mpr = new MainPriceResponse() { SearchUrl = @$"https://reliableparts.net/ca/en/content/#/search/{search}", Source = "Reliableparts.net", Additional = "📦$10.95" };
            var prices = new List<Prices>();
            var authHeader = new List<CustomHttpAdditionals.Headers>() 
            {
                new()
                {
                    Name = "Authorization", 
                    Value = $"Bearer {ReliablePartsAuthCanada.ReliablePartsBearerToken}"
                }
            };

            if (sourceProduct.OemProductManufacturer != null)
            {
                mpr.Source = "Reliableparts.net->[OEM]";
            }
            else
            {
                mpr.Source = "Reliableparts.net->[NON OEM]";
            }

            string target_manifatcure;
            string target_number;
            if (sourceProduct.OemProductManufacturer != null)
            {
                target_manifatcure = sourceProduct.OemProductManufacturer;
                target_number = sourceProduct.OemProductNumber;
            }
            else
            {
                target_manifatcure = sourceProduct.AlternateProductManufacturer;
                target_number = sourceProduct.AlternateProductNumber;
            }

            string product_details = null;

            try
            {
                product_details = CustomHttpClass.GetToString($@"https://prodapi.reliableparts.net/ca/navapp/v1/product/detail/{target_number}?mfc={target_manifatcure}", headers: authHeader);

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

            if (product_details != null)
            {
                var productDetailDeserialized = JsonConvert.DeserializeObject<ReliablePartsClasses.ProductDetail>(product_details);


                decimal retailPrice = 0;
                decimal yourPrice = 0;
                string title = null;
                string availability = null;

                title = productDetailDeserialized.Description;
                retailPrice = decimal.Parse(productDetailDeserialized.RetailPrice, CultureInfo.InvariantCulture);
                yourPrice = decimal.Parse(productDetailDeserialized.PartnerPrice, CultureInfo.InvariantCulture);

                if (productDetailDeserialized.InStock) availability = "In Stock";
                else if (productDetailDeserialized.SpecialOrder) availability = "Special Order";
                else if (!productDetailDeserialized.InStock && !productDetailDeserialized.SpecialOrder)
                {
                    mpr.NothingFoundOrOutOfStock = true;
                    mainPriceResponsesList.Add(mpr);
                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }

                prices.Add(new() { Availability = availability, DoublePrice = $@"${yourPrice.ToString().Replace(',', '.')} / ${retailPrice.ToString().Replace(',', '.')}", Title = title, Url = $@"https://reliableparts.net/ca/en/content/#/part/{target_number}" });
                mpr.LowestPrice = Math.Min(yourPrice, retailPrice);
                mpr.PricesList = prices;

                MakeAlternativeLinkFromReliablePartsCa(request, productDetailDeserialized, mpr);
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);

            }

        }
        
        public static void MakeAlternativeLinkFromReliablePartsCa (DatabaseTotalResults request, ReliablePartsClasses.ProductDetail sourceProduct, MainPriceResponse mpr)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);

            bool isAlternate = false;

            //mpr.SearchUrl = @$"https://www.reliableparts.ca/search?q={search}";
            var prices = new List<Prices>();


            string redirect = null;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.reliableparts.ca/search?q={search}");

            }
            catch (Exception ex)
            {

                con.Open();
                var error_log = new DatabaseUnregisteredResponses()
                {
                    RequestId = request.ID,
                    RequestText = request.Request,
                    Base64wrongData = null,
                    Base64errorData = MySqlHelper.EscapeString(ex.Message.ToString()),
                    Comment = "STAGE 0-1",
                    Url = null,
                    Source = "Reliableparts.com"

                };
                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                con.Close();
                Console.WriteLine($"Error on Reliableparts.com registered. STAGE - 0-1    Request: {search}");
                

                return;
            }

            string total_url = null;

            if (redirect != null)
            {
                total_url = $@"https://www.reliableparts.ca{redirect}";
            }
            else //issue analysis.
            {
                if (sourceProduct.OemProductManufacturer != null)
                {
                    isAlternate = true;
                }
                else
                {
                    isAlternate = false;
                }

                string search_result = null;

                try
                {
                    search_result = CustomHttpClass.GetToString(@$"https://www.reliableparts.ca/search?q={search}", acceptencoding: "none");
                }
                catch(Exception ex)
                {
                    con.Open();
                    var error_log = new DatabaseUnregisteredResponses()
                    {
                        RequestId = request.ID,
                        RequestText = request.Request,
                        Base64wrongData = null,
                        Base64errorData = MySqlHelper.EscapeString(ex.Message.ToString()),
                        Comment = "STAGE 1-1",
                        Url = null,
                        Source = "Reliableparts.com"

                    };
                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{error_log.RequestId}', '{error_log.RequestText}', '{error_log.Source}', '{error_log.Base64wrongData}', '{error_log.Base64errorData}', '{error_log.Comment}', '{error_log.Url}');");
                    con.Close();
                    Console.WriteLine($"Error on Reliableparts.com registered. STAGE - 1-1    Request: {search}");
                    return;
                }

                if (search_result != null)
                {

                    var config = Configuration.Default;
                    using var context = BrowsingContext.New(config);
                    using var document = context.OpenAsync(req => req.Content(search_result)).Result;

                    //ul.srp-results >  li.s-item.s-item__pl-on-bottom > div.s-item__wrapper > div.s-item__info

                    try
                    {
                        IHtmlCollection<IElement> results = document.QuerySelectorAll(@"div.box-bottom > h2 > a");
                        if (isAlternate)
                        {
                            foreach(var result in results)
                            {
                                if (result.TextContent.Contains("Alternate"))
                                {
                                    total_url = result.GetAttribute("href");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var result in results)
                            {
                                if (!result.TextContent.Contains("Alternate"))
                                {
                                    total_url = result.GetAttribute("href");
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }


                }
                
                

               
            }

            if (total_url != null)
            {
                mpr.AlternativeLink = total_url;
                mpr.AlternativeSearchLink = @$"https://www.reliableparts.ca/search?q={search}";

            }
            else
            {
               
                return;
            }


        }
    
    
    }
}
