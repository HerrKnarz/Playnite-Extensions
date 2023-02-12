using Playnite.SDK.Data;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the Epic graphql api.
namespace LinkUtilities.Models.Epic
{
    public class Catalog
    {
        [SerializationPropertyName("searchStore")]
        public SearchStore SearchStore;
    }

    public class Data
    {
        [SerializationPropertyName("Catalog")]
        public Catalog Catalog;
    }

    public class Element
    {
        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("urlSlug")]
        public string UrlSlug;

        [SerializationPropertyName("seller")]
        public Seller Seller;
    }

    public class Extensions
    {
    }

    public class EpicSearchResult
    {
        [SerializationPropertyName("data")]
        public Data Data;

        [SerializationPropertyName("extensions")]
        public Extensions Extensions;
    }

    public class SearchStore
    {
        [SerializationPropertyName("elements")]
        public List<Element> Elements;
    }

    public class Seller
    {
        [SerializationPropertyName("name")]
        public string Name;
    }


}
