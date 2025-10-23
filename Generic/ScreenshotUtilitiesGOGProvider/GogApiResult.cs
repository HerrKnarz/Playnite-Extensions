using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScreenshotUtilitiesGOGProvider
{
    public class FormattedImage
    {
        [JsonProperty("formatter_name")]
        public string FormatterName { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }

    public class Images
    {
        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("logo2x")]
        public string Logo2x { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("sidebarIcon")]
        public string SidebarIcon { get; set; }

        [JsonProperty("sidebarIcon2x")]
        public string SidebarIcon2x { get; set; }

        [JsonProperty("menuNotificationAv")]
        public string MenuNotificationAv { get; set; }

        [JsonProperty("menuNotificationAv2")]
        public string MenuNotificationAv2 { get; set; }
    }

    public class GogApiResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("screenshots")]
        public List<Screenshot> Screenshots { get; set; }
    }

    public class Screenshot
    {
        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("formatter_template_url")]
        public string FormatterTemplateUrl { get; set; }

        [JsonProperty("formatted_images")]
        public List<FormattedImage> FormattedImages { get; set; }
    }
}
