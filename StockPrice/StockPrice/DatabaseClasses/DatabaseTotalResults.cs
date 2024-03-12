
namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for totalresults table
    /// </summary>
    public sealed class DatabaseTotalResults
    {
        /// <summary>
        /// ID in DB
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// MessageID in DB
        /// </summary>
        public long MessageID { get; set; }

        /// <summary>
        /// ChatID for request
        /// </summary>
        public long ChatID { get; set; }

        /// <summary>
        /// BotMessageID for request
        /// </summary>
        public long BotMessageID { get; set; }

        /// <summary>
        /// BotPhotoMessageID for request
        /// </summary>
        public long BotPhotoMessageID { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string Request { get; set; }



        /// <summary>
        /// Mass testing or not
        /// </summary>
        public bool IsMassTestingRequest { get; set; }

        /// <summary>
        /// ID in DB
        /// </summary>
        public long MassTestingID { get; set; }

        /// <summary>
        /// Request work started or not
        /// </summary>
        public bool WorkStarted { get; set; }

        /// <summary>
        /// Reduced result for user
        /// </summary>
        public string SendedResult { get; set; }

        /// <summary>
        /// Full result of search
        /// </summary>
        public string fullResult { get; set; }

        /// <summary>
        /// DateTime when request was registered
        /// </summary>
        public DateTime RequestStart { get; set; }

        /// <summary>
        /// DateTime when response for request was sent to user
        /// </summary>
        public DateTime ResponseSent { get; set; }
    }
}
