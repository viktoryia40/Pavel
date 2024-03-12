using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using OfficeOpenXml;
using StockPrice.DatabaseClasses;
using StockPrice.Methods.TableWorks.Classes;
using StockPrice.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dropbox.Api.FileProperties.PropertyType;

namespace StockPrice.MassTestingModule
{
    public static class MainChecker
    {

        public static void MainMassTestingChecking(CancellationToken cancellationToken)
        {
            Console.WriteLine("Main Mass Testing Checker started");
            while (!cancellationToken.IsCancellationRequested)
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);

                con.Open();
                var massRequestFirst = con.QueryFirstOrDefault<DatabaseMassTestingRequestsData>("SELECT * FROM `mass_testing_requests` WHERE `WorkDone`='0';"); //Get a data
                con.Close();

                List<string> SKUs = new();


                if (massRequestFirst != null)
                {
                    SKUs = JsonConvert.DeserializeObject<List<string>>(massRequestFirst.SkuList);

                    while (SKUs.Count > 0)
                    {
                        string req = SKUs[0];
                        con.Open();
                        var qur = con.QueryFirstOrDefault<long>($@"INSERT INTO `totalresults` (`request`, `isMassTestingRequest`, `massTestingID`, `chatID`) VALUES ('{req}', '1', '{massRequestFirst.ID}', '{massRequestFirst.InitiatorID}');
SELECT LAST_INSERT_ID();");
                        con.Close();

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            con.Open();
                            var result = con.QueryFirstOrDefault<DatabaseTotalResults>($@"SELECT * FROM `totalresults` WHERE `ID`='{qur}';");
                            con.Close();

                            if (result.SendedResult != null && result.fullResult != null) break;
                            else Thread.Sleep(500);
                        }

                        SKUs.RemoveAt(0);

                        string j_string = JsonConvert.SerializeObject(SKUs);

                        con.Open();
                        con.Query($@"UPDATE `mass_testing_requests` SET `SkuList`='{j_string}' WHERE `ID`='{massRequestFirst.ID}';");
                        con.Close();
                    }

                    con.Open();
                    con.Query($@"UPDATE `mass_testing_requests` SET `WorkDone`='1' WHERE `ID`='{massRequestFirst.ID}';");
                    con.Close();

                }
                


                Thread.Sleep(500);
            }
        }
    }
}
