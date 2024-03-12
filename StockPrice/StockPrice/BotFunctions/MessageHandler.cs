using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using StockPrice.ReplacesModule;
using StockPrice.Methods.DropboxClass;
using StockPrice.BotFunctions;
using StockPrice.Methods.AddSite;
using Telegram.Bot.Types.Enums;

namespace StockPrice.BotFunctions
{
    /// <summary>
    /// Bot commands Handler
    /// </summary>
    public sealed class MessageHandler
    {
        public static async void TextMessageHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var message = update.Message;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            if (message.Type.ToString().Equals("Text"))
            {
                // /start
                if (message.Text.ToString().Equals("/start") || message.Text.ToString().Equals("/Start"))
                {
                    StartCommand(botClient, update, cancellationToken); return;
                }

                

                await con.OpenAsync(cancellationToken);
                var checkUserRegistered = con.Query<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`='{update.Message.Chat.Id}';");
                await con.CloseAsync(cancellationToken);

                if (!checkUserRegistered.Any())
                {
                    await botClient.SendTextMessageAsync(
                       chatId: update.Message.Chat.Id,
                       text: $"You are not registered!",
                       replyToMessageId: update.Message.MessageId,
                       cancellationToken: cancellationToken
                       );
                    return;
                }

                /* // /add
                 if (message.Text.ToString().Equals("/add") || message.Text.ToString().Equals("/Add"))
                 {
                     AddSkuMethods.StartAddSkuCommand(botClient, update, cancellationToken); return;
                 }*/

                if (message.Text.ToString().Equals("/addsite"))
                {

                    MakeSiteRequest.StartAddSiteRequest(botClient, update, cancellationToken);
                    return;
                }

                if (message.Text.ToString().Equals("/changecountry"))
                {
                    StartChangeCountry(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("/profile"))
                {
                    MyData(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("/stockfile"))
                {
                    StockFileData(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("/help"))
                {
                    HelpMe(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("/canadasettings"))
                {
                    CanadaSitesSettings(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("/usasettings"))
                {
                    UsaSitesSettings(botClient, update, cancellationToken); return;
                }

                if (message.Text.ToString().Equals("/amazondays"))
                {
                    EditDbData.AskEditAmazonDays(botClient, update, cancellationToken);
                    return;
                    
                }


                if (message.ReplyToMessage != null)
                {

                    if (message.ReplyToMessage.Text.ToString().Equals("Please enter your DropBox email") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        DropBoxClass.MakeFile(botClient, update, cancellationToken); return;
                    }
                    if (message.ReplyToMessage.Text.ToString().Equals("Please enter new day count") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        EditDbData.FinishEditAmazonDays(botClient, update, cancellationToken); return;
                    }

                    if (message.ReplyToMessage.Text.ToString().Equals("Which site do you want to add?") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        MakeSiteRequest.EndAddSiteRequest(botClient, update, cancellationToken); return;
                    }

                    /*if (message.ReplyToMessage.Text.ToString().Equals("Please enter SKU") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        //DropBoxClass.MakeFile(botClient, update, cancellationToken); return;
                    }

                    if (message.ReplyToMessage.Text.ToString().Equals("Enter a new SKU") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        //DropBoxClass.MakeFile(botClient, update, cancellationToken); return;
                    }*/

                    /*if (message.ReplyToMessage.Text.ToString().Equals("Provide a link to your google spreadsheet") && message.ReplyToMessage.From.Id == botClient.BotId)
                    {
                        UpdateGoogleSheetID(botClient, update, cancellationToken); return;
                    }*/
                }



                //В случае, если варианты выше не подошли - совершаем добавление в БД задачи.

               


               

                else
                {
                    await con.OpenAsync(cancellationToken);
                    await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"INSERT INTO totalresults (`MessageID`, `request`, `ChatID`, `RequestStart`) VALUES ('{update.Message.MessageId}', '{update.Message.Text}', '{update.Message.Chat.Id}', '{DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss")}');");
                    await con.CloseAsync(cancellationToken);
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: $"The task has been added to the task list.",
                        replyToMessageId: update.Message.MessageId,
                        cancellationToken: cancellationToken
                        );
                }


            }

        }

        public static void CallBackQueryHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var callback = update.CallbackQuery;
            //CloseSettingsMenu



            if (callback.Data.ToString().Equals("CloseSettingsMenu"))
            {
                CloseMenu(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ShowMoreQuery"))
            {
                ShowMoreAction(botClient, update, cancellationToken);
                return;
            }
            if (callback.Data.ToString().Equals("ShowLessQuery"))
            {
                ShowLessAction(botClient, update, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Equals("CreateTableCallback"))
            {
                DropBoxClass.StartMakingTable(botClient, update, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Equals("CreateNewTableCallback"))
            {
                DropBoxClass.StartMakingTable(botClient, update, cancellationToken, true);
                return;
            }

            if (callback.Data.ToString().Equals("AllDoneTableCallback"))
            {
                DropBoxClass.FinishMakingTable(botClient, update, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Contains("ChangeResourceCallback"))
            {
                var spl = callback.Data.ToString().Split('_');
                string resource = spl[1];
                string type = spl[2];
                StartChangeResource.EditResource(botClient, update, resource, type, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Contains("ChangeParseCallback"))
            {
                var spl = callback.Data.ToString().Split('_');
                string resource = spl[1];
                StartChangeResource.EditParseSettings(botClient, update, resource, cancellationToken);
                return;
            }

            if (callback.Data.ToString().Equals("ChangeNumberOfDeliveryDays"))
            {
                EditDbData.StartEditAmazonDays(botClient, update, cancellationToken); return;
                return;
            }

            //ChooseCountry
            if (callback.Data.ToString().Contains("ChooseCountry"))
            {
                var spl = callback.Data.ToString().Split('_');
                string country = spl[1];
                SelectCountry(botClient, update, country, cancellationToken);
            }

           





        }




        /// <summary>
        /// Register/update menu command
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void StartCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long userId = update.Message.Chat.Id;
            string username = null;
            if (update.Message.Chat.Username != null) username = update.Message.Chat.Username;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            int count = await con.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={userId}");
            await con.CloseAsync(cancellationToken);

            if (count == 0) //If there is no user data in the user database
            {
                con.Open();
                await con.QueryAsync($"INSERT INTO userdata (`userId`, `username`) VALUES ('{userId}', '{username}')");

                await botClient.SendTextMessageAsync( //Sending a message about successful registration
                chatId: userId,
                text: $@"Please choose your country.",
                replyMarkup: Buttons.ChoseYourCountryButton(),
                cancellationToken: cancellationToken);
            }

            else
            {
                await botClient.SendTextMessageAsync( //Sending a message about successful registration
                    chatId: userId,
                    text: $@"Bot is ready to go.",
                    cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Request for change country
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void StartChangeCountry(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long userId = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            await botClient.DeleteMessageAsync(
                chatId: userId,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken
            );

            await botClient.SendTextMessageAsync( 
                chatId: userId,
                text: $@"Please choose your country.",
                replyMarkup: Buttons.ChoseYourCountryButton(),
                cancellationToken: cancellationToken);
            
        }

        /// <summary>
        /// Update menu command
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="selectedCountry"></param>
        private static async void SelectCountry(ITelegramBotClient botClient, Update update,  string selectedCountry, CancellationToken cancellationToken)
        {
            long userId = update.CallbackQuery.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            int count = await con.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*) FROM userdata WHERE `userId`={userId}");
            await con.CloseAsync(cancellationToken);
            
                con.Open();
                switch (selectedCountry)
                {
                    case "Canada":
                        await con.QueryAsync($"UPDATE `userdata` SET `parseCanada`=1, `parseUSA`=0 WHERE `userId`={userId}");
                        break;
                    case "USA":
                        await con.QueryAsync($"UPDATE `userdata` SET `parseUSA`=1, `parseCanada`=0 WHERE `userId`={userId}");
                        break;
                }

                await botClient.SendTextMessageAsync( //Sending a message about successful registration
                    chatId: userId,
                    text: $@"You select country: {selectedCountry}.",
                    cancellationToken: cancellationToken);
                try
                {
                    await botClient.DeleteMessageAsync(
                        chatId: userId,
                        messageId: update.CallbackQuery.Message.MessageId, 
                        cancellationToken: cancellationToken);
                }
                catch
                {
                    // ignored
                }
        }

        /// <summary>
        /// Register/update menu command
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void CloseMenu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long userId = update.CallbackQuery.From.Id;



            try
            {
                await botClient.DeleteMessageAsync(
                    chatId: userId,
                    messageId: update.CallbackQuery.Message.MessageId, 
                    cancellationToken: cancellationToken);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Response to user with info about his data
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void MyData(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            long userId = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            DatabaseUserData userdata = await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={userId}");
            await con.CloseAsync(cancellationToken);

            await botClient.SendTextMessageAsync( //Sending a message with user information
               chatId: userId,
               text:
               @$"<b>User</b>.
Your ID in the database: {userdata.ID}
Your Telegram ID: {userdata.UserId}
Maximum delivery time for Amazon: {userdata.MaxDeliveryDays},
Availability of the Stock Table: {userdata.IsHaveStockTable} 
",

               replyMarkup: Buttons.MyProfileSettingsOutput(update),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);

        }

        /// <summary>
        /// Response to user with info about his data
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void StockFileData(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            long userId = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            DatabaseUserData userdata = await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`={userId}");
            await con.CloseAsync(cancellationToken);

            await botClient.SendTextMessageAsync( //Sending a message with user information
                chatId: userId,
                text:
                @$"<b>Stock file data</b>
The URL of your stock table: {userdata.StockTableUrl}",

                replyMarkup: Buttons.MyStockDataMarkup(update),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);

        }

