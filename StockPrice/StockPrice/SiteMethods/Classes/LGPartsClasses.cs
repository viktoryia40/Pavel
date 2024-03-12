
namespace StockPrice.SiteMethods.Classes
{
    /// <summary>
    /// Classes for LGParts parsing
    /// </summary>
    public sealed class LGPartsClasses
    {
        /// <summary>
        /// Variants Data
        /// </summary>
        public sealed class Variants
        {
            /// <summary>
            /// Variant title
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Variant SKU
            /// </summary>
            public string SKU { get; set; }

            /// <summary>
            /// Variant Barcode
            /// </summary>
            public string Barcode { get; set; }

        }
    }
}
