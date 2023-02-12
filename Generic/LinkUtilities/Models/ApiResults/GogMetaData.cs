using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the gog api.
namespace LinkUtilities.Models.Gog
{
    public class ContentSystemCompatibility
    {
        [SerializationPropertyName("windows")]
        public bool Windows;

        [SerializationPropertyName("osx")]
        public bool Osx;

        [SerializationPropertyName("linux")]
        public bool Linux;
    }

    public class Images
    {
        [SerializationPropertyName("background")]
        public string Background;

        [SerializationPropertyName("logo")]
        public string Logo;

        [SerializationPropertyName("logo2x")]
        public string Logo2x;

        [SerializationPropertyName("icon")]
        public string Icon;

        [SerializationPropertyName("sidebarIcon")]
        public string SidebarIcon;

        [SerializationPropertyName("sidebarIcon2x")]
        public string SidebarIcon2x;

        [SerializationPropertyName("menuNotificationAv")]
        public string MenuNotificationAv;

        [SerializationPropertyName("menuNotificationAv2")]
        public string MenuNotificationAv2;
    }

    public class InDevelopment
    {
        [SerializationPropertyName("active")]
        public bool Active;

        [SerializationPropertyName("until")]
        public object Until;
    }

    public class Languages
    {
        [SerializationPropertyName("en")]
        public string En;
    }

    public class Links
    {
        [SerializationPropertyName("purchase_link")]
        public string PurchaseLink;

        [SerializationPropertyName("product_card")]
        public string ProductCard;

        [SerializationPropertyName("support")]
        public string Support;

        [SerializationPropertyName("forum")]
        public string Forum;
    }

    public class GogMetaData
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("purchase_link")]
        public string PurchaseLink;

        [SerializationPropertyName("slug")]
        public string Slug;

        [SerializationPropertyName("content_system_compatibility")]
        public ContentSystemCompatibility ContentSystemCompatibility;

        [SerializationPropertyName("languages")]
        public Languages Languages;

        [SerializationPropertyName("links")]
        public Links Links;

        [SerializationPropertyName("in_development")]
        public InDevelopment InDevelopment;

        [SerializationPropertyName("is_secret")]
        public bool IsSecret;

        [SerializationPropertyName("is_installable")]
        public bool IsInstallable;

        [SerializationPropertyName("game_type")]
        public string GameType;

        [SerializationPropertyName("is_pre_order")]
        public bool IsPreOrder;

        [SerializationPropertyName("release_date")]
        public DateTime ReleaseDate;

        [SerializationPropertyName("images")]
        public Images Images;

        [SerializationPropertyName("dlcs")]
        public List<object> Dlcs;
    }
}
