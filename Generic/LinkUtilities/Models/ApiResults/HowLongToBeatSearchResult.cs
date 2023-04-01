using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkUtilities.Models.HowLongToBeat
{
    public class Datum
    {
        [JsonProperty("count")]
        public int Count;

        [JsonProperty("game_id")]
        public int GameId;

        [JsonProperty("game_name")]
        public string GameName;

        [JsonProperty("game_name_date")]
        public int GameNameDate;

        [JsonProperty("game_alias")]
        public string GameAlias;

        [JsonProperty("game_type")]
        public string GameType;

        [JsonProperty("game_image")]
        public string GameImage;

        [JsonProperty("comp_lvl_combine")]
        public int CompLvlCombine;

        [JsonProperty("comp_lvl_sp")]
        public int CompLvlSp;

        [JsonProperty("comp_lvl_co")]
        public int CompLvlCo;

        [JsonProperty("comp_lvl_mp")]
        public int CompLvlMp;

        [JsonProperty("comp_lvl_spd")]
        public int CompLvlSpd;

        [JsonProperty("comp_main")]
        public int CompMain;

        [JsonProperty("comp_plus")]
        public int CompPlus;

        [JsonProperty("comp_100")]
        public int Comp100;

        [JsonProperty("comp_all")]
        public int CompAll;

        [JsonProperty("comp_main_count")]
        public int CompMainCount;

        [JsonProperty("comp_plus_count")]
        public int CompPlusCount;

        [JsonProperty("comp_100_count")]
        public int Comp100Count;

        [JsonProperty("comp_all_count")]
        public int CompAllCount;

        [JsonProperty("invested_co")]
        public int InvestedCo;

        [JsonProperty("invested_mp")]
        public int InvestedMp;

        [JsonProperty("invested_co_count")]
        public int InvestedCoCount;

        [JsonProperty("invested_mp_count")]
        public int InvestedMpCount;

        [JsonProperty("count_comp")]
        public int CountComp;

        [JsonProperty("count_speedrun")]
        public int CountSpeedrun;

        [JsonProperty("count_backlog")]
        public int CountBacklog;

        [JsonProperty("count_review")]
        public int CountReview;

        [JsonProperty("review_score")]
        public int ReviewScore;

        [JsonProperty("count_playing")]
        public int CountPlaying;

        [JsonProperty("count_retired")]
        public int CountRetired;

        [JsonProperty("profile_dev")]
        public string ProfileDev;

        [JsonProperty("profile_popular")]
        public int ProfilePopular;

        [JsonProperty("profile_steam")]
        public int ProfileSteam;

        [JsonProperty("profile_platform")]
        public string ProfilePlatform;

        [JsonProperty("release_world")]
        public int ReleaseWorld;
    }

    public class HowLongToBeatSearchResult
    {
        [JsonProperty("color")]
        public string Color;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("category")]
        public string Category;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("pageCurrent")]
        public int PageCurrent;

        [JsonProperty("pageTotal")]
        public int PageTotal;

        [JsonProperty("pageSize")]
        public int PageSize;

        [JsonProperty("data")]
        public List<Datum> Data;

        [JsonProperty("userData")]
        public List<object> UserData;

        [JsonProperty("displayModifier")]
        public object DisplayModifier;
    }
}