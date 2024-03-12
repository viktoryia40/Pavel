using Newtonsoft.Json;

namespace StockPrice.Methods
{
    public sealed class ShortUrlJson
    {
        public sealed class Shorten
        {
            [JsonProperty("api_key")]
            public string ApiKey { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class ShortenResponse
        {
            [JsonProperty("execution_time")]
            public decimal ExecutedTime { get; set; }

            [JsonProperty("status")]
            public int Status { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("data")]
            public ShortenResponseData Data { get; set; }


        }

        public sealed class ShortenResponseData
        {

            [JsonProperty("url_id")]
            public string UrlID { get; set; }

            [JsonProperty("short_code")]
            public string ShortCode { get; set; }

            [JsonProperty("short_url")]
            public string ShurtUrl { get; set; }
        }
    }
}
