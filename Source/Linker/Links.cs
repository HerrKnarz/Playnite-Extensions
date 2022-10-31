using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all website Links that can be added.
    /// </summary>
    public class Links : List<Link>
    {
        public Links(LinkUtilitiesSettings settings)
        {
            Add(new LibraryLinkGog(settings));
            Add(new LinkHG101(settings));
            if (!string.IsNullOrWhiteSpace(settings.ItchApiKey))
            {
                Add(new LibraryLinkItch(settings));
            }
            Add(new LinkMetacritic(settings));
            Add(new LinkMobyGames(settings));
            Add(new LinkPCGamingWiki(settings));
            Add(new LibraryLinkSteam(settings));
            Add(new LinkWikipedia(settings));
        }
    }
}
