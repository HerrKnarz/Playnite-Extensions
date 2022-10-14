using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities
{
    /// <summary>
    /// Helper class containing functions used in the link utilities extension
    /// </summary>
    internal class LinkHelper
    {
        /// <summary>
        /// Adds a link to the specified URL to a game.
        /// </summary>
        /// <param name="game">Game the link will be added to</param>
        /// <param name="linkName">Name of the link</param>
        /// <param name="linkUrl">URL of the link</param>
        /// <returns>
        /// True, if a link could be added. Returns false, if a link with that name was already present or couldn't be added.
        /// </returns>
        public static bool AddLink(Game game, string linkName, string linkUrl)
        {
            Link link = new Link(linkName, linkUrl);
            bool mustUpdate = false;

            // If the game doesn't have any links yet, we have to add the collection itself.
            if (game.Links is null)
            {
                game.Links = new ObservableCollection<Link> { link };
                mustUpdate = true;
            }
            // otherwise we'll check if a link with the specified name is already present. If not, we'll add the link and return true.
            else
            {
                if (game.Links.Count(x => x.Name == linkName) == 0)
                {
                    game.Links.Add(link);
                    mustUpdate = true;
                }
            }

            // Updates the game in the database if we added a new link.
            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        /// <summary>
        /// Sorts the links of a game alphabetically by the link name.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool SortLinks(Game game)
        {
            if (game.Links != null && game.Links.Count > 0)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));

                API.Instance.Database.Games.Update(game);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
