

namespace StockPrice.Settings
{
    public sealed class TelegramSettings
    {
        /// <summary>
        /// Token of TelegramBot
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// A Admin ID to send info about new sites requests
        /// </summary>
        public string AdminId { get; set; }

    }
}
