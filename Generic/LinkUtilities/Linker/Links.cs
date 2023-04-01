using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all website Links that can be added.
    /// </summary>
    internal class Links : List<ILinker>
    {
        public Links()
        {
            Add(new LinkAdventureGamers());
            Add(new LinkArcadeDatabase());
            Add(new LinkCoOptimus());
            Add(new LinkEpic());
            Add(new LinkGamePressureGuides());
            Add(new LinkGamerGuides());
            Add(new LinkGiantBomb());
            Add(new LibraryLinkGog());
            Add(new LinkHg101());
            Add(new LinkHowLongToBeat());
            Add(new LinkIgn());
            Add(new LinkIgnGuides());
            Add(new LinkIsThereAnyDeal());
            Add(new LibraryLinkItch());
            Add(new LinkLemonAmiga());
            Add(new LinkMapGenie());
            Add(new LinkMetacritic());
            Add(new LinkMobyGames());
            Add(new LinkNecRetro());
            Add(new LinkNintendoWiki());
            Add(new LinkPcGamingWiki());
            Add(new LinkRawg());
            Add(new LinkSegaRetro());
            Add(new LinkSnkWiki());
            Add(new LibraryLinkSteam());
            Add(new LinkStrategyWiki());
            Add(new LinkWikipedia());
            Add(new LinkZopharsDomain());
        }
    }
}