namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for `unregistered_responses` table in DB
    /// </summary>
    public sealed class DatabaseUnregisteredResponses
    {
        /// <summary>
        /// ID of row in  DB
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Request ID from `totalresults` table
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// Request text from `totalresults` table
        /// </summary>
        public string RequestText { get; set; }

        /// <summary>
        /// The source where the error occurred 
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Base64-encoded data of error
        /// </summary>
        public string Base64wrongData { get; set; }

        /// <summary>
        /// Base64-encoded data of error
        /// </summary>
        public string Base64errorData { get; set; }

        /// <summary>
        /// Comment with additional information
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The URL to which the request was made and later an error was detected
        /// </summary>
        public string Url { get; set; }
    }
}
