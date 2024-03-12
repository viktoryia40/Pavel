using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.BotFunctions;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockPrice.MassTestingModule
{
    public class MainCreator
    {
        public static async void MakeNewRequest(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long chatId = update.Message.Chat.Id;
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            await con.OpenAsync(cancellationToken);
            var userData = await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"SELECT * FROM `userdata` WHERE `userId`='{chatId}'");
            await con.CloseAsync(cancellationToken);


            if (!userData.CanUseMassTesting)
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                    text: @"Send SKU for information.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
                return;
            }


            string fileId = update.Message.Document.FileId;
            List<string> SKUs = new();

            try
            {

                var file = await botClient.GetFileAsync(fileId);
                using var saveImageStream = new MemoryStream();
                var dwn = botClient.DownloadFileAsync(file.FilePath, saveImageStream, cancellationToken);
                dwn.Wait(cancellationToken);
                SKUs = Encoding.UTF8.GetString(saveImageStream.ToArray()).Split('\r','\n').ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
                return;

            }

            string jData = JsonConvert.SerializeObject(SKUs, Formatting.None);



            await con.OpenAsync(cancellationToken);
            var makingRequest = con.QueryFirstOrDefault<DatabaseMassTestingRequestsData>($@"INSERT INTO `mass_testing_requests` (`InitiatorID`, `SkuList`) VALUES ('{chatId}', '{jData}');
SELECT * FROM `mass_testing_requests` WHERE `ID` = LAST_INSERT_ID();"); //Get a data
            await con.CloseAsync(cancellationToken);


            await botClient.SendTextMessageAsync(chatId: chatId,
                text: $@"You created an application for mass testing\. 
The unique ID of your request is `{makingRequest.ID}`\.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);


        }
    }
}
