using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;

namespace StockPrice.SiteMethods.Canada_Sites
{
    public sealed class PartsExpert
    {
        private const string Source = "partsexpert.ca";
        private const string ClassSource = "PartsExpert";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            var mpr = new MainPriceResponse() { SearchUrl = @$"https://partsexpert.ca/?s={search}&post_type=product&type_aws=true", Source = "Partsexpert.ca", Additional = "📦$14.99" };
            var prices = new List<Prices>();


            try
            {
                string searchResult = CustomHttpClass.PostToString(url: @"https://partsexpert.ca/?wc-ajax=aws_action",
                    jsonData: @$"action=aws_action&keyword={search}&aws_page=&aws_tax=&lang=&pageurl=https://partsexpert.ca/&typedata=json",
                    contentType: "application/x-www-form-urlencoded");
                if (searchResult != null)
                {
                    dynamic searchDataJ = JsonConvert.DeserializeObject(searchResult);

                    if (searchResult.Contains("products") && searchDataJ.products.Count > 0)
                    {
                        List<Prices> tempList = new();
                        for (int i = 0; i < searchDataJ.products.Count; i++)
                        {
                            string title = Convert.ToString(searchDataJ.products[i].title);
                            title = title.Split('>').ToList().Last().Trim();

                            decimal price = decimal.Parse(searchDataJ.products[i].f_price.ToString(), CultureInfo.InvariantCulture);
                            if (price == 0) continue;

                            string url = searchDataJ.products[i].link;
                            tempList.Add(new Prices
                            {
                                Price = price,
                                Title = title,
                                Url = url
                            });
                        }

                        tempList = tempList.OrderBy(x => x.Price).ToList();
                        if (tempList.Count > 0) prices.Add(tempList.First());

                        if (prices.Count > 0)
                        {
                            prices = prices.OrderBy(x => x.Price).ToList();
                            decimal lowestPrice = prices.Select(x => x.Price).First();
                            mpr.LowestPrice = lowestPrice;
                            mpr.PricesList = prices;
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

                        

                        mainPriceResponsesList.Add(mpr);

                        await ResponseCreator.MakeResponseLog(con: con,
                            mpr: mpr,
                            request: request);
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
