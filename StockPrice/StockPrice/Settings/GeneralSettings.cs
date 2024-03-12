
namespace StockPrice.Settings
{
    public sealed class GeneralSettings
    {
        /// <summary>
        /// Maximum delivery days
        /// </summary>
        public int MaxDeliveryDays { get; set; }

        /// <summary>
        /// The path where the DropBox folder for storing users' stock tables is located.
        /// </summary>
        public string DropBoxTablesPath { get; set; }
    }
}
