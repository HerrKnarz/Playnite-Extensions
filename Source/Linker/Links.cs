using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all website Links that can be added.
    /// </summary>
    public class Links : List<Link>
    {
        public Links(LinkUtilities plugin)
        {
            Add(new LinkArcadeDatabase(plugin));
            Add(new LinkCoOptimus(plugin));
            Add(new LinkEpic(plugin));
            Add(new LinkGamerGuides(plugin));
            Add(new LinkGiantBomb(plugin));
            Add(new LibraryLinkGog(plugin));
            Add(new LinkHG101(plugin));
            Add(new LinkIGN(plugin));
            Add(new LinkIsThereAnyDeal(plugin));
            Add(new LibraryLinkItch(plugin));
            Add(new LinkLemonAmiga(plugin));
            Add(new LinkMetacritic(plugin));
            Add(new LinkMobyGames(plugin));
            Add(new LinkNECRetro(plugin));
            Add(new LinkNintendoWiki(plugin));
            Add(new LinkPCGamingWiki(plugin));
            Add(new LinkRAWG(plugin));
            Add(new LinkSegaRetro(plugin));
            Add(new LinkSNKWiki(plugin));
            Add(new LibraryLinkSteam(plugin));
            Add(new LinkStrategyWiki(plugin));
            Add(new LinkWikipedia(plugin));
            Add(new LinkZopharsDomain(plugin));
        }
    }
}
