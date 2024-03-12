
namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for searspartsdirect_data
    /// </summary>
    public sealed class DatabaseSearsPartsDirectData
    {
        /// <summary>
        /// TraceId from DB
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// ApiKey from DB
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// SHA256Hash from BD
        /// </summary>
        public string SHA256Hash { get; set; }
    }
}
