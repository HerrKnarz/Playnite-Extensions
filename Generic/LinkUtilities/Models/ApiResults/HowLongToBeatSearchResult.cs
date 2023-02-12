using Playnite.SDK.Data;
using System.Collections.Generic;

namespace LinkUtilities.Models.HowLongToBeat
{
    public class Datum
    {
        [SerializationPropertyName("count")]
        public int Count;

        [SerializationPropertyName("game_id")]
        public int GameId;

        [SerializationPropertyName("game_name")]
        public string GameName;

        [SerializationPropertyName("game_name_date")]
        public int GameNameDate;

        [SerializationPropertyName("game_alias")]
        public string GameAlias;

        [SerializationPropertyName("game_type")]
        public string GameType;

        [SerializationPropertyName("game_image")]
        public string GameImage;

        [SerializationPropertyName("comp_lvl_combine")]
        public int CompLvlCombine;

        [SerializationPropertyName("comp_lvl_sp")]
        public int CompLvlSp;

        [SerializationPropertyName("comp_lvl_co")]
        public int CompLvlCo;

        [SerializationPropertyName("comp_lvl_mp")]
        public int CompLvlMp;

        [SerializationPropertyName("comp_lvl_spd")]
        public int CompLvlSpd;

        [SerializationPropertyName("comp_main")]
        public int CompMain;

        [SerializationPropertyName("comp_plus")]
        public int CompPlus;

        [SerializationPropertyName("comp_100")]
        public int Comp100;

        [SerializationPropertyName("comp_all")]
        public int CompAll;

        [SerializationPropertyName("comp_main_count")]
        public int CompMainCount;

        [SerializationPropertyName("comp_plus_count")]
        public int CompPlusCount;

        [SerializationPropertyName("comp_100_count")]
        public int Comp100Count;

        [SerializationPropertyName("comp_all_count")]
        public int CompAllCount;

        [SerializationPropertyName("invested_co")]
        public int InvestedCo;

        [SerializationPropertyName("invested_mp")]
        public int InvestedMp;

        [SerializationPropertyName("invested_co_count")]
        public int InvestedCoCount;

        [SerializationPropertyName("invested_mp_count")]
        public int InvestedMpCount;

        [SerializationPropertyName("count_comp")]
        public int CountComp;

        [SerializationPropertyName("count_speedrun")]
        public int CountSpeedrun;

        [SerializationPropertyName("count_backlog")]
        public int CountBacklog;

        [SerializationPropertyName("count_review")]
        public int CountReview;

        [SerializationPropertyName("review_score")]
        public int ReviewScore;

        [SerializationPropertyName("count_playing")]
        public int CountPlaying;

        [SerializationPropertyName("count_retired")]
        public int CountRetired;

        [SerializationPropertyName("profile_dev")]
        public string ProfileDev;

        [SerializationPropertyName("profile_popular")]
        public int ProfilePopular;

        [SerializationPropertyName("profile_steam")]
        public int ProfileSteam;

        [SerializationPropertyName("profile_platform")]
        public string ProfilePlatform;

        [SerializationPropertyName("release_world")]
        public int ReleaseWorld;
    }

    public class HowLongToBeatSearchResult
    {
        [SerializationPropertyName("color")]
        public string Color;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("category")]
        public string Category;

        [SerializationPropertyName("count")]
        public int Count;

        [SerializationPropertyName("pageCurrent")]
        public int PageCurrent;

        [SerializationPropertyName("pageTotal")]
        public int PageTotal;

        [SerializationPropertyName("pageSize")]
        public int PageSize;

        [SerializationPropertyName("data")]
        public List<Datum> Data;

        [SerializationPropertyName("userData")]
        public List<object> UserData;

        [SerializationPropertyName("displayModifier")]
        public object DisplayModifier;
    }
}
