using StockPrice.BotFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockPrice.ReplacesModule
{
    public class AddSkuMethods
    {
        public static async void StartAddSkuCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $@"Please enter SKU",
                replyMarkup: Buttons.SkuFRM(),
                cancellationToken: cancellationToken);
        }

        public static async void StartAddSkuCallback(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: update.CallbackQuery.Id,
                cancellationToken: cancellationToken
                );
            await botClient.DeleteMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                messageId: update.CallbackQuery.Message.MessageId
                );
            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                text: $@"Enter a new SKU",
                replyMarkup: Buttons.SkuFRM(),
                cancellationToken: cancellationToken);
        }

        public static async void MakeReplaces(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var our_message = await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $@"Searching replaces, please wait.",
                cancellationToken: cancellationToken);

            AnalogsUpdater.MakeReplaces(update.Message.Text, out string ready_replaces, out string ready_title);
            

        }

    }
}
