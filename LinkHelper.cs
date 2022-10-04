using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkManager
{
    internal class LinkHelper
    {
        public static bool AddLink(Game game, string linkName, string linkUrl)
        {
            Link link = new Link(linkName, linkUrl);
            bool mustUpdate = false;

            if (game.Links is null)
            {
                game.Links = new ObservableCollection<Link> { link };
                mustUpdate = true;
            }
            else
            {
                if (game.Links.Count(x => x.Name == linkName) == 0)
                {
                    game.Links.Add(link);
                    mustUpdate = true;
                }
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }
            return mustUpdate;
        }

        public static bool SortLinks(Game game)
        {
            if (game.Links != null && game.Links.Count > 0)
            {
                game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));

                API.Instance.Database.Games.Update(game);

                return true;
            }
            else return false;
        }
    }
}
