using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    ///     List of all website Links that can be added.
    /// </summary>
    public class Links : List<BaseClasses.Linker>
    {
        public Links()
        {
            Add(new LinkAdventureGamers());
            Add(new LinkArcadeDatabase());
            Add(new LinkBackloggd());
            Add(new LinkCoOptimus());
            Add(new LinkEpic());
            Add(new LinkGamePressureGuides());
            Add(new LinkGamerGuides());
            Add(new LinkGGDeals());
            Add(new LinkGiantBomb());
            Add(new LibraryLinkGog());
            Add(new LinkGogDb());
            Add(new LinkGrouvee());
            Add(new LinkHg101());
            Add(new LinkHowLongToBeat());
            Add(new LinkIgn());
            Add(new LinkIgnGuides());
            Add(new LinkIsThereAnyDeal());
            Add(new LibraryLinkItch());
            Add(new LinkLemon64());
            Add(new LinkLemonAmiga());
            Add(new LinkMapGenie());
            Add(new LinkMetacritic());
            Add(new LinkMobyGames());
            Add(new LinkModDB());
            Add(new LinkNecRetro());
            Add(new LinkNintendoWiki());
            Add(new LinkPcGamingWiki());
            Add(new LinkProtonDb());
            Add(new LinkRawg());
            Add(new LinkSegaRetro());
            Add(new LinkSnkWiki());
            Add(new LibraryLinkSteam());
            Add(new LinkSteamDb());
            Add(new LinkStrategyWiki());
            Add(new LinkTvTropes());
            Add(new LinkWikipedia());
            Add(new LinkZopharsDomain());
        }
    }
}