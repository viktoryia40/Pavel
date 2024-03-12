using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.BotFunctions;
using StockPrice.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StockPrice.Methods.AddSite
{
    internal class MakeSiteRequest
    {
        public static async void StartAddSiteRequest(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId, 
                cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $@"Which site do you want to add?",
                replyMarkup: Buttons.SiteFRM(),
                cancellationToken: cancellationToken);
        }

        public static async void EndAddSiteRequest(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string messText = update.Message.Text.Trim();

            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.ReplyToMessage.MessageId,
                cancellationToken: cancellationToken);


            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);
            await con.OpenAsync(cancellationToken);
            await con.QueryAsync($"INSERT INTO add_site_requests (`userId`, `userUserName`, `targetSite`) VALUES ('{update.Message.Chat.Id}', '@{update.Message.Chat.Username}', '{MySqlHelper.EscapeString(messText)}');");
            await con.CloseAsync(cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: AppSettings.Current.Telegram.AdminId,
                text: $@"<b>New site request!</b>

Site: {messText}
Owner Username: @{update.Message.Chat.Username}
Owner ID Telegram: {update.Message.Chat.Id}",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);


            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $@"Your request has been sent to support.",
                cancellationToken: cancellationToken);


        }
    }
}
