
namespace StockPrice.Settings
{
    public sealed class DatabaseSettings
    {
        /// <summary>
        /// Database Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Database Login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Database Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Database Database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Database Port
        /// </summary>
        public int Port { get; set; }
    }
}
