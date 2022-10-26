using LinkUtilities.Models.Itch;
using System;
using System.Net;
using Game = Playnite.SDK.Models.Game;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to the itch.io page of the game, if it is part of the steam library.
    /// </summary>
    class LibraryLinkItch : LibraryLink
    {
        public override Guid LibraryId { get; } = Guid.Parse("00000001-ebb2-4eec-abcb-7c89937a42bb");
        public override string LinkName { get; } = "Itch";

        public override bool AddLink(Game game)
        {
            // To get the link to a game on the itch website, you need an API key and request the game data from their api.
            if (!string.IsNullOrWhiteSpace(Settings.ItchApiKey) && !LinkHelper.LinkExists(game, LinkName))
            {
                try
                {
                    string apiUrl = $"https://itch.io//api/1/{Settings.ItchApiKey}/game/{game.GameId}";

                    WebClient client = new WebClient();

                    string jsonResult = client.DownloadString(apiUrl);

                    ItchMetaData itchMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ItchMetaData>(jsonResult);

                    LinkUrl = itchMetaData.Game.Url;

                    return LinkHelper.AddLink(game, LinkName, LinkUrl, Settings);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error loading data from itch.io");

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public LibraryLinkItch(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}
