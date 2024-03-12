

namespace StockPrice.SiteMethods.Classes
{
    /// <summary>
    /// Classes for LGParts parsing
    /// </summary>
    public sealed class EnCompassClasses
    {
        /// <summary>
        /// Class for table from search results
        /// </summary>
        public sealed class SearchTable
        {
            /// <summary>
            /// Model Number 
            /// </summary>
            public string ModelName { get; set; }

            /// <summary>
            /// HREF for this model
            /// </summary>
            public string Href { get; set; }
        }

        public sealed class ServiceManuals
        {
            /// <summary>
            /// Title 
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// HREF for SDerviceManual
            /// </summary>
            public string Href { get; set; }
        }

        public sealed class PartListPDFs
        {
            /// <summary>
            /// Title 
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// HREF for Partlist
            /// </summary>
            public string Href { get; set; }
        }
    }
}
