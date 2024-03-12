using Newtonsoft.Json;

namespace StockPrice.SiteMethods.Classes
{
    public class AmazonCAClasses
    {
        public class Search
        {
            [JsonProperty("customer-action")]
            public string CustomerAction { get; set; } = "pagination";
        }


        public struct TemperatureElement
        {
            public string String;
            public TemperatureClass TemperatureClass;

            public static implicit operator TemperatureElement(string String) => new TemperatureElement { String = String };
            public static implicit operator TemperatureElement(TemperatureClass TemperatureClass) => new TemperatureElement { TemperatureClass = TemperatureClass };
        }

        public partial class TemperatureClass
        {
            public string Html { get; set; }
            public string Asin { get; set; }
            public long Index { get; set; }
        }
    }
}
