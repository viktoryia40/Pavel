
namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for `sites_priority` table in DB
    /// </summary>
    public sealed class DatabaseSitesPriority
    {
        /// <summary>
        /// ID of Source
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// URL of Source
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Name of Source
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Priority for Diagram WEB data
        /// </summary>
        public int DiagramWEBPriority { get; set; }

        /// <summary>
        /// Priority for Parlist PDF data
        /// </summary>
        public int PartlistPDFPriority { get; set; }

        /// <summary>
        /// Priority for Tech Sheet PDF data
        /// </summary>
        public int TechSheetPDFPriority { get; set; }

        /// <summary>
        /// Priority for Service Manual PDF data
        /// </summary>
        public int ServiceManualPDFProirity { get; set; }

        /// <summary>
        /// Priority for Service Manual WEB data
        /// </summary>
        public int ServiceManualWEBPriority { get; set; }

        /// <summary>
        /// Priority for Wiring Sheet PDF data
        /// </summary>
        public int WiringSheetPDFPriority { get; set; }

        /// <summary>
        /// Priority for Service Pointer PDF data
        /// </summary>
        public int ServicePointerPDFPriority { get; set; }

        /// <summary>
        /// Priority for Photo from source
        /// </summary>
        public int PhotoPriority { get; set; }

    }
}
