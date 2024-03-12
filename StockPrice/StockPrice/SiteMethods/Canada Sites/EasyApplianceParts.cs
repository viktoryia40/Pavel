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
    public sealed class EasyApplianceParts
    {
        private const string Source = "easyapplianceparts.ca";
        private const string ClassSource = "EasyApplianceParts";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            var mpr = new MainPriceResponse() { SearchUrl = @$"https://www.easyapplianceparts.ca/Search.ashx?SearchTerm={search}&SearchMethod=standard", Source = "Easyapplianceparts.ca", Additional = "📦$12.99" };
            var prices = new List<Prices>();

            string redirect;

            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@$"https://www.easyapplianceparts.ca/Search.ashx?SearchTerm={search}&SearchMethod=standard");
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
                if (redirect.Contains(@"SearchSuggestion.aspx?term"))
                {
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
            else//If there is no redirect at all, that's a problem.
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


                    var dataArgsRegex = Regex.Matches(response, @"(?<= data-args="").*?(?="")");
                    if (dataArgsRegex.Count > 0)
                    {
                        string dataArgs = dataArgsRegex.First().Value.Trim();

                        var argsSplit = dataArgs.Split('|').ToList();

                        if (argsSplit.Count > 5)
                        {
                            string title = argsSplit[1];
                            decimal price = decimal.Parse(argsSplit[2], CultureInfo.InvariantCulture);
                            string availability = argsSplit[5];

                            prices.Add(new Prices
                            {
                                Availability = availability,
                                Price = Math.Round(price, 2),
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
                        else
                        {
                            Console.WriteLine("ERROR EasyApplianceParts - not enough args!");

                            mpr.NoAnswerOrError = true;
                            mpr.ErrorMessage = "Not enough args.";
                            mainPriceResponsesList.Add(mpr);

                            await ResponseCreator.MakeResponseLog(con: con,
                                mpr: mpr,
                                request: request);

                            return;
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
