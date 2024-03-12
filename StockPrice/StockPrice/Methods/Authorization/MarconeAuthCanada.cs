using Dapper;
using Leaf.xNet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using System.Web;

namespace StockPrice.Methods.Authorization
{


    public class MarconeAuthCanada
    {
        public static CookieStorage MarconeAuthCookie { get; set; }
        public static bool AuthReady { get; set; } = false;

        public static DatabaseProxyData SelectedProxy { get; set; } = null;



        public static void AuthMarcone()
        {
            Console.WriteLine("First auth Marcone [Canada]..");
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var marconeAuthInfo = con.QueryFirstOrDefault<DatabaseAuthData>($"SELECT * FROM auth_data WHERE `resource`='marcone';");
            var gotProxy = con.QueryFirstOrDefault<DatabaseProxyData>($"SELECT * FROM proxy_table WHERE `isActive`='1' ORDER BY RAND() LIMIT 1;");
            con.Close();

            var firstReq = new CookieStorage();
            try
            {
                firstReq = CustomHttpClass.GetToCookieStorage(@"https://beta.marcone.com/UserLogin", selected_proxy: gotProxy);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error auth Marcone, stage - 1. Exception: {ex.Message}");
            }


            CookieStorage totalStorage = firstReq;
            totalStorage.Set("mRemember", "true", "beta.marcone.com");

            RequestParams rp1 = new();
            rp1["UserName"] = marconeAuthInfo.Login;
            rp1["Password"] = marconeAuthInfo.Password;


            string checkAuth = null;
            try
            {
                checkAuth = CustomHttpClass.PostToString("https://beta.marcone.com/UserLogin/DoLogin", data: rp1, coockies: totalStorage, selected_proxy: gotProxy);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error auth Marcone [Canada], stage - 2. Exception: {ex.Message}");
            }


            dynamic _jres = JsonConvert.DeserializeObject(checkAuth);

            if (_jres.Result.ToString().Equals("True"))
            {

                RequestParams rp2 = new();
                rp2["str1"] = marconeAuthInfo.Login;
                rp2["str2"] = marconeAuthInfo.Password;

                List<CustomHttpAdditionals.Headers> headers = new()
                {
                    new CustomHttpAdditionals.Headers
                    {
                        Name = "X-Requested-With",
                        Value = "XMLHttpRequest"
                    }
                };


                string logPasEncrypted = null;

                try
                {
                    logPasEncrypted = CustomHttpClass.PostToString("https://beta.marcone.com/UserLogin/EncryptString", data: rp2, coockies: totalStorage, acceptencoding: "none", selected_proxy: gotProxy);

                    var spl_res = logPasEncrypted.Split('|');
                    if (spl_res.Count() == 2)
                    {
                        totalStorage.Set("mUserName", HttpUtility.HtmlEncode(spl_res[0]), "beta.marcone.com");
                        totalStorage.Set("mPassword", HttpUtility.HtmlEncode(spl_res[1]), "beta.marcone.com");

                        MarconeAuthCookie = totalStorage;
                        AuthReady = true;
                        SelectedProxy = gotProxy;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error auth Marcone [Canada], stage - 3. Exception: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Incorrect password or login Marcone [Canada].");
            }

            //Start the re-authorization task
            Task.Run(() => ReAuthMarcone());


        }

        public static void ReAuthMarcone()
        {
            while (true)
            {
                Thread.Sleep(10 * 60 * 1000);
                Console.WriteLine("Re-auth Marcone [Canada]..");
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);
                con.Open();
                var marconeAuthInfo = con.QueryFirstOrDefault<DatabaseAuthData>($"SELECT * FROM auth_data WHERE `resource`='marcone';");
                var gotProxy = con.QueryFirstOrDefault<DatabaseProxyData>($"SELECT * FROM proxy_table WHERE `isActive`='1' ORDER BY RAND() LIMIT 1;");
                con.Close();

                var totalStorage = new CookieStorage();
                var firstReq = new CookieStorage();
                try
                {
                    firstReq = CustomHttpClass.GetToCookieStorage(@"https://beta.marcone.com/UserLogin", selected_proxy: gotProxy);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error re-auth Marcone [Canada], stage - 1. Exception: {ex.Message}");
                }


                totalStorage = firstReq;
                totalStorage.Set("mRemember", "true", "beta.marcone.com");

                RequestParams rp1 = new();
                rp1["UserName"] = marconeAuthInfo.Login;
                rp1["Password"] = marconeAuthInfo.Password;


                string checkAuth = null;
                try
                {
                    checkAuth = CustomHttpClass.PostToString("https://beta.marcone.com/UserLogin/DoLogin", data: rp1, coockies: totalStorage, selected_proxy: gotProxy);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error re-auth Marcone [Canada], stage - 2. Exception: {ex.Message}");
                }


                dynamic jres = JsonConvert.DeserializeObject(checkAuth);

                if (jres.Result.ToString().Equals("True"))
                {

                    RequestParams rp2 = new();
                    rp2["str1"] = marconeAuthInfo.Login;
                    rp2["str2"] = marconeAuthInfo.Password;

                    List<CustomHttpAdditionals.Headers> headers = new()
                    {
                        new CustomHttpAdditionals.Headers
                        {
                            Name = "X-Requested-With",
                            Value = "XMLHttpRequest"
                        }
                    };


                    string logPasEncrypted = null;

                    try
                    {
                        logPasEncrypted = CustomHttpClass.PostToString("https://beta.marcone.com/UserLogin/EncryptString", data: rp2, coockies: totalStorage, acceptencoding: "none", selected_proxy: gotProxy);

                        var splRes = logPasEncrypted.Split('|');
                        if (splRes.Count() == 2)
                        {
                            totalStorage.Set("mUserName", HttpUtility.HtmlEncode(splRes[0]), "beta.marcone.com");
                            totalStorage.Set("mPassword", HttpUtility.HtmlEncode(splRes[1]), "beta.marcone.com");

                            MarconeAuthCookie = totalStorage;
                            AuthReady = true;
                            SelectedProxy = gotProxy;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error re-auth Marcone [Canada], stage - 3. Exception: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect password or login Marcone [Canada].");
                }
            }


        }
    }
}
