using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.ViewModels
{
    internal class CheckLinksViewModel : ViewModelBase
    {
        private CheckLinks _checkLinks;
        private List<Game> _games;

        public CheckLinksViewModel(List<Game> games, bool hideOkOnLinkCheck)
        {
            _games = games;
            CheckLinks = new CheckLinks(_games, hideOkOnLinkCheck);
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

        public RelayCommand FilterCommand
            => new RelayCommand(() => CheckLinks.FilterLinks());

        public List<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged("Games");
            }
        }

        public RelayCommand<IList<object>> RemoveCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<CheckLink>())
            {
                _checkLinks.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> ReplaceCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<CheckLink>())
            {
                _checkLinks.Replace(item);
            }
        }, items => items?.Any() ?? false);
    }
}