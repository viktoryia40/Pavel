using System.Numerics;

namespace StockPrice.SiteMethods.Classes
{
    public sealed class ReliablePartsClasses
    {

        public sealed class Results
        {
            public int TotalFound { get; set; } = 0;
            public int ModelsFound { get; set; } = 0;
            public int ProductFound { get; set; } = 0;
            public bool HasExactResult { get; set; } = false;
            public List<Models> Models { get; set; }
            public List<Products> Products { get; set; }
        }

        public class Models
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ProductNumber { get; set; }
            public string Description { get; set; }
            public string Manufacturer { get; set; }
            public int Ranking { get; set; } = 0;
            public string SubcategoryName { get; set; }
            public string SubcategoryId { get; set; }
            public string CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string Type { get; set; }
            public BigInteger Version { get; set; } = 0;
            public int Score { get; set; } = 0;
        }

        public class Products
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ProductNumber { get; set; }
            public string Description { get; set; }
            public string Manufacturer { get; set; }
            public int Ranking { get; set; } = 0;
            public string SubcategoryName { get; set; }
            public string SubcategoryId { get; set; }
            public string CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string Type { get; set; }
            public BigInteger Version { get; set; } = 0;
            public int Score { get; set; } = 0;
        }

        public sealed class ProductDetail
        {
            public string Name { get; set; }
            public object Manufacturer { get; set; }
            public string Description { get; set; }
            public string ManufacturerCode { get; set; }
            public string PartnerPrice { get; set; }
            public string RetailPrice { get; set; }
            public object Code { get; set; }
            public string StockCode { get; set; }
            public string Extras { get; set; }
            public string Notes { get; set; }
            public string ProductNumber { get; set; }
            public string AlternateProductManufacturer { get; set; }
            public string AlternateProductNumber { get; set; }
            public string OemProductManufacturer { get; set; }
            public string OemProductNumber { get; set; }
            public long Qty { get; set; }
            public string Core { get; set; }
            public long Totalqty { get; set; }
            public long ModelsNumberTotalFound { get; set; }
            public List<Warehouse> Warehouses { get; set; }
            public List<Location> Locations { get; set; }
            public List<Uri> Images { get; set; }
            public string Replacement { get; set; }
            public string State { get; set; }
            public string StateDescription { get; set; }
            public List<object> ExtraProducts { get; set; }
            public long ExtraQty { get; set; }
            public long ExtraTotalAmount { get; set; }
            public object DiscountInfo { get; set; }
            public bool IsFavorite { get; set; }
            public object IdFavoriteItem { get; set; }
            public List<ListItem> ListItem { get; set; }
            public object ReplacedPart { get; set; }
            public object ReplacedMfc { get; set; }
            public object ReplacementPart { get; set; }
            public object ReplacementMfc { get; set; }
            public bool Available { get; set; }
            public bool InStock { get; set; }
            public bool SpecialOrder { get; set; }
        }

        public partial class ListItem
        {
            public long IdItem { get; set; }
            public long IdList { get; set; }
        }

        public partial class Location
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string AddressLine4 { get; set; }
            public string StateCode { get; set; }
            public string PostalCode { get; set; }
            public bool PickUpOnly { get; set; }
        }

        public partial class Warehouse
        {
            public string WarehouseCode { get; set; }
            public string Description { get; set; }
            public string Quantity { get; set; }
            public string SuppliedFrom { get; set; }
            public bool Default { get; set; }
        }


    }
}
