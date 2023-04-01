using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the RAWG api.
namespace LinkUtilities.Models.RAWG
{
    public class AddedByStatus
    {
        [JsonProperty("yet")]
        public int Yet;

        [JsonProperty("owned")]
        public int Owned;

        [JsonProperty("beaten")]
        public int Beaten;

        [JsonProperty("toplay")]
        public int ToPlay;

        [JsonProperty("dropped")]
        public int Dropped;

        [JsonProperty("playing")]
        public int Playing;
    }

    public class EsrbRating
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("name_en")]
        public string NameEn;

        [JsonProperty("name_ru")]
        public string NameRu;
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
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("percent")]
        public double Percent;
    }

    public class Result
    {
        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("playtime")]
        public int Playtime;

        [JsonProperty("platforms")]
        public List<Platform> Platforms;

        [JsonProperty("stores")]
        public List<Store> Stores;

        [JsonProperty("released")]
        public string Released;

        [JsonProperty("tba")]
        public bool Tba;

        [JsonProperty("background_image")]
        public string BackgroundImage;

        [JsonProperty("rating")]
        public double Rating;

        [JsonProperty("rating_top")]
        public int RatingTop;

        [JsonProperty("ratings")]
        public List<Rating> Ratings;

        [JsonProperty("ratings_count")]
        public int RatingsCount;

        [JsonProperty("reviews_text_count")]
        public int ReviewsTextCount;

        [JsonProperty("added")]
        public int Added;

        [JsonProperty("added_by_status")]
        public AddedByStatus AddedByStatus;

        [JsonProperty("metacritic")]
        public int? Metacritic;

        [JsonProperty("suggestions_count")]
        public int SuggestionsCount;

        [JsonProperty("updated")]
        public DateTime Updated;

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("score")]
        public string Score;

        [JsonProperty("clip")]
        public object Clip;

        [JsonProperty("tags")]
        public List<Tag> Tags;

        [JsonProperty("esrb_rating")]
        public EsrbRating EsrbRating;

        [JsonProperty("user_game")]
        public object UserGame;

        [JsonProperty("reviews_count")]
        public int ReviewsCount;

        [JsonProperty("saturated_color")]
        public string SaturatedColor;

        [JsonProperty("dominant_color")]
        public string DominantColor;

        [JsonProperty("short_screenshots")]
        public List<ShortScreenshot> ShortScreenshots;

        [JsonProperty("parent_platforms")]
        public List<ParentPlatform> ParentPlatforms;

        [JsonProperty("genres")]
        public List<Genre> Genres;

        [JsonProperty("community_rating")]
        public int? CommunityRating;
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
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("language")]
        public string Language;

        [JsonProperty("games_count")]
        public int GamesCount;

        [JsonProperty("image_background")]
        public string ImageBackground;
    }
}