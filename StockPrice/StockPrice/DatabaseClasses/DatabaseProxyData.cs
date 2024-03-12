namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for proxy from Database
    /// </summary>
    public sealed class DatabaseProxyData
    {
        /// <summary>
        /// Id of taken proxy
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Type of taken proxy
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// IP of taken proxy
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Port of taken proxy
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Login of taken proxy
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Password of taken proxy
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Status of this proxy
        /// </summary>
        public bool IsActive { get; set; }
    }
}
