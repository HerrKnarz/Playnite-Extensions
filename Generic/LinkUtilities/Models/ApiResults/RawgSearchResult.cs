using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the RAWG api.
namespace LinkUtilities.Models.RAWG
{
    public class AddedByStatus
    {
        [SerializationPropertyName("yet")]
        public int Yet;

        [SerializationPropertyName("owned")]
        public int Owned;

        [SerializationPropertyName("beaten")]
        public int Beaten;

        [SerializationPropertyName("toplay")]
        public int Toplay;

        [SerializationPropertyName("dropped")]
        public int Dropped;

        [SerializationPropertyName("playing")]
        public int Playing;
    }

    public class EsrbRating
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("slug")]
        public string Slug;

        [SerializationPropertyName("name_en")]
        public string NameEn;

        [SerializationPropertyName("name_ru")]
        public string NameRu;
    }

    public class Genre
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("slug")]
        public string Slug;
    }

    public class ParentPlatform
    {
        [SerializationPropertyName("platform")]
        public Platform Platform;
    }

    public class Platform
    {
        [SerializationPropertyName("platform")]
        public Platform PlatformSub;
    }

    public class Platform2
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("slug")]
        public string Slug;
    }

    public class Rating
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("count")]
        public int Count;

        [SerializationPropertyName("percent")]
        public double Percent;
    }

    public class Result
    {
        [SerializationPropertyName("slug")]
        public string Slug;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("playtime")]
        public int Playtime;

        [SerializationPropertyName("platforms")]
        public List<Platform> Platforms;

        [SerializationPropertyName("stores")]
        public List<Store> Stores;

        [SerializationPropertyName("released")]
        public string Released;

        [SerializationPropertyName("tba")]
        public bool Tba;

        [SerializationPropertyName("background_image")]
        public string BackgroundImage;

        [SerializationPropertyName("rating")]
        public double Rating;

        [SerializationPropertyName("rating_top")]
        public int RatingTop;

        [SerializationPropertyName("ratings")]
        public List<Rating> Ratings;

        [SerializationPropertyName("ratings_count")]
        public int RatingsCount;

        [SerializationPropertyName("reviews_text_count")]
        public int ReviewsTextCount;

        [SerializationPropertyName("added")]
        public int Added;

        [SerializationPropertyName("added_by_status")]
        public AddedByStatus AddedByStatus;

        [SerializationPropertyName("metacritic")]
        public int? Metacritic;

        [SerializationPropertyName("suggestions_count")]
        public int SuggestionsCount;

        [SerializationPropertyName("updated")]
        public DateTime Updated;

        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("score")]
        public string Score;

        [SerializationPropertyName("clip")]
        public object Clip;

        [SerializationPropertyName("tags")]
        public List<Tag> Tags;

        [SerializationPropertyName("esrb_rating")]
        public EsrbRating EsrbRating;

        [SerializationPropertyName("user_game")]
        public object UserGame;

        [SerializationPropertyName("reviews_count")]
        public int ReviewsCount;

        [SerializationPropertyName("saturated_color")]
        public string SaturatedColor;

        [SerializationPropertyName("dominant_color")]
        public string DominantColor;

        [SerializationPropertyName("short_screenshots")]
        public List<ShortScreenshot> ShortScreenshots;

        [SerializationPropertyName("parent_platforms")]
        public List<ParentPlatform> ParentPlatforms;

        [SerializationPropertyName("genres")]
        public List<Genre> Genres;

        [SerializationPropertyName("community_rating")]
        public int? CommunityRating;
    }

    public class RawgSearchResult
    {
        [SerializationPropertyName("count")]
        public int Count;

        [SerializationPropertyName("next")]
        public object Next;

        [SerializationPropertyName("previous")]
        public object Previous;

        [SerializationPropertyName("results")]
        public List<Result> Results;

        [SerializationPropertyName("user_platforms")]
        public bool UserPlatforms;
    }

    public class ShortScreenshot
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("image")]
        public string Image;
    }

    public class Store
    {
        [SerializationPropertyName("store")]
        public Store StoreSub;
    }

    public class Store2
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("slug")]
        public string Slug;
    }

    public class Tag
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("name")]
        public string Name;

        [SerializationPropertyName("slug")]
        public string Slug;

        [SerializationPropertyName("language")]
        public string Language;

        [SerializationPropertyName("games_count")]
        public int GamesCount;

        [SerializationPropertyName("image_background")]
        public string ImageBackground;
    }
}