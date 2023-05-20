using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities
{
    internal class CheckLinksViewModel : ViewModelBase
    {
        private CheckLinks _checkLinks;
        private List<Game> _games;

        public CheckLinksViewModel(List<Game> games)
        {
            _games = games;
            CheckLinks = new CheckLinks(_games);
        }

        public List<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged("Games");
            }
        }

        public CheckLinks CheckLinks
        {
            get => _checkLinks;
            set
            {
                _checkLinks = value;
                OnPropertyChanged("CheckLinks");
            }
        }

        public RelayCommand<IList<object>> RemoveCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (CheckedLink item in items.ToList().Cast<CheckedLink>())
            {
                _checkLinks.Remove(item);
            }
        }, items => items?.Any() ?? false);
    }
}