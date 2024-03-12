using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StockPrice.SiteMethods.CF_works
{
    public class CfRequestGetting
    {
        public static async Task<string> GetCfRequestResponse(int id)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);


            await con.OpenAsync();
            var gotData = await con.QueryFirstOrDefaultAsync<DatabaseCFRequestData>($"SELECT * FROM `cf_request_data` WHERE `ID`='{id}'");
            await con.CloseAsync();

            while (gotData.Status == 0 && (DateTime.Now - gotData.RequestAdd).TotalSeconds <= 120)
            {
                Thread.Sleep(350);
                gotData = await con.QueryFirstOrDefaultAsync<DatabaseCFRequestData>($"SELECT * FROM `cf_request_data` WHERE `ID`='{id}'");
            }

            switch (gotData.Status)
            {
                case 1:
                    return Encoding.Default.GetString(Convert.FromBase64String(gotData.Response));
                default: throw new Exception("Error during making a response");

            }

            
        }
    }
}
