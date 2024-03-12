
namespace StockPrice.ResponseClasses
{
    /// <summary>
    /// Class for main price response
    /// </summary>
    public sealed class MainPriceResponse
    {
        /// <summary>
        /// Source of resposne
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Search url with search results
        /// </summary>
        public string SearchUrl { get; set; }

        /// <summary>
        /// Additional data for source. For example: "📦$9.95"
        /// </summary>
        public string? Additional { get; set; } = null;

        /// <summary>
        /// Additional data for source. For example: "(main page)"
        /// </summary>
        public string? EndAdditional { get; set; } = null;


        /// <summary>
        /// Alternative link (now using only for reliableparts)
        /// </summary>
        public string AlternativeLink { get; set; } = null;

        /// <summary>
        /// Alternative search link (now using only for reliableparts)
        /// </summary>
        public string? AlternativeSearchLink { get; set; } = null;

        /// <summary>
        /// List with delivery locations. May be null.
        /// </summary>
        public List<string> Locations { get; set; } = null;

        /// <summary>
        /// List of Prices
        /// </summary>
        public List<Prices> PricesList { get; set; } = null;

        /// <summary>
        /// Lowest price at this result
        /// </summary>
        public decimal LowestPrice { get; set; } = 0;

        /// <summary>
        /// Boolean value. 'True' if there is more than one response from the site in the output. Default is 'fasle'
        /// </summary>
        public bool MultiChoice { get; set; } = false;

        /// <summary>
        /// Boolean value. 'True' in case there are no results at the source or the product is unavailable. The default is 'false'.
        /// </summary>
        public bool NothingFoundOrOutOfStock { get; set; } = false;

        /// <summary>
        /// Boolean value. 'True' in case no response is received from the site or there is an error. The default is 'false'.
        /// </summary>
        public bool NoAnswerOrError { get; set; } = false;

        /// <summary>
        /// Error message if 'NoAnswerOrError' is 'true'.
        /// </summary>
        public string ErrorMessage { get; set; }

    }


    /// <summary>
    /// Class for Prices of response
    /// </summary>
    public sealed class Prices
    {
        /// <summary>
        /// The title of the result found.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Delivery date. The default is null.
        /// </summary>
        public string DeliveryDays { get; set; }

        /// <summary>
        /// Availability. In stock, Preorder, etc.
        /// </summary>
        public string Availability { get; set; }

        /// <summary>
        /// Double price for sites with authorization.
        /// </summary>
        public string DoublePrice { get; set; }

        /// <summary>
        /// The price of the result found.
        /// </summary>
        public decimal Price { get; set; } = 0;

        /// <summary>
        /// A link to the result found.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Delivery price. Default is '0'.
        /// </summary>
        public decimal DeliveryPrice { get; set; } = 0;
    }
}
