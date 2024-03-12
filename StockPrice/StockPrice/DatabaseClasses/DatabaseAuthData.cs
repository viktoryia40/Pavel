

namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for auth_data database result
    /// </summary>
    public sealed class DatabaseAuthData
    {
        /// <summary>
        /// Resource (ex.: marcone)
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Login of resource
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Password of resource
        /// </summary>
        public string Password { get; set; }
    }
}
