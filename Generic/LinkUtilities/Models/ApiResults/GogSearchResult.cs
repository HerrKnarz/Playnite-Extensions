using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog catalog URL.
namespace LinkUtilities.Models.ApiResults
{
    public class BaseMoney
    {
        [JsonProperty("amount")]
        public string Amount;

        [JsonProperty("currency")]
        public string Currency;
    }

    public class Edition
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("isRootEdition")]
        public bool IsRootEdition;
    }

    public class Feature
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Filters
    {
        [JsonProperty("releaseDateRange")]
        public ReleaseDateRange ReleaseDateRange;

        [JsonProperty("priceRange")]
        public PriceRange PriceRange;

        [JsonProperty("genres")]
        public List<Genre> Genres;

        [JsonProperty("languages")]
        public List<Language> Languages;

        [JsonProperty("systems")]
        public List<PlaySystem> Systems;

        [JsonProperty("tags")]
        public List<Tag> Tags;

        [JsonProperty("discounted")]
        public bool Discounted;

        [JsonProperty("features")]
        public List<Feature> Features;

        [JsonProperty("releaseStatuses")]
        public List<ReleaseStatus> ReleaseStatuses;

        [JsonProperty("types")]
        public List<string> Types;

        [JsonProperty("fullGenresList")]
        public List<FullGenresList> FullGenresList;

        [JsonProperty("fullTagsList")]
        public List<FullTagsList> FullTagsList;
    }

    public class FinalMoney
    {
        [JsonProperty("amount")]
        public string Amount;

        [JsonProperty("currency")]
        public string Currency;

        [JsonProperty("discount")]
        public string Discount;
    }

    public class FullGenresList
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("level")]
        public int Level;
    }

    public class FullTagsList
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Genre
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Language
    {
        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("name")]
        public string Name;
    }

    public class Price
    {
        [JsonProperty("final")]
        public string Final;

        [JsonProperty("base")]
        public string Base;

        [JsonProperty("discount")]
        public object Discount;

        [JsonProperty("finalMoney")]
        public FinalMoney FinalMoney;

        [JsonProperty("baseMoney")]
        public BaseMoney BaseMoney;
    }

    public class PriceRange
    {
        [JsonProperty("min")]
        public double Min;

        [JsonProperty("max")]
        public double Max;

        [JsonProperty("currency")]
        public string Currency;

        [JsonProperty("decimalPlaces")]
        public int DecimalPlaces;
    }

    public class Product
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("features")]
        public List<Feature> Features;

        [JsonProperty("screenshots")]
        public List<string> Screenshots;

        [JsonProperty("userPreferredLanguage")]
        public UserPreferredLanguage UserPreferredLanguage;

        [JsonProperty("releaseDate")]
        public string ReleaseDate;

        [JsonProperty("storeReleaseDate")]
        public string StoreReleaseDate;

        [JsonProperty("productType")]
        public string ProductType;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("coverHorizontal")]
        public string CoverHorizontal;

        [JsonProperty("coverVertical")]
        public string CoverVertical;

        [JsonProperty("developers")]
        public List<string> Developers;

        [JsonProperty("publishers")]
        public List<string> Publishers;

        [JsonProperty("operatingSystems")]
        public List<string> OperatingSystems;

        [JsonProperty("price")]
        public Price Price;

        [JsonProperty("productState")]
        public string ProductState;

        [JsonProperty("genres")]
        public List<Genre> Genres;

        [JsonProperty("tags")]
        public List<Tag> Tags;

        [JsonProperty("reviewsRating")]
        public int ReviewsRating;

        [JsonProperty("editions")]
        public List<Edition> Editions;

        [JsonProperty("ratings")]
        public List<Rating> Ratings;

        [JsonProperty("storeLink")]
        public string StoreLink;
    }

    public class Rating
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("ageRating")]
        public string AgeRating;
    }

    public class ReleaseDateRange
    {
        [JsonProperty("min")]
        public int Min;

        [JsonProperty("max")]
        public int Max;
    }

    public class ReleaseStatus
    {
        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("name")]
        public string Name;
    }

    public class GogSearchResult
    {
        [JsonProperty("pages")]
        public int Pages;

        [JsonProperty("currentlyShownProductCount")]
        public int CurrentlyShownProductCount;

        [JsonProperty("productCount")]
        public int ProductCount;

        [JsonProperty("products")]
        public List<Product> Products;

        [JsonProperty("filters")]
        public Filters Filters;

        [JsonProperty("searchAlgo")]
        public string SearchAlgo;
    }

    public class PlaySystem
    {
        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("name")]
        public string Name;
    }

    public class Tag
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class UserPreferredLanguage
    {
        [JsonProperty("code")]
        public string Code;

        [JsonProperty("inAudio")]
        public bool InAudio;

        [JsonProperty("inText")]
        public bool InText;
    }
}