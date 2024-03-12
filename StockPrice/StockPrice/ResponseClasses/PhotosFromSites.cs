
namespace StockPrice.ResponseClasses
{
    public sealed class PhotosFromSites
    {
        public string Source { get; set; }

        public int Priority { get; set; } = 0;

        public string PhotoURL { get; set; }
    }
}
