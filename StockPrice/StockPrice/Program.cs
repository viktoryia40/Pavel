using StockPrice.BotFunctions;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.Methods.Authorization;
using StockPrice.Settings;
using StockPrice.SiteMethods.Canada_Sites;
using StockPrice.Methods.TableWorks;
using StockPrice.ReplacesModule;
using System.Diagnostics;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StockPrice.MassTestingModule;
using StockPrice.SiteMethods.USA_Sites;

/*var l = new List<StockPrice.ResponseClasses.MainPriceResponse>();
var req = new DatabaseTotalResults() { Request = "LPUNI1" };
ReliablePartsAuthUsa.AuthReliableParts();
ReliablePartsUsa.Parsing(req, l);
Console.ReadKey();*/




AppDomain.CurrentDomain.UnhandledException += ConsoleModify.CurrentDomain_UnhandledException;



AmreSupplyAuth.AuthAmreSupply();
ReliablePartsAuthCanada.AuthReliableParts();
ReliablePartsAuthUsa.AuthReliableParts();
MarconeAuthCanada.AuthMarcone();
MarconeAuthUsa.AuthMarcone();





var botClient = new TelegramBotClient(AppSettings.Current.Telegram.Token);

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();



Console.WriteLine($"Start listening for @{me.Username} thread started!");
Task t = Task.Run(() => InfoCollector.MainTracker(botClient, cts.Token));
Task t2 = Task.Run(() => AnalogsUpdater.MainChecker(cts.Token));
Task t3 = Task.Run(() => MainChecker.MainMassTestingChecking(cts.Token));
Task t4 = Task.Run(() => MainResultCreator.MainMassTestingResultCreator(botClient, cts.Token));
var stop_or_not = Console.ReadLine();


// Send cancellation request to stop bot
if (stop_or_not.Equals("STOP")) cts.Cancel();
//while (true) { }

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{

    if (update.Type.ToString().Equals("Message") && update.Message.Document != null)
    {

        var chatId = update.Message;
        //var query = update.CallbackQuery.Data;
        //Console.WriteLine($"Received a callbackQuery '{query}' in chat {chatId}.");
        await Task.Run(() => StockPrice.MassTestingModule.MainCreator.MakeNewRequest(botClient, update, cancellationToken)); //Starting the button request handler
    }


    if (update.Type.ToString().Equals("Message") && update.Message.Document == null)
    {
        var message = update.Message;
        var messageText = message.Text;
        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a message '{messageText}' in chat {chatId}.");
        await Task.Run(() => MessageHandler.TextMessageHandler(botClient, update, cancellationToken)); //Starting the text message handler
    }

    if (update.Type.ToString().Equals("CallbackQuery"))
    {

        var chatId = update.CallbackQuery.From.Id;
        var query = update.CallbackQuery.Data;
        Console.WriteLine($"Received a callbackQuery '{query}' in chat {chatId}.");
        await Task.Run(() => MessageHandler.CallBackQueryHandler(botClient, update, cancellationToken)); //Starting the button request handler
    }

    


}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    Process.Start(Assembly.GetExecutingAssembly().Location);
    Environment.Exit(Environment.ExitCode);
    Console.ReadKey();
    return Task.CompletedTask;
}

Console.WriteLine("Press enter to close...");
Console.ReadLine();

