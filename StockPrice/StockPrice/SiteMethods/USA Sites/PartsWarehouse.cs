using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;


namespace StockPrice.SiteMethods.USA_Sites
{
    public sealed class PartsWarehouse
    {
        private const string Source = "partswarehouse.com";
        private const string ClassSource = "PartsWarehouse";

        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            var mpr = new MainPriceResponse()
            {
                SearchUrl = @$"https://www.partswarehouse.com/#q={search.ToLower()}", 
                Source = "Partswarehouse.com"
            };
            var prices = new List<Prices>();


            try
            {
                string resp = CustomHttpClass.GetToString(@$"https://searchv7.expertrec.com/v6/search/203604622ae44969b70949e303dfef01/?q={search.ToLower()}");

                if (resp != null)
                {
                    dynamic resp_j = JsonConvert.DeserializeObject(resp);

                    if (resp_j.results.Count > 0)
                    {

                        for (int i = 0; i < resp_j.results.Count; i++)
                        {
                            string productcode = resp_j.results[i].productcode;

                            List<string> product_code_split = productcode.Split('-').ToList();
                            if (product_code_split.Contains(search.ToUpper()) || product_code_split.Contains(search.ToLower()))
                            {



                                string title = resp_j.results[i].productname;
                                decimal price = decimal.Parse(resp_j.results[i].saleprice.ToString().Replace(",", "."), CultureInfo.InvariantCulture);
                                string url = $"https://www.partswarehouse.com/-p/{resp_j.results[i].productcode}.htm";

                                prices.Add(new()
                                {
                                    Price = price,
                                    Title = title,
                                    Url = url
                                });

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


                mpr.NothingFoundOrOutOfStock = true;
                mainPriceResponsesList.Add(mpr);

                await ResponseCreator.MakeResponseLog(con: con,
                    mpr: mpr,
                    request: request);
                return;
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
        }
    }
}
