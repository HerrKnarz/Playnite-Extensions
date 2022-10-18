using LinkUtilities.Models.Gog;
using Playnite.SDK.Models;
using System;
using System.Net;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// Adds a link to the gog page of the game, if it is part of the steam library.
    /// </summary>
    class LibraryLinkGog : LibraryLink
    {
        public override bool AddLink(Game game)
        {
            try
            {
                // To add a link to the gog page you have to get it from their API via the GameId.
                WebClient client = new WebClient();

                client.Headers.Add("Accept", "application/json");

                string myJSON = client.DownloadString("https://api.gog.com/v2/games/" + game.GameId);

                GogMetaData gogMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<GogMetaData>(myJSON);

                return LinkHelper.AddLink(game, LinkName, gogMetaData.Links.Store.Href, Settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading data from GOG");

                return false;
            }
        }

        public LibraryLinkGog(LinkUtilitiesSettings settings)
        {
            LibraryId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
            LinkName = "GOG";
            LinkUrl = "";
            Settings = settings;
        }
    }
}
