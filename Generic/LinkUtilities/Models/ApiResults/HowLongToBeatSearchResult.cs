using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.ApiResults
{
    public class Datum
    {
        [JsonProperty("comp_100")]
        public int Comp100;

        [JsonProperty("comp_100_count")]
        public int Comp100Count;

        [JsonProperty("comp_all")]
        public int CompAll;

        [JsonProperty("comp_all_count")]
        public int CompAllCount;

        [JsonProperty("comp_lvl_co")]
        public int CompLvlCo;

        [JsonProperty("comp_lvl_combine")]
        public int CompLvlCombine;

        [JsonProperty("comp_lvl_mp")]
        public int CompLvlMp;

        [JsonProperty("comp_lvl_sp")]
        public int CompLvlSp;

        [JsonProperty("comp_lvl_spd")]
        public int CompLvlSpd;

        [JsonProperty("comp_main")]
        public int CompMain;

        [JsonProperty("comp_main_count")]
        public int CompMainCount;

        [JsonProperty("comp_plus")]
        public int CompPlus;

        [JsonProperty("comp_plus_count")]
        public int CompPlusCount;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("count_backlog")]
        public int CountBacklog;

        [JsonProperty("count_comp")]
        public int CountComp;

        [JsonProperty("count_playing")]
        public int CountPlaying;

        [JsonProperty("count_retired")]
        public int CountRetired;

        [JsonProperty("count_review")]
        public int CountReview;

        [JsonProperty("count_speedrun")]
        public int CountSpeedrun;

        [JsonProperty("game_alias")]
        public string GameAlias;

        [JsonProperty("game_id")]
        public int GameId;

        [JsonProperty("game_image")]
        public string GameImage;

        [JsonProperty("game_name")]
        public string GameName;

        [JsonProperty("game_name_date")]
        public int GameNameDate;

        [JsonProperty("game_type")]
        public string GameType;

        [JsonProperty("invested_co")]
        public int InvestedCo;

        [JsonProperty("invested_co_count")]
        public int InvestedCoCount;

        [JsonProperty("invested_mp")]
        public int InvestedMp;

        [JsonProperty("invested_mp_count")]
        public int InvestedMpCount;

        [JsonProperty("profile_dev")]
        public string ProfileDev;

        [JsonProperty("profile_platform")]
        public string ProfilePlatform;

        [JsonProperty("profile_popular")]
        public int ProfilePopular;

        [JsonProperty("profile_steam")]
        public int ProfileSteam;

        [JsonProperty("release_world")]
        public int ReleaseWorld;

        [JsonProperty("review_score")]
        public int ReviewScore;
    }

    public class HowLongToBeatSearchResult
    {
        [JsonProperty("category")]
        public string Category;

        [JsonProperty("color")]
        public string Color;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("data")]
        public List<Datum> Data;

        [JsonProperty("displayModifier")]
        public object DisplayModifier;

        [JsonProperty("pageCurrent")]
        public int PageCurrent;

        [JsonProperty("pageSize")]
        public int PageSize;

        [JsonProperty("pageTotal")]
        public int PageTotal;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("userData")]
        public List<object> UserData;
    }
}