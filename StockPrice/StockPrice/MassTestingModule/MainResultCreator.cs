using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Methods;
using StockPrice.ResponseClasses;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace StockPrice.MassTestingModule
{
    public class MainResultCreator
    {
        public static async void MainMassTestingResultCreator (ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            Console.WriteLine("Mass Testing Result Creator started");
            while (!cancellationToken.IsCancellationRequested)
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);

                con.Open();
                var massRequestFirst = con.QueryFirstOrDefault<DatabaseMassTestingRequestsData>("SELECT * FROM `mass_testing_requests` WHERE `WorkDone`='1' AND `ReportSended`='0';"); //Get a data
                con.Close();

                if (massRequestFirst != null && massRequestFirst.ID != 0)
                {

                    con.Open();
                    var responses = con.Query<DatabaseTotalResults>($@"SELECT * FROM `totalresults` WHERE `ChatID`='{massRequestFirst.InitiatorID}' AND `massTestingID`='{massRequestFirst.ID}';");
                    con.Close();

                    if (responses.Count() > 0)
                    {
                        List<List<MainPriceResponse>> price_result_deserialized = new();



                        foreach(var response in responses)
                        {
                            if (response.fullResult != null)
                                price_result_deserialized.Add(JsonConvert.DeserializeObject<List<MainPriceResponse>>(response.fullResult));
                        }


                        var _j_ser_resp = JsonConvert.SerializeObject(price_result_deserialized, Formatting.None);
                        //var _j = JsonConvert.DeserializeObject(_j_ser_resp);
                        string total_csv = CSVMaker.AsposeJsonToCsv(_j_ser_resp);

                        using (var sr = new StreamReader(total_csv))
                        await botClient.SendDocumentAsync(
                            chatId: massRequestFirst.InitiatorID,
                            document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(sr.BaseStream, $@"Report for request #{massRequestFirst.ID} from {massRequestFirst.InitiatorID}.csv"));

                        con.Open();
                        con.Query($@"UPDATE `mass_testing_requests` SET `ReportSended`='1' WHERE `ID`='{massRequestFirst.ID}';"); //Get a data
                        con.Close();
                    }
                }

            }


        }
       
    }
}
