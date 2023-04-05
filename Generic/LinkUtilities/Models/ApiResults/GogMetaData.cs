using Newtonsoft.Json;

// Contains all the classes needed to deserialize the JSON fetched from the gog api.
namespace LinkUtilities.Models.Gog
{
    /*    public class ContentSystemCompatibility
        {
            [JsonProperty("windows")]
            public bool Windows;

            [JsonProperty("osx")]
            public bool Osx;

            [JsonProperty("linux")]
            public bool Linux;
        }

        public class Dlcs
        {
            [JsonProperty("products")]
            public List<Product2> Products;

            [JsonProperty("all_products_url")]
            public string AllProductsUrl;

            [JsonProperty("expanded_all_products_url")]
            public string ExpandedAllProductsUrl;
        }

        public class Images
        {
            [JsonProperty("background")]
            public string Background;

            [JsonProperty("logo")]
            public string Logo;

            [JsonProperty("logo2x")]
            public string Logo2X;

            [JsonProperty("icon")]
            public string Icon;

            [JsonProperty("sidebarIcon")]
            public string SidebarIcon;

            [JsonProperty("sidebarIcon2x")]
            public string SidebarIcon2X;

            [JsonProperty("menuNotificationAv")]
            public string MenuNotificationAv;

            [JsonProperty("menuNotificationAv2")]
            public string MenuNotificationAv2;
        }

        public class InDevelopment
        {
            [JsonProperty("active")]
            public bool Active;

            [JsonProperty("until")]
            public DateTime? Until;
        }

        public class Languages
        {
            [JsonProperty("en")]
            public string En;
        }

        public class Links
        {
            [JsonProperty("purchase_link")]
            public string PurchaseLink;

            [JsonProperty("product_card")]
            public string ProductCard;

            [JsonProperty("support")]
            public string Support;

            [JsonProperty("forum")]
            public string Forum;
        }

        public class Product2
        {
            [JsonProperty("id")]
            public int Id;

            [JsonProperty("link")]
            public string Link;

            [JsonProperty("expanded_link")]
            public string ExpandedLink;
        }

        public class GogMetaData
        {
            [JsonProperty("id")]
            public int Id;

            [JsonProperty("title")]
            public string Title;

            [JsonProperty("purchase_link")]
            public string PurchaseLink;

            [JsonProperty("slug")]
            public string Slug;

            [JsonProperty("content_system_compatibility")]
            public ContentSystemCompatibility ContentSystemCompatibility;

            [JsonProperty("languages")]
            public Languages Languages;

            [JsonProperty("links")]
            public Links Links;

            [JsonProperty("in_development")]
            public InDevelopment InDevelopment;

            [JsonProperty("is_secret")]
            public bool IsSecret;

            [JsonProperty("is_installable")]
            public bool IsInstallable;

            [JsonProperty("game_type")]
            public string GameType;

            [JsonProperty("is_pre_order")]
            public bool IsPreOrder;

            [JsonProperty("release_date")]
            public DateTime? ReleaseDate;

            [JsonProperty("images")]
            public Images Images;

            [JsonProperty("dlcs")]
            public Dlcs Dlcs;
        }*/

    public class GogMetaData
    {
        [JsonProperty("slug")]
        public string Slug;
    }
}