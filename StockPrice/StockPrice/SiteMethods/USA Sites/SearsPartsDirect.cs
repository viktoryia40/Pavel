using System.Web;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.Classes;

namespace StockPrice.SiteMethods.USA_Sites
{
    public class SearsPartsDirect
    {
        private const string Source = "searspartsdirect.com";
        private const string ClassSource = "SearsPartsDirect";

        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request.ToLower();

            var mpr = new MainPriceResponse()
            {
                SearchUrl = @$"https://www.searspartsdirect.com/search?q={search}#parttab", 
                Source = "Searspartsdirect.com"
            };
            var prices = new List<Prices>();


            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync();
            var priority = await con.QueryFirstOrDefaultAsync<DatabaseSitesPriority>($"SELECT * FROM sites_priority WHERE `SiteName`='SearsPartsDirect'");
            var searsPartsData = await con.QueryFirstOrDefaultAsync<DatabaseSearsPartsDirectData>($"SELECT * FROM searspartsdirect_data");
            await con.CloseAsync();

            string searchResult = null;
            try
            {



                string operationName = "partSearch";
                var variablesObj = new SearsPartsDirectClasses.VariablesConstructor.Variables()
                {
                    Q = search.ToLower()
                };
                

                // ORDERS
                variablesObj.Orders = new()
                {
                    new()
                    {
                        Name = "SCORE",
                        OrderOrder = "DESC"
                    },
                    new()
                    {
                        Name = "SYNONYMS",
                        OrderOrder = "ASC"
                    },
                    new()
                    {
                        Name = "SELLABLE",
                        OrderOrder = "DESC"
                    },
                    new()
                    {
                        Name = "RANK",
                        OrderOrder = "DESC"
                    },
                    new()
                    {
                        Name = "AVAILABILITY",
                        OrderOrder = "DESC"
                    }
                };
                //FILTERS
                variablesObj.Filters = new()
                {
                    new()
                    {
                        Name = "RESTRICTION",
                        Type = "NOT",
                        Values = new List<string>(){"31", "49", "60", "4", "5", "9", "10", "11", "13", "21", "22", "25", "33", "34", "12",
                        "17", "6", "16", "26", "52", "59"}

                    },
                    new()
                    {
                        Name = "SELLABLE",
                        Type = "MATCH",
                        Values = "true"
                    }
                };
                //SUBSTILTED
                variablesObj.SubstitutedByListFilter = new()
                {
                    new()
                    {
                        Name = "AVAILABILITY",
                        Type = "MATCH",
                        Values =  
                            new List<string>()
                            {
                                "PIA",
                                "BORD",
                                "NLO"
                            }

                    },
                    new()
                    {
                        Name = "PRICE",
                        Type = "RANGE",
                        Values =  
                            new List<string>()
                            {
                                ">1"
                            }
                    },
                    new()
                    {
                        Name = "SELLABLE",
                        Type = "MATCH",
                        Values = "true"
                    },
                    new()
                    {
                        Name = "RESTRICTION",
                        Type = "NOT",
                        Values =  new List<string>()
                        {
                            "31",
                            "49",
                            "60",
                            "4",
                            "5",
                            "9",
                            "10",
                            "11",
                            "13",
                            "21",
                            "22",
                            "25",
                            "33",
                            "34",
                            "12",
                            "17",
                            "6",
                            "16",
                            "26",
                            "52",
                            "59"
                        }

                    }
                };
                //TAXONOMY
                variablesObj.TaxonomySearchFilter = new()
                {
                    new()
                    {
                        Name = "TYPE",
                        Values = "IARCH"
                    }
                };
                //PAGE
                variablesObj.Page = new()
                {
                    From = 0,
                    Size = 20
                };

                
                string variables = JsonConvert.SerializeObject(variablesObj);
                
                string extensions = JsonConvert.SerializeObject(
                    new SearsPartsDirectClasses.ExtesionsConstructor.Extensions()
                    {
                        PersistedQuery = new()
                        {
                            Version = 1,
                            Sha256Hash = searsPartsData.SHA256Hash
                        }
                    });


                string test =
                    $"https://catalog-staging.partsdirect.io/graphql?operationName={operationName}&variables={HttpUtility.UrlEncode(variables)}&extensions={HttpUtility.UrlEncode(extensions)}";

                string test2 =
                    $"https://catalog-staging.partsdirect.io/graphql?operationName={operationName}&variables={variables}&extensions={extensions}";
                searchResult = CustomHttpClass.GetToString(
                    url: $"https://catalog-staging.partsdirect.io/graphql?operationName={operationName}&variables={variables}&extensions={extensions}",
                    headers: new List<CustomHttpAdditionals.Headers>
                    {
                        new() { Name = "x-trace-id", Value = searsPartsData.TraceId},
                        new() { Name = "x-api-key", Value = searsPartsData.ApiKey },
                        new() { Name = "X-Apollo-Operation-Name", Value = "partSearch" }
                    });



               


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

            if (!string.IsNullOrEmpty(searchResult))
            {
               
            }
            string price = null;
            string availability = null;
            string title = null;
            string totalUrl = null;
            try
            {
                dynamic response = JsonConvert.DeserializeObject(searchResult);

                if (response.data.partSearch.parts.Count > 0)
                {
                    if (response.data.partSearch.parts.Count > 0) mpr.MultiChoice = true;
                    dynamic selectedItem = response.data.partSearch.parts[0];
                    string id = selectedItem.id;
                    string number = selectedItem.number;

                    if (number.ToLower().Trim().Equals(search.ToLower().Trim()))
                    {
                        totalUrl = $"https://www.searspartsdirect.com/product/{id}";

                        if (selectedItem.substitutedByList.parts.Count > 0)
                        {
                            dynamic replacesItem = selectedItem.substitutedByList.parts[0];
                            price = replacesItem.pricing.sell.ToString();
                            string checkAvailability = replacesItem.pricing.availabilityInfo.status;
                            availability = checkAvailability == "PIA" ? "In stock" : checkAvailability;
                            title = replacesItem.title;
                            

                        }
                        else
                        {
                            price = selectedItem.pricing.sell.ToString();
                            string checkAvailability = selectedItem.pricing.availabilityInfo.status;
                            availability = checkAvailability == "PIA" ? "In stock" : checkAvailability;
                            title = selectedItem.title;
                        }
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

            if (price != null && availability != null && title != null)
            {
                prices.Add(new()
                {
                    Availability = availability,
                    Price = decimal.Parse(price),
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
    }
}
