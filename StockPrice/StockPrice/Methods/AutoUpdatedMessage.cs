using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods.TableWorks.Classes;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using Telegram.Bot;

namespace StockPrice.Methods
{
    public class AutoUpdatedMessage
    {
        public static async void MainUpdater(ITelegramBotClient botClient, CancellationToken cancellationToken, DatabaseTotalResults request, List<Task> allTasks)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            var settings = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={request.ChatID}");
            await con.CloseAsync(cancellationToken);


            var mainPriceResponsesList = new List<MainPriceResponse>();
            var tableResponses = new List<StockTable>();

            var photosFromSitesList = new List<PhotosFromSites> { };

            const string searchViewOne = "⏳";
            string searchViewTwo = "⌛️";
            string nowSearchView = searchViewOne;
            string nowText = $"{nowSearchView} Searching..";

            var sendedMessage = await botClient.SendTextMessageAsync(
              chatId: request.ChatID,
              text: $"{nowSearchView} Searching..",
              replyToMessageId: int.Parse(request.MessageID.ToString()),
              parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
              disableWebPagePreview: true,
              cancellationToken: cancellationToken
              );



            await con.OpenAsync(cancellationToken);
            await con.QueryFirstOrDefaultAsync($"UPDATE totalresults SET `botMessageID`='{sendedMessage.MessageId}' WHERE `ID`='{request.ID}'");
            await con.CloseAsync(cancellationToken);
            request.BotMessageID = sendedMessage.MessageId;

            int responseCount = 0;

            while (!AllTaskReady(allTasks))
            {
                mainPriceResponsesList.Clear();
                tableResponses.Clear();

                var resultsInDb = await con.QueryAsync<DatabaseResponseTempDB>($"SELECT * FROM response_temp_db WHERE `RequestID`={request.ID};");
                if (resultsInDb.Count() == responseCount)
                {
                    if (nowSearchView.Equals("⏳"))
                    {
                        try
                        {
                            await botClient.EditMessageTextAsync(
                              chatId: request.ChatID,
                              text: $"{nowText.Replace(nowSearchView, "⌛️")}",
                              messageId: int.Parse(request.BotMessageID.ToString()),
                              parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                              disableWebPagePreview: true,
                              cancellationToken: cancellationToken
                              );

                            Thread.Sleep(500);
                            nowText = $"{nowText.Replace(nowSearchView, "⌛️")}";
                            nowSearchView = "⌛️";

                            continue;
                        }
                        catch
                        {
                            try
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: request.ChatID,
                                    text: $"{nowText.Replace(nowSearchView, "⏳/")}",
                                    messageId: int.Parse(request.BotMessageID.ToString()),
                                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                                    disableWebPagePreview: true,
                                    cancellationToken: cancellationToken
                                );
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            Thread.Sleep(500);
                            nowText = $"{nowText.Replace(nowSearchView, "⌛️/")}";
                            nowSearchView = "⏳/";

                            continue;
                        }
                    }

                    if (nowSearchView.Equals("⌛️"))
                    {
                        try
                        {
                            await botClient.EditMessageTextAsync(
                              chatId: request.ChatID,
                              text: $"{nowText.Replace(nowSearchView, "⏳")}",
                              messageId: int.Parse(request.BotMessageID.ToString()),
                              parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                              disableWebPagePreview: true,
                              cancellationToken: cancellationToken
                              );

                            Thread.Sleep(500);
                            nowText = $"{nowText.Replace(nowSearchView, "⌛️")}";
                            nowSearchView = "⏳";

                            continue;
                        }
                        catch
                        {

                            try
                            {
                                await botClient.EditMessageTextAsync(
                                    chatId: request.ChatID,
                                    text: $"{nowText.Replace(nowSearchView, "⏳/")}",
                                    messageId: int.Parse(request.BotMessageID.ToString()),
                                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                                    disableWebPagePreview: true,
                                    cancellationToken: cancellationToken
                                );
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            Thread.Sleep(500);
                            nowText = $"{nowText.Replace(nowSearchView, "⌛️/")}";
                            nowSearchView = "⏳/";

                            continue;
                        }
                    }





                }
                else responseCount = resultsInDb.Count();

                foreach (var result in resultsInDb)
                {
                    switch (result.Type)
                    {
                        case "Price":
                            mainPriceResponsesList.Add(JsonConvert.DeserializeObject<MainPriceResponse>(result.Data));
                            break;
                        case "Stock":
                            tableResponses.Add(JsonConvert.DeserializeObject<StockTable>(result.Data));
                            break;
                    }
                }


                string text = MakeResponse(request, mainPriceResponsesList, tableResponses);



                if (nowSearchView.Equals("⏳"))
                {
                    nowSearchView = "⌛️";

                    try
                    {
                        await botClient.EditMessageTextAsync(
                            chatId: request.ChatID,
                            text: text + Environment.NewLine + Environment.NewLine + $"{nowSearchView} Searching..",
                            messageId: int.Parse(request.BotMessageID.ToString()),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken
                        );
                    }
                    catch
                    {
                        //ignored
                    }



                    nowText = text + Environment.NewLine + Environment.NewLine + $"{nowSearchView} Searching..";
                    Thread.Sleep(500);
                    continue;
                }
                if (nowSearchView.Equals("⌛️"))
                {
                    nowSearchView = "⏳";

                    try
                    { 
                        await botClient.EditMessageTextAsync(
                          chatId: request.ChatID,
                          text: text + Environment.NewLine + Environment.NewLine + $"{nowSearchView} Searching..",
                          messageId: int.Parse(request.BotMessageID.ToString()),
                          parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                          disableWebPagePreview: true,
                          cancellationToken: cancellationToken
                          );
                    }
                    catch
                    {
                        //ignored
                    }



                    nowText = text + Environment.NewLine + Environment.NewLine + $"{nowSearchView} Searching..";
                    Thread.Sleep(1000);
                    continue;
                }




            }

