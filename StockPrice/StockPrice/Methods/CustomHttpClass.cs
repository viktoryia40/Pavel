using Dapper;
using Leaf.xNet;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;

namespace StockPrice.Methods
{
    class CustomHttpClass
    {
        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static string? GetToString(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true, CookieStorage coockies = null, bool use_chrome_random_ua = false, DatabaseProxyData selected_proxy = null, bool ignoreErrors = false)
        {

            headers ??= new();
            using var req = new HttpRequest();

            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            if (use_chrome_random_ua) req.UserAgent = GenerateUserAgent();
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            if (ignoreErrors) req.IgnoreProtocolErrors = true;
            if (coockies != null) req.Cookies = coockies;



            var resp = req.Get(url);
            return resp.ToString();




        }


        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static CookieStorage GetToCookieStorage(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true, CookieStorage coockies = null, DatabaseProxyData selected_proxy = null)
        {

            headers ??= new();
            using var req = new HttpRequest();

            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            if (coockies != null) req.Cookies = coockies;



            var resp = req.Get(url);
            return resp.Cookies;




        }


        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static HttpResponse GetToResponse(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true, CookieStorage coockies = null, DatabaseProxyData selected_proxy = null)
        {

            headers ??= new();
            using var req = new HttpRequest();

            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            req.IgnoreProtocolErrors = true;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            if (coockies != null) req.Cookies = coockies;



            var resp = req.Get(url);
            
            return resp;




        }

        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static Stream GetToStream(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true, CookieStorage coockies = null, DatabaseProxyData selected_proxy = null)
        {

            headers ??= new();
            using var req = new HttpRequest();
            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (coockies != null) req.Cookies = coockies;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            if (coockies != null) req.Cookies = coockies;


            var myUniqueFileName = string.Format(@"\TempFiles\{0}.jpg", Guid.NewGuid());
            var resp = req.Get(url);

            Stream filestream = resp.ToMemoryStream();
            return filestream;

            /*var resp = req.Get(url);
            return resp.ToMemoryStream();*/




        }


        /// <summary>
        /// Custom method for Post http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="data">Data for POST request</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static string PostToString(string url, RequestParams data = null, List<CustomHttpAdditionals.Headers> headers = null, string contentType = null, string jsonData = null, CookieStorage coockies = null, string acceptencoding = null, bool use_google_ua = true, DatabaseProxyData selected_proxy = null)
        {

            headers ??= new();
            using var req = new HttpRequest();
            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            req.ConnectTimeout = 8000;
            string resp = null;
            if (coockies != null) req.Cookies = coockies;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            if (string.IsNullOrEmpty(contentType) && string.IsNullOrEmpty(jsonData)) resp = req.Post(url, data).ToString();
            else resp = req.Post(url, jsonData, contentType).ToString();
            return resp;




        }


        /// <summary>
        /// Custom method for Post http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="data">Data for POST request</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static HttpResponse PostToResponse(string url, RequestParams data = null, List<CustomHttpAdditionals.Headers> headers = null, string contentType = null, string jsonData = null, CookieStorage coockies = null, string acceptencoding = null, bool use_google_ua = true, string referer = null, string custom_ua = null, DatabaseProxyData selected_proxy = null)
        {

            headers ??= new();
            using var req = new HttpRequest();
            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            if (referer != null) req.Referer = referer;
            if (custom_ua != null) req.UserAgent = custom_ua;
            req.ConnectTimeout = 8000;



            if (coockies != null) req.Cookies = coockies;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            if (string.IsNullOrEmpty(contentType) && string.IsNullOrEmpty(jsonData)) return req.Post(url, data);
            else return req.Post(url, jsonData, contentType);





        }



        /// <summary>
        /// Custom method for GET http request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="headers">List of Headers</param>
        /// <returns></returns>
        public static bool GetIsExist(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true, CookieStorage coockies = null, DatabaseProxyData selected_proxy = null)
        {

            headers ??= new();
            using var req = new HttpRequest();
            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }
            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            if (coockies != null) req.Cookies = coockies;
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;



