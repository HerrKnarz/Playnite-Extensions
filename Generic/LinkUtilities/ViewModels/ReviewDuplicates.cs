using LinkUtilities.BaseClasses;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities
{
    public class ReviewDuplicates : ViewModelBase
    {
        private readonly IEnumerable<Game> _games;
        private ObservableCollectionFast<LinkViewModel> _duplicates = new ObservableCollectionFast<LinkViewModel>();

        public ReviewDuplicates(IEnumerable<Game> games)
        {
            _games = games;
            GetDuplicates();
        }

        public ObservableCollectionFast<LinkViewModel> Duplicates
        {
            get => _duplicates;
            set
            {
                _duplicates = value;
                OnPropertyChanged("Duplicates");
            }
        }

        public void GetDuplicates()
        {
            Duplicates.Clear();
            _games.Aggregate(false, (current, game) => current | GetDuplicates(game));
        }

        private bool GetDuplicates(Game game)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            ObservableCollection<Link> newLinks =
                game.Links.GroupBy(x => x.Name).Where(x => x.Count() > 1).SelectMany(x => x).ToObservable();

            newLinks.AddMissing(game.Links?.GroupBy(x => LinkHelper.CleanUpUrl(x.Url)).Where(x => x.Count() > 1).SelectMany(x => x));

            if (!newLinks.Any())
            {
                return false;
            }

            Duplicates.AddRange(newLinks.Select(x => new LinkViewModel { Game = game, Link = x }));

            return true;
        }

        public void Remove(LinkViewModel linkViewModel)
        {
            linkViewModel.Remove();

            Duplicates.Remove(linkViewModel);
        }
    }
}