
namespace StockPrice.SiteMethods.Classes
{
    public sealed class ZennoJsonCookie
    {
        public string Domain { get; set; }

        public decimal ExpirationDate { get; set; } = 0;
        public bool HostOnly { get; set; } = false;

        public bool HttpOnly { get; set; } = false;

        public string Name { get; set; }

        public string Path { get; set; }

        public string SameSite { get; set; }

        public bool Secure { get; set; }

        public bool Session { get; set; }

        public string StoreId { get; set; }
        public string Value { get; set; }

        public int Id { get; set; }
    }

    public sealed class Cookie
    {

    }
}
