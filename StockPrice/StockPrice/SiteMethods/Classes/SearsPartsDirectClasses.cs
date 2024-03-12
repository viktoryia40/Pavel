using Newtonsoft.Json;
using static OfficeOpenXml.ExcelErrorValue;

namespace StockPrice.SiteMethods.Classes
{
    public sealed class SearsPartsDirectClasses
    {
        public sealed class VariablesConstructor

        {

            public partial class Variables
            {
                [JsonProperty("page")]
                public Page Page { get; set; }

                [JsonProperty("orders")]
                public List<Order> Orders { get; set; }

                [JsonProperty("filters")]
                public List<Filter> Filters { get; set; }

                [JsonProperty("substitutedByListFilter")]
                public List<Filter> SubstitutedByListFilter { get; set; }

                [JsonProperty("taxonomySearchFilter")]
                public List<TaxonomySearchFilter> TaxonomySearchFilter { get; set; }

                [JsonProperty("q")]
                public string Q { get; set; }
            }

            public partial class Filter
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("values")]
                public dynamic Values { get; set; }
            }

            public partial class Order
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("order")]
                public string OrderOrder { get; set; }
            }

            public partial class Page
            {
                [JsonProperty("from")]
                public long From { get; set; } = 0;

                [JsonProperty("size")]
                public long Size { get; set; } = 20;
            }

            public partial class TaxonomySearchFilter
            {
                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("values")]
                public string Values { get; set; }
            }

            
        }

        public sealed class ExtesionsConstructor
        {
            public sealed class Extensions
            {
                [JsonProperty("persistedQuery")]
                public PersistedQuery PersistedQuery { get; set; }
            }

            public sealed class PersistedQuery
            {
                [JsonProperty("version")]
                public int Version { get; set; }

                [JsonProperty("sha256Hash")]

                public string Sha256Hash { get; set; }
            }
        }
    }
}
