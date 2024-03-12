using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;


namespace StockPrice.SiteMethods.CF_works
{
    internal class CfRequestMaker
    {
        /// <summary>
        /// A task to make a request for CF sites
        /// </summary>
        /// <param name="targetUrl">A url from which u want to get data</param>
        /// <returns>ID of inserted request</returns>
        /// <exception cref="Exception">Return only if LAST_INSERT_ID() is '0'</exception>
        public static async Task<int> MakeCfRequest(string targetUrl)
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            await using var con = new MySqlConnection(cs);

            try
            {
                con.Open();
                await con.QueryFirstOrDefaultAsync<DatabaseUserData>($"INSERT INTO `cf_request_data` (`targetUrl`) VALUES ('{MySqlHelper.EscapeString(targetUrl)}');");
                var gotId = await con.QueryFirstOrDefaultAsync<int>("SELECT LAST_INSERT_ID();");
                await con.CloseAsync();
                if (gotId != 0)
                    return gotId;
            }
            catch 
            {
                throw new Exception("Error during adding request");
                
            }

            throw new Exception("Error during adding request");



        }
    }
}
