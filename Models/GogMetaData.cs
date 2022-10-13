using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog api.
namespace LinkManager.Models.Gog
{

    public class BackgroundImage
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class BoxArtImage
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Checkout
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Developer
    {
        [JsonProperty("name")]
        public string Name;
    }

    public class Embedded
    {
        [JsonProperty("product")]
        public Product Product;

        [JsonProperty("productType")]
        public string ProductType;

        [JsonProperty("localizations")]
        public List<Localization> Localizations;

        [JsonProperty("videos")]
        public List<Video> Videos;

        [JsonProperty("bonuses")]
        public List<object> Bonuses;

        [JsonProperty("screenshots")]
        public List<Screenshot> Screenshots;

        [JsonProperty("publisher")]
        public Publisher Publisher;

        [JsonProperty("developers")]
        public List<Developer> Developers;

        [JsonProperty("supportedOperatingSystems")]
        public List<SupportedOperatingSystem> SupportedOperatingSystems;

        [JsonProperty("tags")]
        public List<Tag> Tags;

        [JsonProperty("properties")]
        public List<Property> Properties;

        [JsonProperty("esrbRating")]
        public object EsrbRating;

        [JsonProperty("pegiRating")]
        public object PegiRating;

        [JsonProperty("uskRating")]
        public object UskRating;

        [JsonProperty("brRating")]
        public object BrRating;

        [JsonProperty("gogRating")]
        public object GogRating;

        [JsonProperty("features")]
        public List<Feature> Features;

        [JsonProperty("editions")]
        public List<object> Editions;

        [JsonProperty("series")]
        public object Series;

        [JsonProperty("language")]
        public Language Language;

        [JsonProperty("localizationScope")]
        public LocalizationScope LocalizationScope;
    }

    public class Feature
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("id")]
        public string Id;
    }

    public class Forum
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class GalaxyBackgroundImage
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Icon
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class IconSquare
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Image
    {
        [JsonProperty("href")]
        public string Href;

        [JsonProperty("templated")]
        public bool Templated;

        [JsonProperty("formatters")]
        public List<string> Formatters;
    }

    public class InDevelopment
    {
        [JsonProperty("active")]
        public bool Active;
    }

    public class Language
    {
        [JsonProperty("code")]
        public string Code;

        [JsonProperty("name")]
        public string Name;
    }

    public class Links
    {
        [JsonProperty("self")]
        public Self Self;

        [JsonProperty("store")]
        public Store Store;

        [JsonProperty("support")]
        public Support Support;

        [JsonProperty("forum")]
        public Forum Forum;

        [JsonProperty("icon")]
        public Icon Icon;

        [JsonProperty("iconSquare")]
        public IconSquare IconSquare;

        [JsonProperty("logo")]
        public Logo Logo;

        [JsonProperty("boxArtImage")]
        public BoxArtImage BoxArtImage;

        [JsonProperty("backgroundImage")]
        public BackgroundImage BackgroundImage;

        [JsonProperty("galaxyBackgroundImage")]
        public GalaxyBackgroundImage GalaxyBackgroundImage;

        [JsonProperty("image")]
        public Image Image;

        [JsonProperty("checkout")]
        public Checkout Checkout;

        [JsonProperty("prices")]
        public Prices Prices;

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail;
    }

    public class Localization
    {
        [JsonProperty("_embedded")]
        public Embedded Embedded;
    }

    public class LocalizationScope
    {
        [JsonProperty("type")]
        public string Type;
    }

    public class Logo
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class OperatingSystem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("versions")]
        public string Versions;
    }

    public class Prices
    {
        [JsonProperty("href")]
        public string Href;

        [JsonProperty("templated")]
        public bool Templated;
    }

    public class Product
    {
        [JsonProperty("isAvailableForSale")]
        public bool IsAvailableForSale;

        [JsonProperty("isVisibleInCatalog")]
        public bool IsVisibleInCatalog;

        [JsonProperty("isPreorder")]
        public bool IsPreorder;

        [JsonProperty("isVisibleInAccount")]
        public bool IsVisibleInAccount;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("isInstallable")]
        public bool IsInstallable;

        [JsonProperty("globalReleaseDate")]
        public DateTime GlobalReleaseDate;

        [JsonProperty("hasProductCard")]
        public bool HasProductCard;

        [JsonProperty("gogReleaseDate")]
        public DateTime GogReleaseDate;

        [JsonProperty("isSecret")]
        public bool IsSecret;

        [JsonProperty("_links")]
        public Links Links;
    }

    public class Property
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Publisher
    {
        [JsonProperty("name")]
        public string Name;
    }

    public class Requirement
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;
    }

    public class GogMetaData
    {
        [JsonProperty("inDevelopment")]
        public InDevelopment InDevelopment;

        [JsonProperty("copyrights")]
        public string Copyrights;

        [JsonProperty("isUsingDosBox")]
        public bool IsUsingDosBox;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("size")]
        public int Size;

        [JsonProperty("overview")]
        public string Overview;

        [JsonProperty("_links")]
        public Links Links;

        [JsonProperty("_embedded")]
        public Embedded Embedded;
    }

    public class Screenshot
    {
        [JsonProperty("_links")]
        public Links Links;
    }

    public class Self
    {
        [JsonProperty("href")]
        public string Href;

        [JsonProperty("templated")]
        public bool Templated;

        [JsonProperty("formatters")]
        public List<string> Formatters;
    }

    public class Store
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Support
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class SupportedOperatingSystem
    {
        [JsonProperty("operatingSystem")]
        public OperatingSystem OperatingSystem;

        [JsonProperty("systemRequirements")]
        public List<SystemRequirement> SystemRequirements;
    }

    public class SystemRequirement
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("requirements")]
        public List<Requirement> Requirements;
    }

    public class Tag
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("level")]
        public int Level;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Thumbnail
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Video
    {
        [JsonProperty("provider")]
        public string Provider;

        [JsonProperty("videoId")]
        public string VideoId;

        [JsonProperty("thumbnailId")]
        public string ThumbnailId;

        [JsonProperty("_links")]
        public Links Links;
    }
}
