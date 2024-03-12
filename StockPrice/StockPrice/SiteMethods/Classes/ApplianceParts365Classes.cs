using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPrice.SiteMethods.Classes
{
    public class ApplianceParts365Classes
    {
        public partial class ApplianceParts365Result
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("ShortDescription")]
            public string ShortDescription { get; set; }

            [JsonProperty("FullDescription")]
            public string FullDescription { get; set; }

            [JsonProperty("SeName")]
            public string SeName { get; set; }

            [JsonProperty("MarkAsNew")]
            public bool MarkAsNew { get; set; }

            [JsonProperty("ProductPrice")]
            public ProductPrice ProductPrice { get; set; }

            [JsonProperty("DefaultPictureModel")]
            public DefaultPictureModel DefaultPictureModel { get; set; }

            [JsonProperty("SpecificationAttributeModels")]
            public SpecificationAttributeModel[] SpecificationAttributeModels { get; set; }

            [JsonProperty("ReviewOverviewModel")]
            public ReviewOverviewModel ReviewOverviewModel { get; set; }

            [JsonProperty("Id")]
            public long Id { get; set; }

            [JsonProperty("CustomProperties")]
            public ApplianceParts365ResultCustomProperties CustomProperties { get; set; }
        }

        public partial class ApplianceParts365ResultCustomProperties
        {
            [JsonProperty("Url")]
            public string Url { get; set; }
        }

        public partial class DefaultPictureModel
        {
            [JsonProperty("ImageUrl")]
            public Uri ImageUrl { get; set; }

            [JsonProperty("FullSizeImageUrl")]
            public Uri FullSizeImageUrl { get; set; }

            [JsonProperty("Title")]
            public string Title { get; set; }

            [JsonProperty("AlternateText")]
            public string AlternateText { get; set; }

            [JsonProperty("CustomProperties")]
            public DefaultPictureModelCustomProperties CustomProperties { get; set; }
        }

        public partial class DefaultPictureModelCustomProperties
        {
        }

        public partial class ProductPrice
        {
            [JsonProperty("OldPrice")]
            public string OldPrice { get; set; }

            [JsonProperty("Price")]
            public string Price { get; set; }

            [JsonProperty("PriceValue")]
            public double PriceValue { get; set; }

            [JsonProperty("DisableBuyButton")]
            public bool DisableBuyButton { get; set; }

            [JsonProperty("DisableWishlistButton")]
            public bool DisableWishlistButton { get; set; }

            [JsonProperty("DisableAddToCompareListButton")]
            public bool DisableAddToCompareListButton { get; set; }

            [JsonProperty("AvailableForPreOrder")]
            public bool AvailableForPreOrder { get; set; }

            [JsonProperty("PreOrderAvailabilityStartDateTimeUtc")]
            public object PreOrderAvailabilityStartDateTimeUtc { get; set; }

            [JsonProperty("IsRental")]
            public bool IsRental { get; set; }

            [JsonProperty("ForceRedirectionAfterAddingToCart")]
            public bool ForceRedirectionAfterAddingToCart { get; set; }

            [JsonProperty("DisplayTaxShippingInfo")]
            public bool DisplayTaxShippingInfo { get; set; }

            [JsonProperty("CustomProperties")]
            public DefaultPictureModelCustomProperties CustomProperties { get; set; }
        }

        public partial class ReviewOverviewModel
        {
            [JsonProperty("ProductId")]
            public long ProductId { get; set; }

            [JsonProperty("RatingSum")]
            public long RatingSum { get; set; }

            [JsonProperty("TotalReviews")]
            public long TotalReviews { get; set; }

            [JsonProperty("AllowCustomerReviews")]
            public bool AllowCustomerReviews { get; set; }

            [JsonProperty("CustomProperties")]
            public DefaultPictureModelCustomProperties CustomProperties { get; set; }
        }

        public partial class SpecificationAttributeModel
        {
            [JsonProperty("SpecificationAttributeId")]
            public long SpecificationAttributeId { get; set; }

            [JsonProperty("SpecificationAttributeName")]
            public string SpecificationAttributeName { get; set; }

            [JsonProperty("ValueRaw")]
            public string ValueRaw { get; set; }

            [JsonProperty("CustomProperties")]
            public DefaultPictureModelCustomProperties CustomProperties { get; set; }
        }
    }
}
