using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;

namespace StockPrice.SiteMethods.USA_Sites
{
    public sealed class EbayCom
    {
        private const string Source = "ebay.com";
        private const string ClassSource = "EbayCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request.ToUpper();

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            await using var con = new MySqlConnection(cs);

            await con.OpenAsync();
            var settings = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={request.ChatID}");
            await con.CloseAsync();

            // https://www.ebay.com/sch/i.html?_from=R40&_nkw=W10187839&_sacat=0&_stpos=L4L0A1&_fspt=1&LH_PrefLoc=3&rt=nc&LH_All=1
            var mpr = new MainPriceResponse()
            {
                SearchUrl = @$"https://www.ebay.com/sch/i.html?_from=R40&_nkw={search}&_sacat=0&_stpos=L4L0A1&_fspt=1&LH_PrefLoc=3&rt=nc&LH_All=1", 
                Source = "eBay.com", 
                Additional = "🅱"
            };
            var prices = new List<Prices>();


            string searchRes = null;
            
                

                try
                {

                    searchRes = CustomHttpClass.GetToString(@$"https://www.ebay.com/sch/i.html?_from=R40&_nkw={search}&_sacat=0&_stpos=L4L0A1&_fspt=1&LH_PrefLoc=3&rt=nc&LH_All=1",
                        acceptencoding: "none",
                        use_chrome_random_ua: true,
                        use_google_ua: false);
                   
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
            

            

            if (searchRes != null)
            {
                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(searchRes)).Result;

                //ul.srp-results >  li.s-item.s-item__pl-on-bottom > div.s-item__wrapper > div.s-item__info
                IHtmlCollection<IElement> results = document.QuerySelectorAll(@"ul.srp-results >  li");
                
                if (results.Count() == 0)
                {
                    mpr.PricesList = prices;
                    mpr.NothingFoundOrOutOfStock = true;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
                    return;
                }
                if (results.Count() > 1) mpr.MultiChoice = true;

                
                
                foreach (var result in results)
                {
                    if (result.ClassName.Contains("REWRITE_START")) break;
                    if (result.ClassName.Contains("ANSWER")) continue;
                    try
                    {
                        var url = result.QuerySelector("div.s-item__wrapper > div.s-item__info > a.s-item__link").GetAttribute("href").Split('?').First();
                        var title = result.QuerySelector("div.s-item__wrapper > div.s-item__info > a.s-item__link > div.s-item__title > span").TextContent;
                        string price = null;
                        string deliveryPrice = null;
                        bool isSponsored = false;
                        IHtmlCollection<IElement> productDetails = result.QuerySelectorAll(@"div.s-item__wrapper > div.s-item__info > div.s-item__details > div.s-item__detail > span");

                        foreach (var detail in productDetails)
                        {
                            switch (detail.ClassName)
                            {
                                case "s-item__price":
                                    price = detail.TextContent;
                                    break;
                                case "s-item__shipping s-item__logisticsCost":
                                    deliveryPrice = detail.TextContent;
                                    break;
                                default:
                                    if (detail.InnerHtml.Contains("Sponsored"))
                                        try
                                        {
                                            var tempSelect = detail.QuerySelector("span > span > span");
                                            var hidenOrNot = tempSelect.GetAttribute("aria-hidden");
                                            if (hidenOrNot != "true") isSponsored = true;
                                        }
                                        catch
                                        {
                                            //ignored
                                        }
                                    break;

                            }

                        }

                        if (isSponsored) continue;

                        if (title != null && (title.Contains(search.ToUpper().Trim()) || title.Contains(search.ToLower().Trim())))
                        {
                            decimal decPrice = 0;
                            decimal delPrice = 0;

                            if (title.Length >= 50) title = $@"{title[..47]}...";
                            

                            var priceRegex = Regex.Matches(price, @"\$\d+(?:\.\d+)?");
                            if (priceRegex.Count > 0)
                            {
                                decPrice = decimal.Parse(priceRegex.First().Value.Trim().Replace("$", "").Replace(",", "."), CultureInfo.InvariantCulture);
                            }

                            if (deliveryPrice != null)
                            {
                                var deliveryPriceRegex = Regex.Matches(deliveryPrice, @"\$\d+(?:\.\d+)?");
                                if (deliveryPriceRegex.Count > 0)
                                {
                                    delPrice = decimal.Parse(deliveryPriceRegex.First().Value.Trim().Replace("$", "").Replace(",", "."), CultureInfo.InvariantCulture);
                                }
                            }

                            prices.Add(new Prices
                            {
                                DeliveryPrice = delPrice,
                                Price = decPrice,
                                Url = url
                            });
                        }
                        else continue;
                    }
                    catch 
                    {
                        //ignored
                        continue;
                    }

                
                }

                
                if (prices.Count > 0)
                {
                    var pricesOrdered = prices.OrderBy(x => x.Price).ToList();
                    mpr.LowestPrice = pricesOrdered.First().Price;
                    mpr.PricesList = prices;

                    mainPriceResponsesList.Add(mpr);

                    await ResponseCreator.MakeResponseLog(con: con,
                        mpr: mpr,
                        request: request);
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
            }
        }
    }
}
