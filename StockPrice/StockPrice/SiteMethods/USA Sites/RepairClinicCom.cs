using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.CF_works;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockPrice.Methods;
using StockPrice.SiteMethods.Classes;
using Dapper;
using AngleSharp;
using AngleSharp.Dom;
using Dropbox.Api.TeamLog;

namespace StockPrice.SiteMethods.USA_Sites
{
    internal class RepairClinicCom
    {
        private const string Source = "repairclinic.com";
        private const string ClassSource = "RepairClinicCom";
        public static async void Parsing(DatabaseTotalResults request, List<MainPriceResponse> mainPriceResponsesList)
        {
            string search = request.Request;

            var cs =
                @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            var mpr = new MainPriceResponse()
            {
                SearchUrl = $"https://www.repairclinic.com/Shop-For-Parts?query={request.Request}",
                Source = "Repairclinic.com"
            };
            var prices = new List<Prices>();

            int cfDataId = -1;
            

            // Adding a request for CF in DB
            try
            {
                cfDataId = await CfRequestMaker.MakeCfRequest(mpr.SearchUrl);
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

            string searchResult = null;

            try
            {
                searchResult = await CfRequestGetting.GetCfRequestResponse(cfDataId);
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

            if (searchResult != null)
            {

                var config = Configuration.Default;
                using var context = BrowsingContext.New(config);
                using var document = context.OpenAsync(req => req.Content(searchResult)).Result;

                string totalUrl = null;
                string title = null;
                string availability = null;
                string priceText = null;

                
                try
                {
                    var countSearchData = document.QuerySelector(".counterLabelStrong");
                    string countSearchText = countSearchData.TextContent;
                    if (int.TryParse(countSearchText, out int countResult))
                        if (countResult > 1)
                            mpr.MultiChoice = true;
                        
                }
                catch 
                {
                    // ignored
                }

                try
                {
                    var titleData = document.QuerySelector(".partTitle");
                    var titleNotReady = titleData.InnerHtml;
                    var _spl = titleNotReady.Split('<');

                    title = _spl[0];
                    totalUrl = $"https://www.repairclinic.com/{titleData.GetAttribute("href")}";
                }
                catch 
                {
                    // ignored
                }

                try
                {
                    var priceTextData = document.QuerySelector(".mainNumber");
                    var botReadyPrice = priceTextData.TextContent.Trim();
                    var spl = botReadyPrice.Split("$");
                    if (spl.Length > 0) priceText = spl[1].Trim();
                }
                catch 
                {
                    //ignored
                }

                if (title != null)
                {
                    decimal priceDec = decimal.Parse(priceText, CultureInfo.InvariantCulture);

                    prices.Add(new()
                    {
                        Availability = "In stock",
                        Price = priceDec,
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
                    return;
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
    }
}
