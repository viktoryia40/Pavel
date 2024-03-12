using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StockPrice.BotFunctions
{
    public sealed class EditDbData
    {
        public static async void AskEditAmazonDays(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            await con.OpenAsync(cancellationToken);
            var nowUser = await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"SELECT * FROM userdata WHERE `userId`='{update.Message.Chat.Id}';");
            await con.CloseAsync(cancellationToken);

            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.MessageId,
                cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $"<b>Your actual amazon days count:</b> {nowUser.MaxDeliveryDays}",
                replyMarkup: Buttons.AmazonDaysCountChangeButton(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }

        public static async void StartEditAmazonDays(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                text: $@"Please enter new day count",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }
        public static async void FinishEditAmazonDays(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.DeleteMessageAsync(
                chatId: update.Message.Chat.Id,
                messageId: update.Message.ReplyToMessage.MessageId,
                cancellationToken: cancellationToken);

            long userId = update.Message.Chat.Id;
            if(int.TryParse(update.Message.Text, out int newDayCount))
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                await using var con = new MySqlConnection(cs);
                await con.OpenAsync(cancellationToken);
                var result = await con.QueryFirstOrDefaultAsync($"UPDATE `userdata` SET `maxDeliveryDays`={newDayCount} WHERE `userId`='{userId}'");
                await con.CloseAsync(cancellationToken);

                await botClient.DeleteMessageAsync(
                    chatId: update.Message.Chat.Id,
                    messageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);

                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $@"Number of days successfully updated.",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                   chatId: update.Message.Chat.Id,
                   text: $@"We got some kind of error while trying to change the number of days. Please check if you entered the correct number.",
                   cancellationToken: cancellationToken);
            }
            
        }
    }
}
