using LinkUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.ViewModels
{
    /// <summary>
    ///     View model for the Review Duplicates window
    /// </summary>
    public class ReviewDuplicatesViewModel : ObservableObject
    {
        private List<Game> _games;
        private ReviewDuplicates _reviewDuplicates;

        public ReviewDuplicatesViewModel(List<Game> games)
        {
            _games = games;
            ReviewDuplicates = new ReviewDuplicates(_games);
        }

        public List<Game> Games
        {
            get => _games;
            set => SetValue(ref _games, value);
        }

        public RelayCommand RefreshCommand
            => new RelayCommand(() => ReviewDuplicates.GetDuplicates());

        public RelayCommand<IList<object>> RemoveCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<GameLink>())
            {
                ReviewDuplicates.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public ReviewDuplicates ReviewDuplicates
        {
            get => _reviewDuplicates;
            set => SetValue(ref _reviewDuplicates, value);
        }
    }
}