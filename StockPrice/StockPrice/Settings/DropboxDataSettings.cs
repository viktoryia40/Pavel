

namespace StockPrice.Settings
{
    public sealed class DropboxDataSettings
    {
        /// <summary>
        /// Local path with DropBoxData tables
        /// </summary>
        public string DropBoxTablesPath { get; set; }

        /// <summary>
        /// DropBoxData API key
        /// </summary>
        public string RefreshKey { get; set; }

        /// <summary>
        /// DropBoxData App key
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// DropBoxData App secret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// Path to the folder with tables for API
        /// </summary>
        public string StockTablesPath { get; set; }

        /// <summary>
        /// Absolute path where located ready tables
        /// </summary>
        public string DropBoxAbsoluteTotalPath { get; set; }
    }
}
