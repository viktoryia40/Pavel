

namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for information from `json_redy_auth_data` table
    /// </summary>
    public sealed class DatabaseJsonReadyAuthData
    {
        /// <summary>
        /// Source of data (ex: amresupply)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Type of information (ex: cookie)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Data (ex: cookies in json formatting)
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Data (ex: cookies in json formatting)
        /// </summary>
        public string SelectedProxy { get; set; }
    }
}
