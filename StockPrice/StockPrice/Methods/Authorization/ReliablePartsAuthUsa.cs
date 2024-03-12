using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;

namespace StockPrice.Methods.Authorization
{
    public sealed class ReliablePartsAuthUsa
    {
        public static string ReliablePartsBearerToken { get; set; }
        public static bool AuthReady { get; set; } = false;

        public static void AuthReliableParts()
        {
            Console.WriteLine("First auth ReliableParts [USA]..");
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var reliablepartsAuthInfo = con.QueryFirstOrDefault<DatabaseAuthData>($"SELECT * FROM auth_data WHERE `resource`='reliablepartsUSA';");
            con.Close();


            var authData = new ReliablePartsAuthData()
            {
                Username = reliablepartsAuthInfo.Login,
                Password = reliablepartsAuthInfo.Password
            };

            string authDataString = JsonConvert.SerializeObject(authData, Formatting.None);

            string response = null;
            try
            {
                response = CustomHttpClass.PostToString(url: @"https://prodapi.reliableparts.net/us/accountapp/v1/security/api/auth/login",
                    contentType: "application/json",
                    jsonData: authDataString);
            }
            catch
            {
                Console.WriteLine("Error during auth ReliableParts [USA] Stage - 0");
            }

            if (response != null)
            {
                var respInfo = JsonConvert.DeserializeObject<ReliablePartsAuthResponse>(response);

                if (respInfo.AccessToken != null)
                {
                    ReliablePartsBearerToken = respInfo.AccessToken;
                    AuthReady = true;
                    Console.WriteLine("Auth ReliableParts [USA] Completed!");
                }
            }

            Task.Run(() => ReAuthReliableParts()); //Starting a thread for re-authorization.
        }

        private static void ReAuthReliableParts()
        {
            while (true)
            {
                Thread.Sleep(30 * 1000 * 60);
                Console.WriteLine("Re-auth ReliableParts..");
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);
                con.Open();
                var reliablepartsAuthInfo = con.QueryFirstOrDefault<DatabaseAuthData>($"SELECT * FROM auth_data WHERE `resource`='reliablepartsUSA';");
                con.Close();


                var authData = new ReliablePartsAuthData()
                {
                    Username = reliablepartsAuthInfo.Login,
                    Password = reliablepartsAuthInfo.Password
                };

                string authDataString = JsonConvert.SerializeObject(authData, Formatting.None);

                string response = null;
                try
                {
                    response = CustomHttpClass.PostToString(url: @"https://prodapi.reliableparts.net/us/accountapp/v1/security/api/auth/login",
                        contentType: "application/json",
                        jsonData: authDataString);
                }
                catch
                {
                    Console.WriteLine("Error during auth ReliableParts [USA] Stage - 0");
                }

                if (response != null)
                {
                    var resp_info = JsonConvert.DeserializeObject<ReliablePartsAuthResponse>(response);

                    if (resp_info.AccessToken != null)
                    {
                        ReliablePartsBearerToken = resp_info.AccessToken;
                        AuthReady = true;
                        Console.WriteLine("Re-auth ReliableParts [USA] Completed!");
                    }
                }
            }
        }

    }


    /*internal sealed class ReliablePartsAuthData
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    internal sealed class ReliablePartsAuthResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string TokenType { get; set; }

        public bool IsFirstLogin { get; set; } = false;

        public bool IsApdepotUser { get; set; } = false;

        public bool IsSamsungUser { get; set; } = false;
    }*/
}
