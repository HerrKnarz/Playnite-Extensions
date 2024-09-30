using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Models
{
    public class ReviewDuplicates : ObservableCollectionFast<GameLink>
    {
        private readonly IEnumerable<Game> _games;

        public ReviewDuplicates(IEnumerable<Game> games)
        {
            _games = games;
            GetDuplicates();
        }

        public void GetDuplicates()
        {
            Clear();
            _games.Aggregate(false, (current, game) => current | GetDuplicates(game));
        }

        public new void Remove(GameLink gameLink)
        {
            gameLink.Remove();

            base.Remove(gameLink);
        }

        private bool GetDuplicates(Game game)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            var newLinks =
                game.Links.GroupBy(x => x.Name).Where(x => x.Count() > 1).SelectMany(x => x).ToObservable();

            newLinks.AddMissing(game.Links?.GroupBy(x => LinkHelper.CleanUpUrl(x.Url)).Where(x => x.Count() > 1).SelectMany(x => x));

            if (!newLinks.Any())
            {
                return false;
            }

            AddRange(newLinks.Select(x => new GameLink { Game = game, Link = x }));

            return true;
        }
    }
}