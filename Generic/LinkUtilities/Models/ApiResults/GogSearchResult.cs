using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog embed URL.
namespace LinkUtilities.Models.ApiResults
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

        [JsonProperty("timezone")]
        public string Timezone;

        [JsonProperty("timezone_type")]
        public int TimezoneType;
    }

    public class Price
    {
        [JsonProperty("amount")]
        public string Amount;

        [JsonProperty("baseAmount")]
        public string BaseAmount;

        [JsonProperty("bonusStoreCreditAmount")]
        public string BonusStoreCreditAmount;

        [JsonProperty("discount")]
        public int Discount;

        [JsonProperty("discountDifference")]
        public string DiscountDifference;

        [JsonProperty("discountPercentage")]
        public int DiscountPercentage;

        [JsonProperty("finalAmount")]
        public string FinalAmount;

        [JsonProperty("isBonusStoreCreditIncluded")]
        public bool IsBonusStoreCreditIncluded;

        [JsonProperty("isDiscounted")]
        public bool IsDiscounted;

        [JsonProperty("isFree")]
        public bool IsFree;

        [JsonProperty("promoId")]
        public string PromoId;

        [JsonProperty("symbol")]
        public string Symbol;
    }

    public class Product
    {
        [JsonProperty("ageLimit")]
        public int AgeLimit;

        [JsonProperty("availability")]
        public Availability Availability;

        [JsonProperty("buyable")]
        public bool Buyable;

        [JsonProperty("category")]
        public string Category;

        [JsonProperty("customAttributes")]
        public List<object> CustomAttributes;

        [JsonProperty("developer")]
        public string Developer;

        [JsonProperty("extraInfo")]
        public List<object> ExtraInfo;

        [JsonProperty("forumUrl")]
        public string ForumUrl;

        [JsonProperty("gallery")]
        public List<string> Gallery;

        [JsonProperty("genres")]
        public List<string> Genres;

        [JsonProperty("globalReleaseDate")]
        public int? GlobalReleaseDate;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("image")]
        public string Image;

        [JsonProperty("isComingSoon")]
        public bool IsComingSoon;

        [JsonProperty("isDiscounted")]
        public bool IsDiscounted;

        [JsonProperty("isGame")]
        public bool IsGame;

        [JsonProperty("isInDevelopment")]
        public bool IsInDevelopment;

        [JsonProperty("isMovie")]
        public bool IsMovie;

        [JsonProperty("isPriceVisible")]
        public bool IsPriceVisible;

        [JsonProperty("isTBA")]
        public bool IsTba;

        [JsonProperty("isWishlistable")]
        public bool IsWishlistable;

        [JsonProperty("originalCategory")]
        public string OriginalCategory;

        [JsonProperty("price")]
        public Price Price;

        [JsonProperty("publisher")]
        public string Publisher;

        [JsonProperty("rating")]
        public int Rating;

        [JsonProperty("releaseDate")]
        public int? ReleaseDate;

        [JsonProperty("salesVisibility")]
        public SalesVisibility SalesVisibility;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("supportedOperatingSystems")]
        public List<string> SupportedOperatingSystems;

        [JsonProperty("supportUrl")]
        public string SupportUrl;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("type")]
        public int Type;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("video")]
        public Video Video;

        [JsonProperty("worksOn")]
        public WorksOn WorksOn;
    }

    public class GogSearchResult
    {
        [JsonProperty("page")]
        public int Page;

        [JsonProperty("products")]
        public List<Product> Products;

        [JsonProperty("totalGamesFound")]
        public int TotalGamesFound;

        [JsonProperty("totalMoviesFound")]
        public int TotalMoviesFound;

        [JsonProperty("totalPages")]
        public int TotalPages;

        [JsonProperty("totalResults")]
        public string TotalResults;

        [JsonProperty("ts")]
        public object Ts;
    }

    public class SalesVisibility
    {
        [JsonProperty("from")]
        public int From;

        [JsonProperty("fromObject")]
        public FromObject FromObject;

        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("to")]
        public int To;

        [JsonProperty("toObject")]
        public ToObject ToObject;
    }

    public class ToObject
    {
        [JsonProperty("date")]
        public string Date;

        [JsonProperty("timezone")]
        public string Timezone;

        [JsonProperty("timezone_type")]
        public int TimezoneType;
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
        [JsonProperty("Linux")]
        public bool Linux;

        [JsonProperty("Mac")]
        public bool Mac;

        [JsonProperty("Windows")]
        public bool Windows;
    }
}