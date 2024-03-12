
namespace StockPrice.DatabaseClasses
{
    public sealed class DatabaseCFRequestData
    {
        /// <summary>
        /// ID of CF request ind DB
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Target URL from DB of CF request
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// Total response of this URL request
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Time when request was added
        /// </summary>
        public DateTime RequestAdd { get; set; }

        /// <summary>
        /// Time when request was ready
        /// </summary>
        public DateTime RequestReady { get; set; }

        /// <summary>
        /// Status of this request
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Counter of try get response
        /// </summary>
        public int Counter { get; set; }
    }
}
