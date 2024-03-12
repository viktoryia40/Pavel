using Dapper;
using Leaf.xNet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.Methods.Authorization;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.Classes;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class BetaMarconeCanada
    {
        private const string Source = "marcone.com";
        private const string ClassSource = "BetaMarconeCanada";

        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://beta.marcone.com/Home/SearchPartModelList?searchString={search}&Type=Part", Source = "Marcone.com" };
            var prices = new List<Prices>();

            var authMarcone = MarconeAuthCanada.MarconeAuthCookie;





            decimal retailPrice = 0;
            decimal yourPrice = 0;
            string availability = null;


            string redirect;

            string varMake = null;
            string varPart = null;
            string varDescription = null;

            string zipcode = null;
            List<string> totalUrlsList = new();

            try //Search engine query
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://beta.marcone.com/Home/SearchPartModelList?searchString={search}&Type=Part", coockies: authMarcone, acceptencoding: "none", selected_proxy: MarconeAuthCanada.SelectedProxy);

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

            if (redirect != null) //If you hit a redirect
            {
                totalUrlsList.Add(@$"https://beta.marcone.com{redirect}");
            }

            else //If you didn't hit the redirect - you need to analyze the output
            {
                mpr.MultiChoice = true;
                string search_result = null;
                try
                {
                    search_result = CustomHttpClass.GetToString(@$"https://beta.marcone.com/Home/SearchPartModelList?searchString={search}&Type=Part", coockies: authMarcone, acceptencoding: "none", selected_proxy: MarconeAuthCanada.SelectedProxy);


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

                var searchResultsRegex = Regex.Matches(search_result, @"(?<=<a class=""cursor"" onclick=""redirectToDetails\().*?(?=\);"")");

                if (searchResultsRegex.Count > 0)
                {
                    for (int i = 0; i < searchResultsRegex.Count; i++)
                    {
                        var takenRowSplit = searchResultsRegex[i].Value.Trim().Replace("'", "").Split(',');

                        string takenProductCode = takenRowSplit[0].Trim();
                        string takenProductType = takenRowSplit[1].Trim();




                        totalUrlsList.Add($@"https://beta.marcone.com/Product/Detail?Machine=&Category=&Part={takenProductCode}&Make={takenProductType}");




                    }
                }
                else
                {
                    mpr.NothingFoundOrOutOfStock = true;
                    mpr.PricesList = prices;
                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }

            }


            if (totalUrlsList.Count == 0)
            {

                mpr.NoAnswerOrError = true;
                mpr.ErrorMessage = "There is no answer.";
                mpr.PricesList = prices;
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
                return; //If you can't find the goods at all.
            }

            foreach (var totalUrlTaken in totalUrlsList)
            {
                string totalUrl = totalUrlTaken;
                string response;
                try //We're getting a page response
                {
                    while (true)
                    {
                        response = CustomHttpClass.GetToString(totalUrl, coockies: authMarcone, acceptencoding: "none", selected_proxy: MarconeAuthCanada.SelectedProxy);
                        if (response == null) return;

                        else
                       if (response.Contains("USE WPL"))
                        {
                            var hrefRegex = Regex.Matches(response, @"(?<=<a href="").*?(?="">USE WPL)");
                            if (hrefRegex.Any())
                            {
                                totalUrl = $@"https://beta.marcone.com{hrefRegex.First().Value.Trim()}";

                            }

                        }
                        else break;
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

                if (response != null)
                {
                    try //Getting retail price
                    {
                        var retail_price_block_regex = Regex.Matches(response, @"(?<=Retail Price)[\w\W]*?</tr>");
                        if (retail_price_block_regex.Count > 0)
                        {
                            var retail_price_block = retail_price_block_regex.First().Value;
                            var first_retail_price_regex = Regex.Matches(retail_price_block, @"\$\d.*\d");
                            if (first_retail_price_regex.Count > 0)
                            {
                                var first_retail_price = first_retail_price_regex.First().Value;
                                var second_retail_price_regex = Regex.Matches(first_retail_price, @"\d.*\d");
                                if (second_retail_price_regex.Count > 0)
                                {
                                    retailPrice = decimal.Parse(second_retail_price_regex.First().Value, CultureInfo.InvariantCulture);

                                }
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
                            stage: 3,
                            source: Source,
                            classSource: ClassSource,
                            base64WrongData: null,
                            url: null);
                        return;
                    }

                    try //Getting your price
                    {
                        var yourPriceBlockRegex = Regex.Matches(response, @"(?<=Your Price)[\w\W]*?</tr>");

                        if (yourPriceBlockRegex.Count > 0)
                        {
                            var yourPriceBlock = yourPriceBlockRegex.First().Value;
                            var firstYourPriceRegex = Regex.Matches(yourPriceBlock, @"\$\d.*\d");

                            if (firstYourPriceRegex.Count > 0)
                            {
                                var firstYourPrice = firstYourPriceRegex.First().Value;
                                var secondYourPriceRegex = Regex.Matches(firstYourPrice, @"\d.*\d");
                                if (secondYourPriceRegex.Count > 0)
                                {
                                    yourPrice = decimal.Parse(secondYourPriceRegex.First().Value, CultureInfo.InvariantCulture);

                                }
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
                            stage: 4,
                            source: Source,
                            classSource: ClassSource,
                            base64WrongData: null,
                            url: null);
                        return;
                    }

                    try //Getting part and make
                    {
                        var partRegex = Regex.Matches(response, @"(?<=var part = ').*?(?=';)");
                        if (partRegex.Count > 0)
                        {
                            varPart = partRegex.First().Value.Trim();

                            var makeRegex = Regex.Matches(response, @"(?<=var make = ').*?(?=';)");
                            if (makeRegex.Count > 0)
                            {
                                varMake = makeRegex.First().Value.Trim();
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

                    try//Getting description (title)
                    {

                        var descriptionRegex = Regex.Matches(response, @"(?<=var description = ').*?(?=')");
                        if (descriptionRegex.Count > 0)
                        {
                            varDescription = descriptionRegex.First().Value.Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        await ResponseCreator.MakeErrorLog(con: con,
                            mpr: mpr,
                            mainPriceResponsesList: mainPriceResponsesList,
                            request: request,
                            base64ErrorData: ex.Message.ToString(),
                            stage: 6,
                            source: Source,
                            classSource: ClassSource,
                            base64WrongData: null,
                            url: null);
                        return;
                    }

                    try//Obtaining a zipcode
                    {
                        var zipcodeRegex = Regex.Matches(response, @"(?<=value="").*?(?="" id=""ZipCodeChangetxt"")");
                        if (zipcodeRegex.Count > 0)
                        {
                            zipcode = zipcodeRegex.First().Value.Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        await ResponseCreator.MakeErrorLog(con: con,
                            mpr: mpr,
                            mainPriceResponsesList: mainPriceResponsesList,
                            request: request,
                            base64ErrorData: ex.Message.ToString(),
                            stage: 7,
                            source: Source,
                            classSource: ClassSource,
                            base64WrongData: null,
                            url: null);
                        return;
                    }


                }

                string availabilityResponse;
                try //Request for information on product availability
                {
                    var rp = new RequestParams();
                    rp["make"] = varMake;
                    rp["part"] = varPart;
                    rp["cartId"] = "0";

                    availabilityResponse = CustomHttpClass.PostToString(@"https://beta.marcone.com/Landing/GetSSBStockInfo",
                        coockies: authMarcone,
                        data: rp,
                        acceptencoding: "none", selected_proxy: MarconeAuthCanada.SelectedProxy);
                }
                catch (Exception ex)
                {
                    await ResponseCreator.MakeErrorLog(con: con,
                        mpr: mpr,
                        mainPriceResponsesList: mainPriceResponsesList,
                        request: request,
                        base64ErrorData: ex.Message.ToString(),
                        stage: 8,
                        source: Source,
                        classSource: ClassSource,
                        base64WrongData: null,
                        url: null);
                    return;
                }

                if (availabilityResponse != null)
                {
                    string availabilityAllResponse = null;
                    var availabilityRegex = Regex.Matches(availabilityResponse, @"(?<=> ).*?(?=</span>\|)");

                    if (availabilityRegex.Any())
                    {
                        availability = availabilityRegex.First().Value.Trim();
                        if (availability.Contains("Out Of Stock"))
                        {



                            try
                            {
                                var rp = new RequestParams();
                                rp["part"] = varPart;
                                rp["make"] = varMake;
                                rp["zipcode"] = zipcode;
                                rp["defaultQuantity"] = "0";

                                availabilityAllResponse = CustomHttpClass.PostToString(@"https://beta.marcone.com/Landing/GetBranchAvialabilityForProductDetail",
                                    coockies: authMarcone,
                                    data: rp,
                                    acceptencoding: "none", selected_proxy: MarconeAuthCanada.SelectedProxy);
                            }
                            catch (Exception ex)
                            {
                                await ResponseCreator.MakeErrorLog(con: con,
                                    mpr: mpr,
                                    mainPriceResponsesList: mainPriceResponsesList,
                                    request: request,
                                    base64ErrorData: ex.Message.ToString(),
                                    stage: 9,
                                    source: Source,
                                    classSource: ClassSource,
                                    base64WrongData: null,
                                    url: null);
                                return;
                            }

                            if (availabilityAllResponse != null)
                            {
                                if (!availabilityAllResponse.Contains("In Stock"))
                                {
                                    continue;
                                }

                                else
                                    availability = "In stock (other location)";
                            }
                            else
                            {
                                mpr.NoAnswerOrError = true;
                                mpr.ErrorMessage = "Error stage 5-2.";
                                mpr.PricesList = prices;
                                mainPriceResponsesList.Add(mpr);

                                con.Open();
                                con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, 'Price', '{MySqlHelper.EscapeString(JsonConvert.SerializeObject(mpr))}');");
                                con.Close();
                                return;
                            }


                        }
                        else
                        {
                            try
                            {
                                var rp = new RequestParams();
                                rp["part"] = varPart;
                                rp["make"] = varMake;
                                rp["zipcode"] = zipcode;
                                rp["defaultQuantity"] = "0";

                                availabilityAllResponse = CustomHttpClass.PostToString(@"https://beta.marcone.com/Landing/GetBranchAvialabilityForProductDetail",
                                    coockies: authMarcone,
                                    data: rp,
                                    acceptencoding: "none", selected_proxy: MarconeAuthCanada.SelectedProxy);
                            }
                            catch (Exception ex)
                            {
                                await ResponseCreator.MakeErrorLog(con: con,
                                    mpr: mpr,
                                    mainPriceResponsesList: mainPriceResponsesList,
                                    request: request,
                                    base64ErrorData: ex.Message.ToString(),
                                    stage: 10,
                                    source: Source,
                                    classSource: ClassSource,
                                    base64WrongData: null,
                                    url: null);
                                return;
                            }
                        }

                        if (availabilityAllResponse != null) //A list of all locations.
                        {
                            List<string> allLocationsData = new();
                            try
                            {
                                var deserializedLocations = JsonConvert.DeserializeObject<BetaMarconeClasses.Locations>(Regex.Unescape(availabilityAllResponse));
                                string locationsDataWithoutTags = Regex.Replace(deserializedLocations.Message, @"<.*?>", "");
                                var locationsSplit = locationsDataWithoutTags.Split(',');
                                for (int i = 0; i < locationsSplit.Length; i++)
                                {
                                    string location = locationsSplit[i].Trim().ToString();
                                    var location_regex = Regex.Matches(location, @"\w{1,}\s\(.*\d{1,}");
                                    if (location_regex.Count > 0)
                                    {
                                        string location_name = (location_regex.First().Value + ")").ToLower();
                                        location_name = char.ToUpper(location_name[0]) + location_name[1..];
                                        allLocationsData.Add(location_name);
                                    }
                                }
                                if (allLocationsData.Count > 0) mpr.Locations = allLocationsData;
                            }
                            catch 
                            {
                                Console.WriteLine("BetaMarconeCanada error locations");
                               
                            }
                           
                        }
                    }

                }


                prices.Add(new()
                {
                    Availability = availability,
                    DoublePrice = $@"${yourPrice.ToString().Replace(',', '.')} / ${retailPrice.ToString().Replace(',', '.')}",
                    Url = totalUrl,
                    Title = varDescription
                });
            }

            try
            {
                prices = prices.OrderBy(x => x.Price).Where(x => x.Availability.Contains("In Stock") || x.Availability.Contains("In stock (other location)")).ToList();

            }
            catch (Exception ex)
            {
                await ResponseCreator.MakeErrorLog(con: con,
                    mpr: mpr,
                    mainPriceResponsesList: mainPriceResponsesList,
                    request: request,
                    base64ErrorData: ex.Message.ToString(),
                    stage: 11,
                    source: Source,
                    classSource: ClassSource,
                    base64WrongData: null,
                    url: null);
                return;
            }
            if (prices.Count == 0)
            {
                mpr.NothingFoundOrOutOfStock = true;
                mpr.PricesList = prices;
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
                return;
            }

            var firstPriceTaken = prices.First();

            var doublePriceSplited = firstPriceTaken.DoublePrice.Split('/');

            if (doublePriceSplited.Count() == 2)
            {
                yourPrice = decimal.Parse(doublePriceSplited[0].Trim().Replace("$", ""), CultureInfo.InvariantCulture);
                retailPrice = decimal.Parse(doublePriceSplited[1].Trim().Replace("$", ""), CultureInfo.InvariantCulture);
            }
            else
            {
                mpr.NoAnswerOrError = true;
                mpr.ErrorMessage = "Error stage 6-1.";
                mpr.PricesList = prices;
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
                return;
            }

            mpr.LowestPrice = Math.Min(yourPrice, retailPrice);
            mpr.PricesList = new List<Prices>() { firstPriceTaken };

            mainPriceResponsesList.Add(mpr);

            await ResponseCreator.MakeResponseLog(con: con,
                mpr: mpr,
                request: request);


        }
    }
}

