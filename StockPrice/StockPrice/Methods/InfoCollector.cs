using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods.TableWorks;
using StockPrice.Methods.TableWorks.Classes;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.Canada_Sites;
using System.Collections.Generic;
using StockPrice.SiteMethods.USA_Sites;
using Telegram.Bot;

namespace StockPrice.Methods
{
    public class InfoCollector
    {
        public static async void MainTracker(ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            Console.WriteLine("MainTracker has been started!");
            while (!cancellationToken.IsCancellationRequested)
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                await using var con = new MySqlConnection(cs);
                con.Open();
                int count = await con.QueryFirstAsync<int>($"SELECT COUNT(*) FROM totalresults WHERE `workStarted`=0");
                if (count > 0)
                {
                    var requests = await con.QueryAsync<DatabaseTotalResults>($"SELECT * FROM totalresults WHERE `workStarted`=0");

                    foreach (var response in requests)
                    {
                        Task sender = Task.Run(() => MainSender(botClient, cancellationToken, response), cancellationToken); //Run the task responsible for sending the results
                        await con.QueryFirstOrDefaultAsync($"UPDATE totalresults SET `workStarted`='1' WHERE `MessageID`={response.MessageID} AND `ChatID`='{response.ChatID}';");

                    }
                }
                await con.CloseAsync(cancellationToken);
                Thread.Sleep(250);


            }

            
        }

