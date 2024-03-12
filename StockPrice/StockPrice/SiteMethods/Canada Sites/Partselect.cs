using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;
using System.Text.RegularExpressions;


namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class PartselectCa
    {
        private const string Source = "partselect.ca";
        private const string ClassSource = "PartselectCa";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            //https://www.partselect.ca/Search-Suggestions.aspx?term=DE634A
            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.partselect.ca/Search.ashx?SearchTerm={search}&SearchMethod=standard", Source = "Partselect.ca", Additional = "📦$12.99" };
            var prices = new List<Prices>();

            string redirect;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.partselect.ca/Search.ashx?SearchTerm={search}&SearchMethod=standard");

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

            if (!string.IsNullOrEmpty(redirect))
            {
                if (redirect.Contains("Search-Suggestions.aspx?term")) //If you've been bounced to a multicarrier.
                {
                    string searchResult = null;
                    mpr.NothingFoundOrOutOfStock = true;
                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);

                    return;


                }
                else
                    totalUrl = redirect;
            }
            else//If there is no redirect at all - error.
            {
                mpr.NoAnswerOrError = true;
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);

                return;
            }

            if (totalUrl != null)
            {
                try
                {
                    string response = CustomHttpClass.GetToString(totalUrl);

                    var titleRegex = Regex.Matches(response, @"(?<=""name"">).*(?=</h1>)");
                    if (titleRegex.Any())
                    {
                        string title = titleRegex.First().Value.Trim();

                        var priceRegex = Regex.Matches(response, @"(?<=class=""js-partPrice"">).*(?=</span>)");
                        if (priceRegex.Any())
                        {
                            decimal price = decimal.Parse(priceRegex.First().Value, CultureInfo.InvariantCulture);

                            var availabilityRegex = Regex.Matches(response, @"(?<= <span itemprop=""availability"" content="").*(?="">)");
                            if (availabilityRegex.Count > 0)
                            {
                                string availability = availabilityRegex.First().Value.Trim();
                                availability = string.Join("", availability.ToCharArray().Select(x => char.IsUpper(x) ? " " + x : "" + x).ToList());


                                prices.Add(new()
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
}