        /// <summary>
        /// Response to user with info about his Canada resources data
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void CanadaSitesSettings(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);


            await botClient.SendTextMessageAsync( //Sending a message with information about Canada's sites
               chatId: update.Message.Chat.Id,
               text:
               @$"<b>Canada resources settings</b>
",

               replyMarkup: Buttons.CanadaSettingsOutput(update),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Response to user with info about his USA resources data
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void UsaSitesSettings(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);


            await botClient.SendTextMessageAsync( //Sending a message with information about Canada's sites
               chatId: update.Message.Chat.Id,
               text:
               @$"<b>USA resources settings</b>
",

               replyMarkup: Buttons.UsaSettingsOutput(update),
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);
        }



        /// <summary>
        /// Response to user with help info
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void HelpMe(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync( //Sending a message with user information
               chatId: update.Message.Chat.Id,
               text:
               @$"<b>Help section</b>

For the bot to work, you need to send the SKU model.
When sending a file, each SKU will be processed sequentially.
You can select a country or individual sites in the settings.
Email pricebotappliance@gmail.com",
               
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
               cancellationToken: cancellationToken);
        }




        private static async void ShowMoreAction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            long user_id = update.CallbackQuery.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var result = con.QueryFirstOrDefault<DatabaseTotalResults>($"SELECT * FROM totalresults WHERE `ChatID`={user_id} AND `botMessageID`={update.CallbackQuery.Message.MessageId}");
            con.Close();

