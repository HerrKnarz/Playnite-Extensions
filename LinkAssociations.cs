using LinkManager.Models;
using System;
using System.Net;
using Game = Playnite.SDK.Models.Game;

namespace LinkManager
{
    interface ILinkAssociation
    {
        Guid AssociationId { get; }
        string LinkName { get; set; }
        string LinkUrl { get; set; }
        LinkManagerSettings Settings { get; set; }

        bool AddLink(Game game);
    }

    class SteamLink : ILinkAssociation
    {
        public Guid AssociationId { get; set; }
        public string LinkName { get; set; }
        public string LinkUrl { get; set; }
        public LinkManagerSettings Settings { get; set; }

        public bool AddLink(Game game)
        {
            return LinkHelper.AddLink(game, LinkName, string.Format(LinkUrl, game.GameId));
        }

        public SteamLink(LinkManagerSettings settings)
        {
            AssociationId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
            LinkName = "Steam";
            LinkUrl = "https://store.steampowered.com/app/{0}/";
            Settings = settings;
        }
    }

    class GogLink : ILinkAssociation
    {
        public Guid AssociationId { get; set; }
        public string LinkName { get; set; }
        public string LinkUrl { get; set; }
        public LinkManagerSettings Settings { get; set; }

        public bool AddLink(Game game)
        {
            WebClient client = new WebClient();

            client.Headers.Add("Accept", "application/json");

            string myJSON = client.DownloadString("https://api.gog.com/v2/games/" + game.GameId);

            GogMetaData gogMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<GogMetaData>(myJSON);

            return LinkHelper.AddLink(game, LinkName, gogMetaData.Links.Store.Href);
        }

        public GogLink(LinkManagerSettings settings)
        {
            AssociationId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
            LinkName = "GOG";
            LinkUrl = "";
            Settings = settings;
        }
    }

    class ItchLink : ILinkAssociation
    {
        public Guid AssociationId { get; set; }
        public string LinkName { get; set; }
        public string LinkUrl { get; set; }
        public LinkManagerSettings Settings { get; set; }

        public bool AddLink(Game game)
        {
            if (!string.IsNullOrWhiteSpace(Settings.ItchApiKey))
            {
                string apiUrl = string.Format("https://itch.io//api/1/{0}/game/{1}", Settings.ItchApiKey, game.GameId);

                WebClient client = new WebClient();

                string myJSON = client.DownloadString(apiUrl);

                ItchMetaData itchMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ItchMetaData>(myJSON);

                return LinkHelper.AddLink(game, LinkName, itchMetaData.Game.Url);
            }
            else return false;
        }

        public ItchLink(LinkManagerSettings settings)
        {
            AssociationId = Guid.Parse("00000001-ebb2-4eec-abcb-7c89937a42bb");
            LinkName = "Itch";
            LinkUrl = "";
            Settings = settings;
        }
    }

    class Libraries : System.Collections.Generic.List<ILinkAssociation>
    {
        public Libraries(LinkManagerSettings settings)
        {
            Add(new SteamLink(settings));
            Add(new GogLink(settings));
            Add(new ItchLink(settings));
        }
    }

    internal class LinkAssociations
    {
        public static readonly Guid AmazonId = Guid.Parse("402674cd-4af6-4886-b6ec-0e695bfa0688");
        public static readonly Guid BattleNetId = Guid.Parse("e3c26a3d-d695-4cb7-a769-5ff7612c7edd");
        public static readonly Guid EpicId = Guid.Parse("00000002-dbd1-46c6-b5d0-b1ba559d10e4");
        public static readonly Guid HumbleId = Guid.Parse("96e8c4bc-ec5c-4c8b-87e7-18ee5a690626");
        public static readonly Guid IndiegalaId = Guid.Parse("f7da6eb0-17d7-497c-92fd-347050914954");
        public static readonly Guid LegacyGamesId = Guid.Parse("34c3178f-6e1d-4e27-8885-99d4f031b168");
        public static readonly Guid OriginId = Guid.Parse("85dd7072-2f20-4e76-a007-41035e390724");
        public static readonly Guid UbisoftConnectId = Guid.Parse("c2f038e5-8b92-4877-91f1-da9094155fc5");
        public static readonly Guid XboxId = Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287");
    }
}
