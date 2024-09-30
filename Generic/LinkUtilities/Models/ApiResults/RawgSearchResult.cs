using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the RAWG api.
// ReSharper disable once CheckNamespace
namespace LinkUtilities.Models.RAWG
{
    public class AddedByStatus
    {
        [JsonProperty("beaten")]
        public int Beaten;

        [JsonProperty("dropped")]
        public int Dropped;

        [JsonProperty("owned")]
        public int Owned;

        [JsonProperty("playing")]
        public int Playing;

        [JsonProperty("toplay")]
        public int ToPlay;

        [JsonProperty("yet")]
        public int Yet;
    }

    public class EsrbRating
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("name_en")]
        public string NameEn;

        [JsonProperty("name_ru")]
        public string NameRu;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Genre
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class ParentPlatform
    {
        [JsonProperty("platform")]
        public Platform Platform;
    }

    public class Platform
    {
        [JsonProperty("platform")]
        public Platform PlatformSub;
    }

    public class Platform2
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Rating
    {
        [JsonProperty("count")]
        public int Count;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("percent")]
        public double Percent;

        [JsonProperty("title")]
        public string Title;
    }

    public class Result
    {
        [JsonProperty("added")]
        public int Added;

        [JsonProperty("added_by_status")]
        public AddedByStatus AddedByStatus;

        [JsonProperty("background_image")]
        public string BackgroundImage;

        [JsonProperty("clip")]
        public object Clip;

        [JsonProperty("community_rating")]
        public int? CommunityRating;

        [JsonProperty("dominant_color")]
        public string DominantColor;

        [JsonProperty("esrb_rating")]
        public EsrbRating EsrbRating;

        [JsonProperty("genres")]
        public List<Genre> Genres;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("metacritic")]
        public int? Metacritic;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("parent_platforms")]
        public List<ParentPlatform> ParentPlatforms;

        [JsonProperty("platforms")]
        public List<Platform> Platforms;

        [JsonProperty("playtime")]
        public int Playtime;

        [JsonProperty("rating")]
        public double Rating;

        [JsonProperty("ratings")]
        public List<Rating> Ratings;

        [JsonProperty("ratings_count")]
        public int RatingsCount;

        [JsonProperty("rating_top")]
        public int RatingTop;

        [JsonProperty("released")]
        public string Released;

        [JsonProperty("reviews_count")]
        public int ReviewsCount;

        [JsonProperty("reviews_text_count")]
        public int ReviewsTextCount;

        [JsonProperty("saturated_color")]
        public string SaturatedColor;

        [JsonProperty("score")]
        public string Score;

        [JsonProperty("short_screenshots")]
        public List<ShortScreenshot> ShortScreenshots;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("stores")]
        public List<Store> Stores;

        [JsonProperty("suggestions_count")]
        public int SuggestionsCount;

        [JsonProperty("tags")]
        public List<Tag> Tags;

        [JsonProperty("tba")]
        public bool Tba;

        [JsonProperty("updated")]
        public DateTime Updated;

        [JsonProperty("user_game")]
        public object UserGame;
    }

    public class RawgSearchResult
    {
        [JsonProperty("count")]
        public int Count;

        [JsonProperty("next")]
        public object Next;

        [JsonProperty("previous")]
        public object Previous;

        [JsonProperty("results")]
        public List<Result> Results;

        [JsonProperty("user_platforms")]
        public bool UserPlatforms;
    }

    public class ShortScreenshot
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("image")]
        public string Image;
    }

    public class Store
    {
        [JsonProperty("store")]
        public Store StoreSub;
    }

    public class Store2
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }

    public class Tag
    {
        [JsonProperty("games_count")]
        public int GamesCount;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("image_background")]
        public string ImageBackground;

        [JsonProperty("language")]
        public string Language;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;
    }
}