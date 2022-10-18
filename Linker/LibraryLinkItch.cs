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
        public override bool AddLink(Game game)
        {
            // To get the link to a game on the itch website, you need an API key and request the game data from their api.
            if (!string.IsNullOrWhiteSpace(Settings.ItchApiKey))
            {
                try
                {
                    string apiUrl = string.Format("https://itch.io//api/1/{0}/game/{1}", Settings.ItchApiKey, game.GameId);

                    WebClient client = new WebClient();

                    string myJSON = client.DownloadString(apiUrl);

                    ItchMetaData itchMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ItchMetaData>(myJSON);

                    return LinkHelper.AddLink(game, LinkName, itchMetaData.Game.Url, Settings);
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

        public LibraryLinkItch(LinkUtilitiesSettings settings)
        {
            LibraryId = Guid.Parse("00000001-ebb2-4eec-abcb-7c89937a42bb");
            LinkName = "Itch";
            LinkUrl = "";
            Settings = settings;
        }
    }
}
