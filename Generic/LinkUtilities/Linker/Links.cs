using LinkUtilities.Linker.Libraries;
using LinkUtilities.Linker.LinkSources;
using LinkUtilities.Models;
using System.Collections.Generic;
using System.Linq;

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
            Add(new LinkGgDeals());
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
            Add(new LinkModDb());
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

        public void RefreshCustomLinkProfiles(List<CustomLinkProfile> customLinkProfiles)
        {
            var linksToRemove = this.Where(x => x.Settings.IsCustomSource).ToList();

            foreach (var linker in linksToRemove)
            {
                Remove(linker);
            }

            foreach (var customLinkProfile in customLinkProfiles)
            {
                Add(new CustomLinker(customLinkProfile));
            }
        }
    }
}