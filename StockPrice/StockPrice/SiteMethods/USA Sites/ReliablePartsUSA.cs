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


namespace StockPrice.SiteMethods.USA_Sites
{
    public sealed class ReliablePartsUsa
    {
        private const string Source = "reliableparts.net";
        private const string ClassSource = "ReliablePartsUsa";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://reliableparts.net/us/content/#/search/{search}", Source = "Reliableparts.net", Additional = "📦$10.95" };
            var prices = new List<Prices>();

            var auth_header = new List<CustomHttpAdditionals.Headers>() {
                new(){ Name = "Authorization", Value = $"Bearer {ReliablePartsAuthUsa.ReliablePartsBearerToken}" }
            };

            string search_result = null;
            try
            {
                search_result = CustomHttpClass.GetToString($@"https://prodapi.reliableparts.net/us/navapp/v1/search/modelProduct?q={search}", headers: auth_header);
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
            if (search_result != null)
            {
                var search_result_deserialized = JsonConvert.DeserializeObject<ReliablePartsClasses.Results>(search_result);

                if (search_result_deserialized.Products.Count > 0)
                {
                    if (search_result_deserialized.Products.Count > 1) mpr.MultiChoice = true;
                    var first_product = search_result_deserialized.Products.First();

                    string selected_name = first_product.Name;
                    string selected_manufacturer = first_product.Manufacturer;
                    string selected_product_number = first_product.ProductNumber;

                    string product_details = null;

                    try
                    {
                        product_details = CustomHttpClass.GetToString($@"https://prodapi.reliableparts.net/us/navapp/v1/product/detail/{selected_name}?mfc={selected_manufacturer}", headers: auth_header);

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

                    if (product_details != null)
                    {
                        var product_detail_deserialized = JsonConvert.DeserializeObject<ReliablePartsClasses.ProductDetail>(product_details);


                        decimal retail_price = 0;
                        decimal your_price = 0;
                        string title = null;
                        string availability = null;

                        title = product_detail_deserialized.Description;
                        retail_price = decimal.Parse(product_detail_deserialized.RetailPrice, CultureInfo.InvariantCulture);
                        your_price = decimal.Parse(product_detail_deserialized.PartnerPrice, CultureInfo.InvariantCulture);

                        if (product_detail_deserialized.InStock) availability = "In Stock";
                        else if (product_detail_deserialized.SpecialOrder) availability = "Special Order";
                        else if (!product_detail_deserialized.InStock && !product_detail_deserialized.SpecialOrder)
                        {
                            mpr.NothingFoundOrOutOfStock = true;
                            mainPriceResponsesList.Add(mpr);
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
                            Url = $@"https://reliableparts.net/us/content/#/part/{selected_product_number}"
                        });
                        mpr.LowestPrice = Math.Min(your_price, retail_price);

                        if (product_detail_deserialized.OemProductManufacturer != null && product_detail_deserialized.OemProductNumber != null)
                        {
                            mpr.Source = "Reliableparts.net->[NON OEM]";

                            MakeOEMorAlternative(mainPriceResponsesList, product_detail_deserialized, request);
                        }
                        else if (product_detail_deserialized.AlternateProductNumber != null && product_detail_deserialized.AlternateProductManufacturer != null)
                        {
                            mpr.Source = "Reliableparts.net->[OEM]";

                            MakeOEMorAlternative(mainPriceResponsesList, product_detail_deserialized, request);
                        }

                        if (product_detail_deserialized.Warehouses.Count > 0)
                        {
                            List<string> locations = new();
                            foreach (var warehouse_data in product_detail_deserialized.Warehouses)
                            {
                                string location_name = warehouse_data.Description.Replace("PICK UP ONLY", "pickup").ToLower();
                                location_name = char.ToUpper(location_name[0]) + location_name[1..];
                                locations.Add($"{location_name} ({warehouse_data.Quantity})");
                            }
                            if (locations.Count > 0) mpr.Locations = locations;
                        }

                        MarkeAlternativeLinkFromReliablePartsCa(request, product_detail_deserialized, mpr);
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

            var mpr = new MainPriceResponse() { SearchUrl = @$"https://reliableparts.net/us/content/#/search/{search}", Source = "Reliableparts.net", Additional = "📦$10.95" };
            var prices = new List<Prices>();
            var authHeader = new List<CustomHttpAdditionals.Headers>() {
                new(){ Name = "Authorization", Value = $"Bearer {ReliablePartsAuthUsa.ReliablePartsBearerToken}" }
            };

            if (sourceProduct.OemProductManufacturer != null)
            {
                mpr.Source = "Reliableparts.net->[OEM]";
            }
            else
            {
                mpr.Source = "Reliableparts.net->[NON OEM]";
            }

            string targetManifatcure;
            string targetNumber;
            if (sourceProduct.OemProductManufacturer != null)
            {
                targetManifatcure = sourceProduct.OemProductManufacturer;
                targetNumber = sourceProduct.OemProductNumber;
            }
            else
            {
                targetManifatcure = sourceProduct.AlternateProductManufacturer;
                targetNumber = sourceProduct.AlternateProductNumber;
            }

            string productDetails = null;

            try
            {
                productDetails = CustomHttpClass.GetToString($@"https://prodapi.reliableparts.net/us/navapp/v1/product/detail/{targetNumber}?mfc={targetManifatcure}", headers: authHeader);

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

            if (productDetails != null)
            {
                var productDetailDeserialized = JsonConvert.DeserializeObject<ReliablePartsClasses.ProductDetail>(productDetails);


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

                prices.Add(new() { Availability = availability, DoublePrice = $@"${yourPrice.ToString().Replace(',', '.')} / ${retailPrice.ToString().Replace(',', '.')}", Title = title, Url = $@"https://reliableparts.net/ca/en/content/#/part/{targetNumber}" });
                mpr.LowestPrice = Math.Min(yourPrice, retailPrice);
                mpr.PricesList = prices;

                MarkeAlternativeLinkFromReliablePartsCa(request, productDetailDeserialized, mpr);
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);

            }

        }
        
        public static void MarkeAlternativeLinkFromReliablePartsCa (DatabaseTotalResults request, ReliablePartsClasses.ProductDetail source_product, MainPriceResponse mpr)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);

            bool isLaternate = false;

            //mpr.SearchUrl = @$"https://www.reliableparts.com/search?q={search}";
            var prices = new List<Prices>();


            string redirect = null;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.reliableparts.com/search?q={search}");

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
                total_url = $@"https://www.reliableparts.com{redirect}";
            }
            else //issue analysis.
            {
                if (source_product.OemProductManufacturer != null)
                {
                    isLaternate = true;
                }
                else
                {
                    isLaternate = false;
                }

                string search_result = null;

                try
                {
                    search_result = CustomHttpClass.GetToString(@$"https://www.reliableparts.com/search?q={search}", acceptencoding: "none");
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
                        if (isLaternate)
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
                mpr.AlternativeSearchLink = @$"https://www.reliableparts.com/search?q={search}";

            }
            else
            {
               
                return;
            }


        }
    
    
    }
}
