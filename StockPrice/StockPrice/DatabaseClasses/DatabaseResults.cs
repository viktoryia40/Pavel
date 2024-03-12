namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for `totalresults`
    /// </summary>
    public sealed class DatabaseResults
    {
        /// <summary>
        /// ID in DB
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// ChatID for request
        /// </summary>
        public long ChatID { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string DiagramWEB { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string PartlistPDF { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string TechSheetPDF { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string ServiceManualPDF { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string ServiceManualWEB { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string WiringSheetPDF { get; set; }

        /// <summary>
        /// Search request
        /// </summary>
        public string ServicePointerPDF { get; set; }
    }
}