            try
            {
                var resp = req.Get(url);
                if (resp.IsOK && resp.ContentLength > 100) return true;
                else return false;
            }
            catch
            { return false; }




        }


        public static string? CheckRedirectGet(string url, List<CustomHttpAdditionals.Headers> headers = null, string referrer = null, string acceptencoding = null, bool use_google_ua = true, bool use_chrome_random_ua = false, CookieStorage coockies = null, DatabaseProxyData selected_proxy = null)
        {
            headers ??= new();
            using var req = new HttpRequest();
            var taken_proxy = new DatabaseProxyData();
            if (selected_proxy == null) taken_proxy = GetRandomProxy();
            else taken_proxy = selected_proxy;
            switch (taken_proxy.Type)
            {
                case "HTTP":
                    var HttpProxy = HttpProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) HttpProxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) HttpProxy.Password = taken_proxy.Password;
                    req.Proxy = HttpProxy;
                    break;

                case "SOCKS5":
                    var Socks5Proxy = Socks5ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks5Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks5Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks5Proxy;
                    break;

                case "SOCKS4":
                    var Socks4Proxy = Socks4ProxyClient.Parse($"{taken_proxy.IP}:{taken_proxy.Port}");
                    if (!string.IsNullOrEmpty(taken_proxy.Login)) Socks4Proxy.Username = taken_proxy.Login;
                    if (!string.IsNullOrEmpty(taken_proxy.Password)) Socks4Proxy.Password = taken_proxy.Password;
                    req.Proxy = Socks4Proxy;
                    break;
                default: break;
            }

            foreach (var header in headers)
            {
                req.AddHeader(header.Name, header.Value);
            }
            if (coockies != null) req.Cookies = coockies;
            if (use_google_ua) req.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
            if (use_chrome_random_ua) req.UserAgent = GenerateUserAgent();
            req.ConnectTimeout = 8000;
            req.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(referrer)) req.Referer = referrer;
            if (!string.IsNullOrEmpty(acceptencoding)) req.AcceptEncoding = acceptencoding;
            var resp = req.Get(url);
            if (string.IsNullOrEmpty(resp.Location)) return null;
            else return resp.Location;
        }


        public static DatabaseProxyData GetRandomProxy()
        {
            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            var proxy = con.QueryFirstOrDefault<DatabaseProxyData>($"SELECT * FROM proxy_table WHERE `isActive`='1' ORDER BY RAND() LIMIT 1;");
            con.Close();
            return proxy;

        }

        public static string GenerateUserAgent()
        {
            string[] os = {
                "Macintosh; Intel Mac OS X 10_15_7",
                "Macintosh; Intel Mac OS X 10_15_5",
                "Macintosh; Intel Mac OS X 10_11_6",
                "Macintosh; Intel Mac OS X 10_6_6",
                "Macintosh; Intel Mac OS X 10_9_5",
                "Macintosh; Intel Mac OS X 10_10_5",
                "Macintosh; Intel Mac OS X 10_7_5",
                "Macintosh; Intel Mac OS X 10_11_3",
                "Macintosh; Intel Mac OS X 10_10_3",
                "Macintosh; Intel Mac OS X 10_6_8",
                "Macintosh; Intel Mac OS X 10_10_2",
                "Macintosh; Intel Mac OS X 10_10_3",
                "Macintosh; Intel Mac OS X 10_11_5",
                "Windows NT 10.0; Win64; x64",
                "Windows NT 10.0; WOW64",
                "Windows NT 10.0"
            };

            var rnd = new Random();

            return @$"Mozilla/5.0 ({os[int.Parse(Math.Round(rnd.NextDouble() * (os.Length-1)).ToString())]}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{
                Math.Round(rnd.NextDouble() * 4) + 100}.0.{Math.Round(rnd.NextDouble() * 190) + 4100}.{Math.Round(rnd.NextDouble() * 50) + 140} Safari/537.36";
            //return @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37";
        }
    }
}
