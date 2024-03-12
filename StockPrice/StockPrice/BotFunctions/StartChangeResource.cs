using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockPrice.BotFunctions
{
    public sealed class StartChangeResource
    {

        public static async void EditResource(ITelegramBotClient botClient, Update update, string resource, string type, CancellationToken cancellationToken)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            await con.OpenAsync(cancellationToken);
            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userid`='{update.CallbackQuery.From.Id}';");
            await con.CloseAsync(cancellationToken);

            //bool resouce_correct = false;
            string correctDbTarget = null;
            bool newStatusResource = false;
            switch (resource)
            {
                case "AmazonCA":
                    correctDbTarget = "amazonCA";
                    newStatusResource = !userdata.AmazonCA;
                    break;
                case "AppliancepartshqCA":
                    correctDbTarget = "appliancepartshqCA";
                    newStatusResource = !userdata.AppliancepartshqCA;
                    break;
                case "PartsexpertCA":
                    correctDbTarget = "partsexpertCA";
                    newStatusResource = !userdata.PartsexpertCa;
                    break;
                case "PartselectCA":
                    correctDbTarget = "partselectCA";
                    newStatusResource = !userdata.PartselectCA;
                    break;
                case "ReliablepartsCA":
                    correctDbTarget = "reliablepartsCA";
                    newStatusResource = !userdata.ReliablepartsCA;
                    break;
                case "EasyappliancepartsCA":
                    correctDbTarget = "easyappliancepartsCA";
                    newStatusResource = !userdata.EasyappliancepartsCA;
                    break;
                case "AmresupplyCOM":
                    correctDbTarget = "amresupplyCOM";
                    newStatusResource = !userdata.AmresupplyCOM;
                    break;
                case "UniversalappliancepartsCA":
                    correctDbTarget = "universalappliancepartsCA";
                    newStatusResource = !userdata.UniversalappliancepartsCA;
                    break;
                case "GreenlineappliancespartsCA":
                    correctDbTarget = "greenlineappliancespartsCA";
                    newStatusResource = !userdata.GreenlineappliancespartsCA;
                    break;
                case "AppliancepartsCanadaCom":
                    correctDbTarget = "appliancepartsCanadaCom";
                    newStatusResource = !userdata.AppliancepartsCanadaCom;
                    break;
                case "ApwagnerCA":
                    correctDbTarget = "apwagnerCA";
                    newStatusResource = !userdata.ApwagnerCA;
                    break;
                case "MajorappliancepartsCA":
                    correctDbTarget = "majorappliancepartsCA";
                    newStatusResource = !userdata.MajorappliancepartsCA;
                    break;
                case "MarconeCanada":
                    correctDbTarget = "marconeCanada";
                    newStatusResource = !userdata.MarconeCanada;
                    break;
                case "MarconeUsa":
                    correctDbTarget = "marconeUsa";
                    newStatusResource = !userdata.MarconeUsa;
                    break;
                case "LowesCOM":
                    correctDbTarget = "lowesCOM";
                    newStatusResource = !userdata.LowesCOM;
                    break;
                case "PartswarehouseCOM":
                    correctDbTarget = "partswarehouseCOM";
                    newStatusResource = !userdata.PartswarehouseCOM;
                    break;
                case "BulbsproCOM":
                    correctDbTarget = "bulbsproCOM";
                    newStatusResource = !userdata.BulbsproCOM;
                    break;
                case "EbayCA":
                    correctDbTarget = "ebayCA";
                    newStatusResource = !userdata.EbayCA;
                    break;
                case "EncompassCOM":
                    correctDbTarget = "encompassCOM";
                    newStatusResource = !userdata.EncompassCOM;
                    break;
                case "SearsPartsDirectCOM":
                    correctDbTarget = "searsPartsDirectCOM";
                    newStatusResource = !userdata.SearsPartsDirectCOM;
                    break;
                case "CoastPartsCOM":
                    correctDbTarget = "coastPartsCOM";
                    newStatusResource = !userdata.CoastPartsCOM;
                    break;
                case "GuaranteedPartsCOM":
                    correctDbTarget = "guaranteedPartsCOM";
                    newStatusResource = !userdata.GuaranteedPartsCOM;
                    break;
                case "PartsDrCOM":
                    correctDbTarget = "partsDrCOM";
                    newStatusResource = !userdata.PartsDrCOM;
                    break;
                case "AppliancePartsProsCOM":
                    correctDbTarget = "appliancePartsProsCOM";
                    newStatusResource = !userdata.AppliancePartsProsCOM;
                    break;
                case "PartSelectCOM":
                    correctDbTarget = "partSelectCOM";
                    newStatusResource = !userdata.PartSelectCOM;
                    break;
                case "ApplianceParts365COM":
                    correctDbTarget = "applianceParts365COM";
                    newStatusResource = !userdata.ApplianceParts365COM;
                    break;
                case "ApwagnerCOM":
                    correctDbTarget = "apwagnerCOM";
                    newStatusResource = !userdata.ApwagnerCOM;
                    break;

                case "ReliablePartsCOM":
                    correctDbTarget = "reliablePartsCOM";
                    newStatusResource = !userdata.ReliablePartsCom;
                    break;
                case "EbayCOM":
                    correctDbTarget = "ebayCOM";
                    newStatusResource = !userdata.EbayCOM;
                    break;
                case "AmazonCOM":
                    correctDbTarget = "amazonCOM";
                    newStatusResource = !userdata.AmazonCOM;
                    break;
                case "CashWellsCom":
                    correctDbTarget = "cashWellsCOM";
                    newStatusResource = !userdata.CashWellsCom;
                    break;
                case "DlPartsCoCom":
                    correctDbTarget = "dlPartscoCOM";
                    newStatusResource = !userdata.DlPartsCoCom;
                    break;
                case "RepairClinicCom":
                    correctDbTarget = "repairClinicCom";
                    newStatusResource = !userdata.RepairClinicCom;
                    break;
                case "PartsTownCom":
                    correctDbTarget = "partsTownCom";
                    newStatusResource = !userdata.PartsTownCom;
                    break;
                case "AllVikingPartsCom":
                    correctDbTarget = "allVikingPartsCom";
                    newStatusResource = !userdata.AllVikingPartsCom;
                    break;

                //MarconeUsa
                //DlPartsCoCom


                default: break;
            }

            try
            {
                await con.OpenAsync(cancellationToken);
                await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"UPDATE userdata SET `{correctDbTarget}`='{Convert.ToInt32(newStatusResource)}' WHERE `userid`='{update.CallbackQuery.From.Id}';");
                await con.CloseAsync(cancellationToken);

                switch (type)
                {
                    case "CA":
                        try
                        {
                            await botClient.EditMessageTextAsync( //Sending a message with information about Canada's sites
                   chatId: update.CallbackQuery.From.Id,
                   messageId: update.CallbackQuery.Message.MessageId,
                   text:
                   @$"<b>Canada resources settings</b>
",

                   replyMarkup: Buttons.CanadaSettingsOutput(update),
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                   cancellationToken: cancellationToken);
                        }
                        catch
                        { 
                            //ignored
                        }
                        break;


                    case "USA":

                        try
                        {
                            await botClient.EditMessageTextAsync( //Sending a message with information about Canada's sites
                  chatId: update.CallbackQuery.From.Id,
                  messageId: update.CallbackQuery.Message.MessageId,
                  text:
                  @$"<b>USA resources settings</b>
",

                  replyMarkup: Buttons.UsaSettingsOutput(update),
                  parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                  cancellationToken: cancellationToken);
                        }
                        catch
                        { }

                        break;

                }
            }
            catch
            {
                await botClient.SendTextMessageAsync( //Sending a message with information about Canada's sites
                 chatId: update.CallbackQuery.From.Id,
                 
                 text:
                 @$"<b>Got error during update DataBase</b>
",
                 
                 parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                 cancellationToken: cancellationToken);

            }
        }

        public static async void EditParseSettings(ITelegramBotClient botClient, Update update, string resource, CancellationToken cancellationToken)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            await con.OpenAsync(cancellationToken);
            DatabaseUserData userdata = await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"SELECT * FROM userdata WHERE `userid`='{update.CallbackQuery.From.Id}';");
            await con.CloseAsync(cancellationToken);

            //bool resouce_correct = false;
            string correctDbTarget = null;
            bool newStatusResource = false;
            switch (resource)
            {

                case "USA":
                    correctDbTarget = "parseUSA";
                    newStatusResource = !userdata.ParseUSA;
                    break;

                case "CA":
                    correctDbTarget = "parseCanada";
                    newStatusResource = !userdata.ParseCanada;
                    break;

                default: break;
            }

            await con.OpenAsync(cancellationToken);
            await con.QueryAsync($"UPDATE userdata SET `{correctDbTarget}`='{Convert.ToInt32(newStatusResource)}' WHERE `userid`='{update.CallbackQuery.From.Id}';");
            await con.CloseAsync(cancellationToken);



            await botClient.EditMessageTextAsync( //Sending a message with information about Canada's sites
       chatId: update.CallbackQuery.From.Id,
       messageId: update.CallbackQuery.Message.MessageId,
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

    }
}
