using Dapper;
using Leaf.xNet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using StockPrice.SiteMethods.Classes;

namespace StockPrice.Methods.Authorization
{
    public class AmreSupplyAuth
    {
        public static CookieStorage AmreSupplyAuthCookie { get; set; }
        public static bool AuthReady { get; set; } = false;
        public static DatabaseProxyData SelectedProxy { get; set; } = null;

        public static void AuthAmreSupply()
        {
            Console.WriteLine("First auth Amresupply..");
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var amresupplyReadyCookies = con.QueryFirstOrDefault<DatabaseJsonReadyAuthData>($"SELECT * FROM json_redy_auth_data WHERE `source`='amresupply';");
            con.Close();

            var totalStorage = new CookieStorage();

            var deserializedCookies = JsonConvert.DeserializeObject<List<ZennoJsonCookie>>(amresupplyReadyCookies.Data);

            var gotProxy = JsonConvert.DeserializeObject<DatabaseProxyData>(amresupplyReadyCookies.SelectedProxy);


            foreach (var resp in deserializedCookies)
            {
                totalStorage.Set(resp.Name, resp.Value, resp.Domain, resp.Path);
            }



            string redirect = null;
            try
            {
                redirect = CustomHttpClass.CheckRedirectGet(@"https://www.amresupply.com/account", coockies: totalStorage, selected_proxy: gotProxy);
            }
            catch
            {
                Console.WriteLine(@"Error during auth on amresupply, stage - 0");
                return;
            }

            if (redirect == null)
            {
                AmreSupplyAuthCookie = totalStorage;
                AuthReady = true;
                SelectedProxy = gotProxy;
            }

            Task.Run(() => ReAuthAmreSupply()); //Starting the re-authorization flow



        }

        public static void ReAuthAmreSupply()
        {
            while (true)
            {
                Thread.Sleep(10 * 60 * 1000);
                Console.WriteLine("Re-auth Amresupply..");
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);
                con.Open();
                var amresupplyReadyCookies = con.QueryFirstOrDefault<DatabaseJsonReadyAuthData>($"SELECT * FROM json_redy_auth_data WHERE `source`='amresupply';");
                con.Close();

                var totalStorage = new CookieStorage();

                var deserializedCookies = JsonConvert.DeserializeObject<List<ZennoJsonCookie>>(amresupplyReadyCookies.Data);

                var gotProxy = JsonConvert.DeserializeObject<DatabaseProxyData>(amresupplyReadyCookies.SelectedProxy);


                foreach (var resp in deserializedCookies)
                {
                    totalStorage.Set(resp.Name, resp.Value, resp.Domain, resp.Path);
                }



                string redirect = null;
                try
                {
                    redirect = CustomHttpClass.CheckRedirectGet(@"https://www.amresupply.com/account", coockies: totalStorage, selected_proxy: gotProxy);
                }
                catch
                {
                    Console.WriteLine(@"Error during auth on amresupply, stage - 0");
                    return;
                }

                if (redirect == null)
                {
                    AmreSupplyAuthCookie = totalStorage;
                    AuthReady = true;
                    SelectedProxy = gotProxy;
                    Console.WriteLine("Re-auth Amresupply completed!");
                }





            }
        }

    }
}
