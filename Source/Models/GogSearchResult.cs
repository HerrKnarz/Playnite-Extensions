using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog embed url.
namespace LinkUtilities.Models
{
    public class Availability
    {
        [JsonProperty("isAvailable")]
        public bool IsAvailable;

        [JsonProperty("isAvailableInAccount")]
        public bool IsAvailableInAccount;
    }

    public class FromObject
    {
        [JsonProperty("date")]
        public string Date;

        [JsonProperty("timezone_type")]
        public int TimezoneType;

        [JsonProperty("timezone")]
        public string Timezone;
    }

    public class Price
    {
        [JsonProperty("amount")]
        public string Amount;

        [JsonProperty("baseAmount")]
        public string BaseAmount;

        [JsonProperty("finalAmount")]
        public string FinalAmount;

        [JsonProperty("isDiscounted")]
        public bool IsDiscounted;

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage;

        [JsonProperty("discountDifference")]
        public string DiscountDifference;

        [JsonProperty("symbol")]
        public string Symbol;

        [JsonProperty("isFree")]
        public bool IsFree;

        [JsonProperty("discount")]
        public int Discount;

        [JsonProperty("isBonusStoreCreditIncluded")]
        public bool IsBonusStoreCreditIncluded;

        [JsonProperty("bonusStoreCreditAmount")]
        public string BonusStoreCreditAmount;

        [JsonProperty("promoId")]
        public string PromoId;
    }

    public class Product
    {
        [JsonProperty("customAttributes")]
        public List<object> CustomAttributes;

        [JsonProperty("developer")]
        public string Developer;

        [JsonProperty("publisher")]
        public string Publisher;

        [JsonProperty("gallery")]
        public List<string> Gallery;

        [JsonProperty("video")]
        public Video Video;

        [JsonProperty("supportedOperatingSystems")]
        public List<string> SupportedOperatingSystems;

        [JsonProperty("genres")]
        public List<string> Genres;

        [JsonProperty("globalReleaseDate")]
        public int? GlobalReleaseDate;

        [JsonProperty("isTBA")]
        public bool IsTBA;

        [JsonProperty("price")]
        public Price Price;

        [JsonProperty("isDiscounted")]
        public bool IsDiscounted;

        [JsonProperty("isInDevelopment")]
        public bool IsInDevelopment;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("releaseDate")]
        public int? ReleaseDate;

        [JsonProperty("availability")]
        public Availability Availability;

        [JsonProperty("salesVisibility")]
        public SalesVisibility SalesVisibility;

        [JsonProperty("buyable")]
        public bool Buyable;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("image")]
        public string Image;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("supportUrl")]
        public string SupportUrl;

        [JsonProperty("forumUrl")]
        public string ForumUrl;

        [JsonProperty("worksOn")]
        public WorksOn WorksOn;

        [JsonProperty("category")]
        public string Category;

        [JsonProperty("originalCategory")]
        public string OriginalCategory;

        [JsonProperty("rating")]
        public int Rating;

        [JsonProperty("type")]
        public int Type;

        [JsonProperty("isComingSoon")]
        public bool IsComingSoon;

        [JsonProperty("isPriceVisible")]
        public bool IsPriceVisible;

        [JsonProperty("isMovie")]
        public bool IsMovie;

        [JsonProperty("isGame")]
        public bool IsGame;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("isWishlistable")]
        public bool IsWishlistable;

        [JsonProperty("extraInfo")]
        public List<object> ExtraInfo;

        [JsonProperty("ageLimit")]
        public int AgeLimit;
    }

    public class GogSearchResult
    {
        [JsonProperty("products")]
        public List<Product> Products;

        [JsonProperty("ts")]
        public object Ts;

        [JsonProperty("page")]
        public int Page;

        [JsonProperty("totalPages")]
        public int TotalPages;

        [JsonProperty("totalResults")]
        public string TotalResults;

        [JsonProperty("totalGamesFound")]
        public int TotalGamesFound;

        [JsonProperty("totalMoviesFound")]
        public int TotalMoviesFound;
    }

    public class SalesVisibility
    {
        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("fromObject")]
        public FromObject FromObject;

        [JsonProperty("from")]
        public int From;

        [JsonProperty("toObject")]
        public ToObject ToObject;

        [JsonProperty("to")]
        public int To;
    }

    public class ToObject
    {
        [JsonProperty("date")]
        public string Date;

        [JsonProperty("timezone_type")]
        public int TimezoneType;

        [JsonProperty("timezone")]
        public string Timezone;
    }

    public class Video
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("provider")]
        public string Provider;
    }

    public class WorksOn
    {
        [JsonProperty("Windows")]
        public bool Windows;

        [JsonProperty("Mac")]
        public bool Mac;

        [JsonProperty("Linux")]
        public bool Linux;
    }
}
