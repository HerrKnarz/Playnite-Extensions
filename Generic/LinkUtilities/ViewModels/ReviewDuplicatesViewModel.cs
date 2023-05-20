using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities
{
    /// <summary>
    ///     View model for the Review Duplicates window
    /// </summary>
    public class ReviewDuplicatesViewModel : ViewModelBase
    {
        private IEnumerable<Game> _games;
        private ReviewDuplicates _reviewDuplicates;

        public ReviewDuplicatesViewModel(IEnumerable<Game> games)
        {
            _games = games;
            ReviewDuplicates = new ReviewDuplicates(_games);
        }

        public IEnumerable<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged("Games");
            }
        }

        public ReviewDuplicates ReviewDuplicates
        {
            get => _reviewDuplicates;
            set
            {
                _reviewDuplicates = value;
                OnPropertyChanged("ReviewDuplicates");
            }
        }

        public RelayCommand RefreshCommand
            => new RelayCommand(() => _reviewDuplicates.GetDuplicates());

        public RelayCommand<IList<object>> RemoveCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (LinkViewModel item in items.ToList().Cast<LinkViewModel>())
            {
                _reviewDuplicates.Remove(item);
            }
        }, items => items?.Any() ?? false);
    }
}