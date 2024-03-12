using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.Methods.Authorization;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;
using System.Text.RegularExpressions;



namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class AmreSupply
    {
        private const string Source = "amresupply.com";
        private const string ClassSource = "AmreSupply";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.amresupply.com/search?q={search}", Source = "Amresupply.com", Additional = "📦$13.99" };
            var prices = new List<Prices>();

            string redirect;
            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.amresupply.com/search?q={search}", coockies: AmreSupplyAuth.AmreSupplyAuthCookie, selected_proxy: AmreSupplyAuth.SelectedProxy);
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

            if (redirect != null && !redirect.Equals("/login")) //If redirected directly to the product
            {
                totalUrl = redirect;

            }
            else
            {
                string searchResult = null;
                try
                {
                    searchResult = CustomHttpClass.GetToString(@$"https://www.amresupply.com/search?q={search}", coockies: AmreSupplyAuth.AmreSupplyAuthCookie, acceptencoding: "none", selected_proxy: AmreSupplyAuth.SelectedProxy);
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



                var searchResultsHtmlBlocksRegex = Regex.Matches(searchResult, @"(?<=method=""post"">)[\w\W]*?(?=</form>)");
                if (searchResultsHtmlBlocksRegex.Count > 0)
                {
                    List<string> searchResultsHtmlBlocksTempList = new();

                    for (int i = 0; i < searchResultsHtmlBlocksRegex.Count; i++)
                    {
                        searchResultsHtmlBlocksTempList.Add(searchResultsHtmlBlocksRegex[i].Value);
                    }

                    searchResultsHtmlBlocksTempList = searchResultsHtmlBlocksTempList.Where(x => x.Contains("In-Stock")).ToList();
                    if (searchResultsHtmlBlocksTempList.Count > 0) mpr.MultiChoice = true;

                    List<Prices> tempPriceList = new();

                    foreach (var productBlock in searchResultsHtmlBlocksTempList)
                    {
                        decimal takenPrice = 0;
                        var tempPriceRegex = Regex.Matches(productBlock, @"\$\d+(?:\.\d+)?");
                        if (tempPriceRegex.Count > 0) takenPrice = decimal.Parse(tempPriceRegex.First().Value.Trim().Replace("$", "").Replace(",", "."), CultureInfo.InvariantCulture);

                        var hrefsList = Regex.Matches(productBlock, @"(?<= href="").*?(?="")").Cast<Match>()
                                  .Select(match => match.Value)
                                  .Distinct()
                                  .ToList();

                        if (hrefsList.Count > 0) tempPriceList.Add(new()
                        {
                            Url = hrefsList.FirstOrDefault(),
                            Price = takenPrice
                        });
                    }

                    tempPriceList = tempPriceList.Where(x => !string.IsNullOrEmpty(x.Url)).ToList().OrderBy(x => x.Price).ToList();
                    if (tempPriceList.Count > 0)
                        totalUrl = tempPriceList[0].Url;
                    else//In fact, the wrong exit.
                    {
                        
                        mpr.NoAnswerOrError = true;
                        mpr.ErrorMessage = "Couldn't generate a temporary list of prices from the search results.";
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


            string response;
            try
            {
                response = CustomHttpClass.GetToString(totalUrl, coockies: AmreSupplyAuth.AmreSupplyAuthCookie, acceptencoding: "none");
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
                decimal retailPrice = 0;
                decimal yourPrice = 0;
                string title = null;
                string availability = null;

                var yourPriceBlockRegex = Regex.Matches(response, @"(?<=<span class=""yourPrice"">)[\w\W]*?(?=</sup>)");
                if (yourPriceBlockRegex.Count > 0)
                {
                    var yourPriceRegex = Regex.Matches(yourPriceBlockRegex.First().Value.Trim(), @"\d{1,}");
                    if (yourPriceRegex.Count == 2)
                    {
                        yourPrice = decimal.Parse(string.Join('.', yourPriceRegex), CultureInfo.InvariantCulture);
                    }
                }

                var allPricesRegex = Regex.Matches(response, @"\$\d+(?:\.\d+)?");
                if (allPricesRegex.Count > 1)
                {
                    if (!response.Contains("alternativePart"))
                        retailPrice = decimal.Parse(allPricesRegex.Last().Value.Trim().Replace("$", ""), CultureInfo.InvariantCulture) + yourPrice;
                    else
                        retailPrice = decimal.Parse(allPricesRegex[^2].Value.Trim().Replace("$", ""), CultureInfo.InvariantCulture) + yourPrice;

                }



                var titleRegex = Regex.Matches(response, @"(?<=>).*?(?=</h1>)");
                if (titleRegex.Count > 0)
                {
                    title = titleRegex.First().Value.Trim();
                }

                var availabilityFirstRegex = Regex.Matches(response, @"(?<=<div class=""partStock"" style=""clear: both;"">)[\w\W]*?(?=</div>)");
                if (availabilityFirstRegex.Count > 0)
                {
                    string availabilityFirstBlock = availabilityFirstRegex.First().Value.Trim();
                    var availabilitySecondRegex = Regex.Matches(availabilityFirstBlock, @"(?<=>).*(?=<)");
                    if (availabilitySecondRegex.Count > 0)
                    {
                        availability = availabilitySecondRegex.First().Value.Trim();
                        availability = string.Join("", availability.ToCharArray().Select(x => char.IsUpper(x) ? " " + x : "" + x).ToList());
                    }
                    else
                    {
                        mpr.NothingFoundOrOutOfStock = true;
                        
                    }
                }

                prices.Add(new Prices
                {
                    Availability = availability,
                    DoublePrice = $@"${yourPrice.ToString(CultureInfo.InvariantCulture).Replace(',', '.')} / ${retailPrice.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}",
                    Title = title,
                    Url = totalUrl

                });
                mpr.LowestPrice = Math.Min(yourPrice, retailPrice);
                mpr.PricesList = prices;

                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
            }
        }
    }
}
