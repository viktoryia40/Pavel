using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockPrice.BotFunctions
{
    /// <summary>
    /// Class with methods that response to user a request to set other rows count
    /// </summary>
    public sealed class StartEditRows
    {
        /// <summary>
        /// Response to user a request for setting new Diagram WEB
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountDiagramWeb(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Укажите желаемое количество строк Diagram WEB в сокращенной выдаче",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Partlist PDF
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountPartlistPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Укажите желаемое количество строк Partlist PDF в сокращенной выдаче",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Tech Sheet PDF
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountTechSheetPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Укажите желаемое количество строк Tech Sheet PDF в сокращенной выдаче",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Service Manual PDF
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountServiceManualPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Укажите желаемое количество строк Service Manual PDF в сокращенной выдаче",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Service Manual WEB
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountServiceManualWEB(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Укажите желаемое количество строк Service Manual WEB в сокращенной выдаче",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Wiring Sheet PDF
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountWiringSheetPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Specify the desired number of lines of Wiring Sheet PDF in abbreviated output",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Response to user a request for setting new Service Pointer PDF
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        public static async void SetCountServicePointerPDF(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                text: $@"Укажите желаемое количество строк Service Pointer PDF в сокращенной выдаче",
                replyMarkup: Buttons.IntegerFRM(),
                cancellationToken: cancellationToken);
        }

    }
}