            if (string.IsNullOrEmpty(result.fullResult)) await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken,
                text: "There are no more results."
                );
            else
            {
                byte[] encodedDataAsBytesFull = Convert.FromBase64String(result.fullResult);
                byte[] encodedDataAsBytesReduced = Convert.FromBase64String(result.SendedResult);
                string full_text = System.Text.Encoding.UTF8.GetString(encodedDataAsBytesReduced) + System.Text.Encoding.UTF8.GetString(encodedDataAsBytesFull);


                if (full_text.Length <= 4096)
                    await botClient.EditMessageTextAsync(
                    chatId: result.ChatID,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: full_text,
                    replyMarkup: Buttons.ShowLessIKM(),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    disableWebPagePreview: true,
                    cancellationToken: cancellationToken
                    );
                else
                {
                    var splited = full_text.Split(Environment.NewLine);
                    string new_total_message = null;
                    foreach (var line in splited)
                    {
                        if ((new_total_message + line + Environment.NewLine).Length > 4096)
                        {
                            await botClient.EditMessageTextAsync(
                            chatId: result.ChatID,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: new_total_message,
                            replyMarkup: Buttons.ShowLessIKM(),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken
                            );
                            new_total_message = null;
                        }
                        else
                        {
                            new_total_message += line + Environment.NewLine;
                        }
                    }
                }
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        private static async void ShowLessAction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long user_id = update.CallbackQuery.From.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var result = con.QueryFirstOrDefault<DatabaseTotalResults>($"SELECT * FROM totalresults WHERE `ChatID`={user_id} AND `botMessageID`={update.CallbackQuery.Message.MessageId}");
            con.Close();

            if (string.IsNullOrEmpty(result.fullResult)) await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken,
                text: "There are no more results."
                );
            else
            {
                await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );

                byte[] encodedDataAsBytesReduced = Convert.FromBase64String(result.SendedResult);
                string full_text = System.Text.Encoding.UTF8.GetString(encodedDataAsBytesReduced);


                if (full_text.Length <= 4096)
                    try
                    {
                        await botClient.EditMessageTextAsync(
                        chatId: result.ChatID,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: full_text,
                        replyMarkup: Buttons.ShowMoreIKM(),
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        disableWebPagePreview: true,
                        cancellationToken: cancellationToken
                        );
                    }
                    catch
                    {
                        await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: update.CallbackQuery.Id,
                        text: "Error callback!",
                        cancellationToken: cancellationToken
                        );
                        return;
                    }
                else
                {
                    var splited = full_text.Split(Environment.NewLine);
                    string new_total_message = null;
                    foreach (var line in splited)
                    {
                        if ((new_total_message + line + Environment.NewLine).Length > 4096)
                        {
                            await botClient.EditMessageTextAsync(
                            chatId: result.ChatID,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: new_total_message,
                            replyMarkup: Buttons.ShowMoreIKM(),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            disableWebPagePreview: true,
                            cancellationToken: cancellationToken
                            );
                            new_total_message = null;
                        }
                        else
                        {
                            new_total_message += line + Environment.NewLine;
                        }
                    }
                }

            }


        }
    }
}
