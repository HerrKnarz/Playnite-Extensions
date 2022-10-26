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
        public override Guid LibraryId { get; } = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        public override string LinkName { get; } = "GOG";

        public override bool AddLink(Game game)
        {
            if (!LinkHelper.LinkExists(game, LinkName))
            {
                try
                {
                    // To add a link to the gog page you have to get it from their API via the GameId.
                    WebClient client = new WebClient();

                    client.Headers.Add("Accept", "application/json");

                    string jsonResult = client.DownloadString("https://api.gog.com/v2/games/" + game.GameId);

                    GogMetaData gogMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<GogMetaData>(jsonResult);

                    LinkUrl = gogMetaData.Links.Store.Href;

                    return LinkHelper.AddLink(game, LinkName, LinkUrl, Settings);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error loading data from GOG");

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public LibraryLinkGog(LinkUtilitiesSettings settings) : base(settings)
        {
        }
    }
}
