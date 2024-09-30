using LinkUtilities.BaseClasses;
using LinkUtilities.Helper;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.Models
{
    public class ReviewDuplicates : ObservableObject
    {
        private readonly IEnumerable<Game> _games;
        private ObservableCollectionFast<GameLink> _duplicates = new ObservableCollectionFast<GameLink>();

        public ReviewDuplicates(IEnumerable<Game> games)
        {
            _games = games;
            GetDuplicates();
        }

        public ObservableCollectionFast<GameLink> Duplicates
        {
            get => _duplicates;
            set => SetValue(ref _duplicates, value);
        }

        public void GetDuplicates()
        {
            Duplicates.Clear();
            _games.Aggregate(false, (current, game) => current | GetDuplicates(game));
        }

        public void Remove(GameLink gameLink)
        {
            gameLink.Remove();

            Duplicates.Remove(gameLink);
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

            Duplicates.AddRange(newLinks.Select(x => new GameLink { Game = game, Link = x }));

            return true;
        }
    }
}