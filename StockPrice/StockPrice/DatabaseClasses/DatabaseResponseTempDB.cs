
namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// Class for response_temp_db
    /// </summary>
    public sealed class DatabaseResponseTempDB
    {
        public long ID { get; set; }

        public long RequestID { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }
    }
}
