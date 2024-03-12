using Newtonsoft.Json;
using StockPrice.Settings;

namespace StockPrice.Methods
{
    public sealed class ShortUrl
    {
        public static string MakeShortURL(string url)
        {
            string request_data = JsonConvert.SerializeObject(new ShortUrlJson.Shorten() { ApiKey = AppSettings.Current.HyperMagic.ApiKey, Url = url });

            string result = CustomHttpClass.PostToString(@"https://api.hm.ru/key/url/shorten", jsonData: request_data, contentType: "application/json");

            var result_parsed = JsonConvert.DeserializeObject<ShortUrlJson.ShortenResponse>(result);

            if (result_parsed.Status == -1) return url;
            else
            {
                return result_parsed.Data.ShurtUrl;
            }


        }

        public static string MakeShortURLClckRU (string url)
        {
            string result = null;
            try
            {
                result = CustomHttpClass.GetToString($@"https://clck.ru/--?url={url}");
                return result;
            }
            catch 
            {

                return url;
            }
        }

    }
}
