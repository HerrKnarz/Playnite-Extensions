using Playnite.SDK.Data;
using System;

namespace WikipediaMetadata.Models
{
    public class Latest
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("timestamp")]
        public DateTime Timestamp;
    }

    public class License
    {
        [SerializationPropertyName("url")]
        public string Url;

        [SerializationPropertyName("title")]
        public string Title;
    }

    public class WikipediaGameData
    {
        [SerializationPropertyName("id")]
        public int Id;

        [SerializationPropertyName("key")]
        public string Key;

        [SerializationPropertyName("title")]
        public string Title;

        [SerializationPropertyName("latest")]
        public Latest Latest;

        [SerializationPropertyName("content_model")]
        public string ContentModel;

        [SerializationPropertyName("license")]
        public License License;

        [SerializationPropertyName("source")]
        public string Source;
    }


}