            await con.OpenAsync(cancellationToken); 
            await con.QueryFirstOrDefaultAsync($"DELETE FROM `response_temp_db` WHERE  `RequestID`={request.ID};");
            await con.CloseAsync(cancellationToken);

        }


        private static bool AllTaskReady(List<Task> allTasks)
        {
            foreach (var task in allTasks)
            {
                if (!task.IsCompleted) return false;
            }
            return true;
        }

        private static bool FindRandomResult(dynamic diagram)
        {

            if (diagram.Priority == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string ModifyTitle(string title, string search)
        {
            if (title == null) return title;
            string new_search = search.ToUpper();
            while (true)
            {
                if (new_search.Length <= 2) break; ;

                if (title.Contains(new_search))
                {
                    return title.Replace(new_search, $"<b>{new_search}</b>");
                }
                else
                {
                    new_search = new_search.Remove(new_search.Length - 1, 1);
                }
            }

            new_search = search.ToLower();

            while (true)
            {
                if (new_search.Length <= 2) return title;

                if (title.Contains(new_search))
                {
                    return title.Replace(new_search, $"<b>{new_search}</b>");
                }
                else
                {
                    new_search = new_search.Remove(new_search.Length - 1, 1);
                }
            }
        }



        /// <summary>
        /// Make a intermediate response
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
                    if (taken_resp.AlternativeSearchLink == null)
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


                List<string> prices_stringify = new();

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

                    prices_stringify.Add(price_prepare);
                }

                ready_string += string.Join("; ", prices_stringify);
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
                ResponseWithErrors = ResponseWithErrors.OrderBy(x => x.Source).ToList();
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
            if (total_text != null) total_text = total_text.Trim();


            MainPriceResponsesList.Clear();
            TableResponses.Clear();
            return total_text;
        }




    }
}
