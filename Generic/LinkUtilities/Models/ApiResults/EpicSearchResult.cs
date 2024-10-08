﻿using Newtonsoft.Json;
using System.Collections.Generic;

// Contains all the classes needed to deserialize the JSON fetched from the Epic graphql api.
namespace LinkUtilities.Models.ApiResults
{
    public class Catalog
    {
        [JsonProperty("searchStore")]
        public SearchStore SearchStore;
    }

    public class Data
    {
        [JsonProperty("Catalog")]
        public Catalog Catalog;
    }

    public class Element
    {
        [JsonProperty("seller")]
        public Seller Seller;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("urlSlug")]
        public string UrlSlug;
    }

    public class Extensions {}

    public class EpicSearchResult
    {
        [JsonProperty("data")]
        public Data Data;

        [JsonProperty("extensions")]
        public Extensions Extensions;
    }

    public class SearchStore
    {
        [JsonProperty("elements")]
        public List<Element> Elements;
    }

    public class Seller
    {
        [JsonProperty("name")]
        public string Name;
    }
}