        private static async void MainSender(ITelegramBotClient botClient, CancellationToken cancellationToken, DatabaseTotalResults request)
        {


            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            var settings = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={request.ChatID}");
            await con.CloseAsync(cancellationToken);

            if (settings == null)
            {
                
                var sendedMessage = await botClient.SendTextMessageAsync(
               chatId: request.ChatID,
               text: $"Your are not registered. Write a /start",
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               disableWebPagePreview: true,
               cancellationToken: cancellationToken
                   );


                await con.OpenAsync(cancellationToken);
                await con.QueryFirstOrDefaultAsync($"UPDATE totalresults SET `botMessageID`='{sendedMessage.MessageId}', `ResponseSent`='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' WHERE `ID`='{request.ID}'");
                await con.CloseAsync(cancellationToken);
                return;
            }

            var mainPriceResponsesList = new List<MainPriceResponse>();
            var tableResponses = new List<StockTable>();



            List<Task> allTasks = new();

            //  [-1] Starting a thread to work with a table
            if (settings.IsHaveStockTable) allTasks.Add(Task.Run(() => TableMainClass.ReadTable(request, tableResponses), cancellationToken));

            if (settings.ParseCanada)
            {
                
                //  [0] Starting a thread for AppliancepartshqCA
                if (settings.AppliancepartshqCA) allTasks.Add(Task.Run(() => AppliancePartsHq.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [1] Starting a thread for PartsexpertCa
                if (settings.PartsexpertCa) allTasks.Add(Task.Run(() => PartsExpert.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [2] Starting a thread for PartselectCA
                if (settings.PartselectCA) allTasks.Add(Task.Run(() => PartselectCa.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [3] Starting a thread for ReliablepartsCA
                if (settings.ReliablepartsCA) allTasks.Add(Task.Run(() => ReliablePartsCanada.Parsing(request, mainPriceResponsesList), cancellationToken));
                //allTasks.Add(Task.Run(() => ReliablePartsCanada.MakeAlternativeLinkFromReliablePartsCa(request, MainPriceResponsesList)));



                //  [4] Starting a thread for EasyappliancepartsCA
                if (settings.EasyappliancepartsCA) allTasks.Add(Task.Run(() => EasyApplianceParts.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [5] Starting a thread for AmresupplyCOM
                if (settings.AmresupplyCOM) allTasks.Add(Task.Run(() => AmreSupply.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [6] Starting a thread for AppliancepartsCanadaCom
                if (settings.AppliancepartsCanadaCom) allTasks.Add(Task.Run(() => AppliancePartsCanada.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [7] Starting a thread for ApwagnerCA
                if (settings.ApwagnerCA) allTasks.Add(Task.Run(() => ApwagnerCa.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [8] Starting a thread for BulbsproCOM
                if (settings.BulbsproCOM) allTasks.Add(Task.Run(() => BulbsPro.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [9] Starting a thread for MajorappliancepartsCA
                if (settings.MajorappliancepartsCA) allTasks.Add(Task.Run(() => MajorApplianceParts.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [10] Starting a stream for AmazonCA
                if (settings.AmazonCA) allTasks.Add(Task.Run(() => AmazonCa.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [11] Starting a thread for MarconeCanada
                if (settings.MarconeCanada) allTasks.Add(Task.Run(() => BetaMarconeCanada.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [12] Starting a thread for EbayCA
                if (settings.EbayCA) allTasks.Add(Task.Run(() => EbayCa.Parsing(request, mainPriceResponsesList), cancellationToken));
            }


            if (settings.ParseUSA)
            {
                
                //  [13] Starting a thread for EnCompass
                if (settings.EncompassCOM) allTasks.Add(Task.Run(() => EncompassCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [14] Starting a thread for CoastParts
                if (settings.CoastPartsCOM) allTasks.Add(Task.Run(() => CoastParts.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [15] Starting a thread for GuaranteedParts
                if (settings.GuaranteedPartsCOM) allTasks.Add(Task.Run(() => GuaranteedParts.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [16] Starting a thread for PartsDr
                if (settings.PartsDrCOM) allTasks.Add(Task.Run(() => Partsdr.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [17] Starting a thread for AppliancePartsPros
                if (settings.AppliancePartsProsCOM) allTasks.Add(Task.Run(() => AppliancePartsPros.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [18] Starting a flow for PartSelectCOM
                if (settings.PartSelectCOM) allTasks.Add(Task.Run(() => PartselectCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [19] Starting a thread for ApplianceParts365
                if (settings.ApplianceParts365COM) allTasks.Add(Task.Run(() => ApplianceParts365.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [20] Starting a thread for ApwagnerCom
                if (settings.ApwagnerCOM) allTasks.Add(Task.Run(() => ApwagnerCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [21] Starting a thread for SearsPartsDirect
                if (settings.SearsPartsDirectCOM) allTasks.Add(Task.Run(() => SearsPartsDirect.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [22] Starting a thread for ReliablePartsCom
                if (settings.ReliablePartsCom) allTasks.Add(Task.Run(()=> ReliablePartsUsa.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [23] Starting a thread for EbayCOM
                if (settings.EbayCOM) allTasks.Add(Task.Run(() => EbayCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [24] Starting a stream for AmazonCOM
                if (settings.AmazonCOM) allTasks.Add(Task.Run(() => AmazonCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [25] Starting a flow for DLPartscoCOM
                if (settings.DlPartsCoCom) allTasks.Add(Task.Run(()=> DlPartscoCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [26] Starting a thread for CashWellsCOM
                if (settings.CashWellsCom) allTasks.Add(Task.Run(() => CashWellsCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [27] Starting a thread for RepairClinicCom
                if (settings.RepairClinicCom) allTasks.Add(Task.Run(() => RepairClinicCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [28] Starting a thread for PartsTownCom
                if (settings.PartsTownCom) allTasks.Add(Task.Run(() => PartsTownCom.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [29] Starting a thread for AllVikingParts
                if (settings.AllVikingPartsCom) allTasks.Add(Task.Run(() => AllVikingParts.Parsing(request, mainPriceResponsesList), cancellationToken));

                //  [11] Starting a thread for MarconeUsa
                if (settings.MarconeUsa) allTasks.Add(Task.Run(() => BetaMarconeUsa.Parsing(request, mainPriceResponsesList), cancellationToken));
            }
            


            if (!request.IsMassTestingRequest)
            {
                await Task.Run(() => AutoUpdatedMessage.MainUpdater(botClient, cancellationToken, request, allTasks), cancellationToken);
            }




            Task.WaitAll(allTasks.ToArray());
            //Console.ReadLine();

            //await botClient.DeleteMessageAsync(chatId: request.ChatID, messageId: (int)request.BotMessageID, cancellationToken: cancellationToken);


            string text = MakeResponse(request, mainPriceResponsesList, tableResponses);

            if (text == null)
            {
                if (!request.IsMassTestingRequest)
                    await botClient.EditMessageTextAsync(
               chatId: request.ChatID,
               text: "No results.",
               messageId: (int)request.BotMessageID,
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               disableWebPagePreview: true,
               cancellationToken: cancellationToken
               );
                return;
            }

            if (!request.IsMassTestingRequest)
            {
                try
                {
                    var sendedMessage = await botClient.EditMessageTextAsync(
                   chatId: request.ChatID,
                   text: text + Environment.NewLine + Environment.NewLine + $"✅ Search completed",
                   messageId: (int)request.BotMessageID,
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                   disableWebPagePreview: true,
                   cancellationToken: cancellationToken
                       );


                    await con.OpenAsync(cancellationToken);
                    await con.QueryFirstOrDefaultAsync($"UPDATE totalresults SET `botMessageID`='{sendedMessage.MessageId}', `ResponseSent`='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' WHERE `ID`='{request.ID}'");
                    await con.CloseAsync(cancellationToken);
                }
                catch
                {
                    Thread.Sleep(5000);
                    var sendedMessage = await botClient.EditMessageTextAsync(
                   chatId: request.ChatID,
                   text: text + Environment.NewLine + Environment.NewLine + $"✅ Search completed",
                   messageId: (int)request.BotMessageID,
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                   disableWebPagePreview: true,
                   cancellationToken: cancellationToken
                       );


                    await con.OpenAsync(cancellationToken);
                    await con.QueryFirstOrDefaultAsync($"UPDATE `totalresults` SET `botMessageID`='{sendedMessage.MessageId}', `ResponseSent`='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' WHERE `ID`='{request.ID}'");
                    await con.CloseAsync(cancellationToken);

                }
            }
            







        }




        private static string? ModifyTitle(string? title, string search)
        {
            if (title == null) return title;
            string newSearch = search.ToUpper();
            while (true)
            {
                if (newSearch.Length <= 2) break; ;

                if (title.Contains(newSearch))
                {
                    return title.Replace(newSearch, $"<b>{newSearch}</b>");
                }
                else
                {
                    newSearch = newSearch.Remove(newSearch.Length - 1, 1);
                }
            }

            newSearch = search.ToLower();

            while (true)
            {
                if (newSearch.Length <= 2) return title;

                if (title.Contains(newSearch))
                {
                    return title.Replace(newSearch, $"<b>{newSearch}</b>");
                }
                else
                {
                    newSearch = newSearch.Remove(newSearch.Length - 1, 1);
                }
            }
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="request">Request from BD</param>
        /// <param name="MainPriceResponsesList">List with results search</param>
        /// <returns></returns>
        private static string MakeResponse(
            DatabaseTotalResults request,
            List<MainPriceResponse> MainPriceResponsesList,
            List<StockTable> TableResponses)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";

            string total_text = null;



            MainPriceResponsesList = MainPriceResponsesList.OrderBy(x => x.LowestPrice).ToList(); //Sorted by price from lower to higher.

            var ReadySendMainPriceResponseList = MainPriceResponsesList.Where(x => x.PricesList != null && x.PricesList.Count > 0 && !x.NoAnswerOrError && !x.NothingFoundOrOutOfStock).ToList();
            var ResponseWithOutOfStockOrNnothingFound = MainPriceResponsesList.Where(x => x.NothingFoundOrOutOfStock && !x.NoAnswerOrError).ToList();
            var ResponseWithErrors = MainPriceResponsesList.Where(x => x.NoAnswerOrError).ToList();

            List<string> responses_stringify = new();

            foreach (var taken_resp in ReadySendMainPriceResponseList)
            {
                string ready_string = null;

                if (taken_resp.PricesList.Count > 1)
                {
                    taken_resp.PricesList = taken_resp.PricesList.OrderBy(x => x.Price).ToList();
                }

                if (taken_resp.MultiChoice)
                    if(taken_resp.AlternativeSearchLink == null)
                        if (taken_resp.Additional == null)
                            ready_string += $@"<b>{taken_resp.Source}-></b><a href='{taken_resp.SearchUrl}'>(All results)</a>->  ";
                        else
                            ready_string += $@"<b>{taken_resp.Source}</b>{taken_resp.Additional}<a href='{taken_resp.SearchUrl}'>(All results)</a>->  ";
                    else
                        if (taken_resp.Additional == null)
                        ready_string += $@"<b>{taken_resp.Source}-></b><a href='{taken_resp.AlternativeSearchLink}'>(All results)</a>->  ";
                    else
                        ready_string += $@"<b>{taken_resp.Source}</b>{taken_resp.Additional}<a href='{taken_resp.AlternativeSearchLink}'>(All results)</a>->  ";


                else
                    if (taken_resp.Additional == null)
                    ready_string += $@"<b>{taken_resp.Source}-></b>  ";
                else
                    ready_string += $@"<b>{taken_resp.Source}</b>{taken_resp.Additional}->  ";


                List<string> pricesStringify = new();

                foreach (var price in taken_resp.PricesList)
                {

                    string price_prepare = null;
                    //if (price.DeliveryPrice > 0) price_prepare += $@" 📦${price.DeliveryPrice.ToString().Replace(',', '.')}";

                    if (price.DoublePrice != null)
                    {
                        if (price.Availability != null) price_prepare += $"{price.Availability}-> ";
                        if (price.Title != null && taken_resp.PricesList.IndexOf(price) == 0) price_prepare += $@" {price.Title.Substring(0, Math.Min(price.Title.Length, 80)).Replace("#", "")} ";
                        if (taken_resp.AlternativeLink == null) price_prepare += $@"<a href='{price.Url}'>{price.DoublePrice}</a>";
                        else
                        {
                            var db_price_split = price.DoublePrice.Split('/');
                            price_prepare += $@"<a href='{price.Url}'>{db_price_split[0].Trim()}</a> / <a href='{taken_resp.AlternativeLink}'>{db_price_split[1].Trim()}</a>";
                        }
                    }
                    else
                    {

                        if (price.Availability != null) price_prepare += $"{price.Availability}-> ";
                        if (price.Title != null && taken_resp.PricesList.IndexOf(price) == 0) price_prepare += $@" {price.Title.Substring(0, Math.Min(price.Title.Length, 80)).Replace("#", "")} ";
                        price_prepare += $@"<a href='{price.Url}'>${price.Price.ToString().Replace(',', '.')}</a>";
                    }


                    if (price.DeliveryDays != null) price_prepare += $"_{price.DeliveryDays}d";
                    if (price.DeliveryPrice > 0) price_prepare += $"+{price.DeliveryPrice.ToString().Replace(",", ".")}";

                    pricesStringify.Add(price_prepare);
                }

                ready_string += string.Join("; ", pricesStringify);
                if (taken_resp.Locations != null && taken_resp.Locations.Count > 0)
                    ready_string += Environment.NewLine + "• " + String.Join(Environment.NewLine + "• ", taken_resp.Locations);

                if (taken_resp.EndAdditional != null)
                    ready_string += $@" {taken_resp.EndAdditional}";

                total_text += ready_string + Environment.NewLine;

                

            }

            if (ResponseWithOutOfStockOrNnothingFound.Count > 0)
            {
                ResponseWithOutOfStockOrNnothingFound = ResponseWithOutOfStockOrNnothingFound.OrderBy(x => x.Source).ToList();
                total_text += Environment.NewLine + Environment.NewLine + @"<b>Nothing found / Out of stock</b>";
                foreach (var response in ResponseWithOutOfStockOrNnothingFound)
                {
                    total_text += Environment.NewLine + $@"<a href='{response.SearchUrl}'>{response.Source}</a>";
                }
            }

            if (ResponseWithErrors.Count > 0)
            {
                ResponseWithErrors = ResponseWithErrors.OrderBy(x=>x.Source).ToList();
                total_text += Environment.NewLine + Environment.NewLine + @"<b>No answer / Error</b>";
                foreach (var response in ResponseWithErrors)
                {
                    total_text += Environment.NewLine + $@"<a href='{response.SearchUrl}'>{response.Source}</a>";
                }
            }

            if (TableResponses.Count > 0)
            {
                total_text += Environment.NewLine + $@"<b>Stock data:</b>";
                foreach (var stock_row in TableResponses)
                {
                    if (stock_row.Sku != null) total_text += Environment.NewLine + $@"<b>#: </b>{stock_row.Sku}";
                    if (stock_row.Name != null) total_text += Environment.NewLine + $@"<b>Name: </b>{stock_row.Name}";
                    if (stock_row.Quantity != null) total_text += Environment.NewLine + $@"<b>Qty: </b>{stock_row.Quantity}";
                    if (stock_row.Condition != null) total_text += $@" // <b>Cond: </b>{stock_row.Condition}";
                    if (stock_row.WareHouse != null) total_text += $@" // <b>Wh#: </b>{stock_row.WareHouse}";
                    if (stock_row.Comment != null) total_text += Environment.NewLine + $@"<b>Cmt: </b>{stock_row.Comment}";
                }
            }
            else
            {
                total_text += Environment.NewLine + $@"<b>You do not have part {request.Request} or replaces in your stock</b>";
            }

            if (total_text != null) total_text = total_text.Trim();
            var _j_response_sended = JsonConvert.SerializeObject(ReadySendMainPriceResponseList, Formatting.None);
            var _j_response_full = JsonConvert.SerializeObject(MainPriceResponsesList.OrderBy(x => x.Source).ToList(), Formatting.None);

            string escaped_sended = MySqlHelper.EscapeString(_j_response_sended);
            string escaped_full = MySqlHelper.EscapeString(_j_response_full);
            using var con = new MySqlConnection(cs);
            con.Open();

            try
            {
                con.QueryFirstOrDefault($"UPDATE totalresults SET `sendedResult`='{escaped_sended}' WHERE `ID`={request.ID};");
                con.QueryFirstOrDefault($"UPDATE totalresults SET `fullResult`='{escaped_full}' WHERE `ID`={request.ID};");
            }
            catch
            {

            }
            con.Close();
            return total_text;
        }



    }


}
