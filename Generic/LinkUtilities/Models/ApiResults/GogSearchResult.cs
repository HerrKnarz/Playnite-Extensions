using Playnite.SDK.Data;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog embed URL.
namespace LinkUtilities.Models.Gog
{
    public class Availability
    {
        [SerializationPropertyName("isAvailable")]
        public bool IsAvailable;

        [SerializationPropertyName("isAvailableInAccount")]
        public bool IsAvailableInAccount;
    }

    public class FromObject
    {
        [SerializationPropertyName("date")]
        public string Date;

        [SerializationPropertyName("timezone_type")]
        public int TimezoneType;

        [SerializationPropertyName("timezone")]
        public string Timezone;
    }

    public class Price
    {
        [SerializationPropertyName("amount")]
        public string Amount;

        [SerializationPropertyName("baseAmount")]
        public string BaseAmount;

        [SerializationPropertyName("finalAmount")]
        public string FinalAmount;

        [SerializationPropertyName("isDiscounted")]
        public bool IsDiscounted;

        [SerializationPropertyName("discountPercentage")]
        public int DiscountPercentage;

        [SerializationPropertyName("discountDifference")]
        public string DiscountDifference;

        [SerializationPropertyName("symbol")]
        public string Symbol;

        [SerializationPropertyName("isFree")]
        public bool IsFree;

        [SerializationPropertyName("discount")]
        public int Discount;

        [SerializationPropertyName("isBonusStoreCreditIncluded")]
        public bool IsBonusStoreCreditIncluded;

        [SerializationPropertyName("bonusStoreCreditAmount")]
        public string BonusStoreCreditAmount;

        [SerializationPropertyName("promoId")]
        public string PromoId;
    }

    public class Product
    {
        [SerializationPropertyName("customAttributes")]
        public List<object> CustomAttributes;

        [SerializationPropertyName("developer")]
        public string Developer;

        [SerializationPropertyName("publisher")]
        public string Publisher;

        [SerializationPropertyName("gallery")]
        public List<string> Gallery;

        [SerializationPropertyName("video")]
        public Video Video;

        [SerializationPropertyName("supportedOperatingSystems")]
        public List<string> SupportedOperatingSystems;

        [SerializationPropertyName("genres")]
        public List<string> Genres;

        [SerializationPropertyName("globalReleaseDate")]
        public int? GlobalReleaseDate;

        [SerializationPropertyName("isTBA")]
        public bool IsTBA;

        [SerializationPropertyName("price")]
        public Price Price;

        [SerializationPropertyName("isDiscounted")]
        public bool IsDiscounted;

        [SerializationPropertyName("isInDevelopment")]
        public bool IsInDevelopment;

        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("releaseDate")]
        public int? ReleaseDate;

        [SerializationPropertyName("availability")]
        public Availability Availability;

        [SerializationPropertyName("salesVisibility")]
        public SalesVisibility SalesVisibility;

        [SerializationPropertyName("buyable")]
        public bool Buyable;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("image")]
        public string Image;

        [SerializationPropertyName("url")]
        public string Url;

        [SerializationPropertyName("supportUrl")]
        public string SupportUrl;

        [SerializationPropertyName("forumUrl")]
        public string ForumUrl;

        [SerializationPropertyName("worksOn")]
        public WorksOn WorksOn;

        [SerializationPropertyName("category")]
        public string Category;

        [SerializationPropertyName("originalCategory")]
        public string OriginalCategory;

        [SerializationPropertyName("rating")]
        public int Rating;

        [SerializationPropertyName("type")]
        public int Type;

        [SerializationPropertyName("isComingSoon")]
        public bool IsComingSoon;

        [SerializationPropertyName("isPriceVisible")]
        public bool IsPriceVisible;

        [SerializationPropertyName("isMovie")]
        public bool IsMovie;

        [SerializationPropertyName("isGame")]
        public bool IsGame;

        [SerializationPropertyName("slug")]
        public string Slug;

        [SerializationPropertyName("isWishlistable")]
        public bool IsWishlistable;

        [SerializationPropertyName("extraInfo")]
        public List<object> ExtraInfo;

        [SerializationPropertyName("ageLimit")]
        public int AgeLimit;
    }

    public class GogSearchResult
    {
        [SerializationPropertyName("products")]
        public List<Product> Products;

        [SerializationPropertyName("ts")]
        public object Ts;

        [SerializationPropertyName("page")]
        public int Page;

        [SerializationPropertyName("totalPages")]
        public int TotalPages;

        [SerializationPropertyName("totalResults")]
        public string TotalResults;

        [SerializationPropertyName("totalGamesFound")]
        public int TotalGamesFound;

        [SerializationPropertyName("totalMoviesFound")]
        public int TotalMoviesFound;
    }

    public class SalesVisibility
    {
        [SerializationPropertyName("isActive")]
        public bool IsActive;

        [SerializationPropertyName("fromObject")]
        public FromObject FromObject;

        [SerializationPropertyName("from")]
        public int From;

        [SerializationPropertyName("toObject")]
        public ToObject ToObject;

        [SerializationPropertyName("to")]
        public int To;
    }

    public class ToObject
    {
        [SerializationPropertyName("date")]
        public string Date;

        [SerializationPropertyName("timezone_type")]
        public int TimezoneType;

        [SerializationPropertyName("timezone")]
        public string Timezone;
    }

    public class Video
    {
        [SerializationPropertyName("id")]
        public string Id;

        [SerializationPropertyName("provider")]
        public string Provider;
    }

    public class WorksOn
    {
        [SerializationPropertyName("Windows")]
        public bool Windows;

        [SerializationPropertyName("Mac")]
        public bool Mac;

        [SerializationPropertyName("Linux")]
        public bool Linux;
    }
}
