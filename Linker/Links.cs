using System.Collections.Generic;

namespace LinkUtilities.Linker
{
    /// <summary>
    /// List of all website links that can be added.
    /// </summary>
    public class Links : List<Link>
    {
        public Links(LinkUtilitiesSettings settings)
        {
            Add(new LinkMobyGames(settings));
        }
    }
}
