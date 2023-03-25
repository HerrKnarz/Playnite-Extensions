using LinkUtilities.BaseClasses;
using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all website Links that can be added.
    /// </summary>
    internal class Links : List<Link>
    {
        public Links()
        {
            Add(new LinkAdventureGamers());
            Add(new LinkArcadeDatabase());
            Add(new LinkCoOptimus());
            Add(new LinkEpic());
            Add(new LinkGamerGuides());
            Add(new LinkGiantBomb());
            Add(new LibraryLinkGog());
            Add(new LinkHG101());
            Add(new LinkHowLongToBeat());
            Add(new LinkIGN());
            Add(new LinkIsThereAnyDeal());
            Add(new LibraryLinkItch());
            Add(new LinkLemonAmiga());
            Add(new LinkMapGenie());
            Add(new LinkMetacritic());
            Add(new LinkMobyGames());
            Add(new LinkNECRetro());
            Add(new LinkNintendoWiki());
            Add(new LinkPCGamingWiki());
            Add(new LinkRAWG());
            Add(new LinkSegaRetro());
            Add(new LinkSNKWiki());
            Add(new LibraryLinkSteam());
            Add(new LinkStrategyWiki());
            Add(new LinkWikipedia());
            Add(new LinkZopharsDomain());
        }
    }
}
