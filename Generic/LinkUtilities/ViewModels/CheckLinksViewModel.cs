using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LinkUtilities.ViewModels
{
    internal class CheckLinksViewModel : ObservableObject
    {
        private CheckLinks _checkLinks;

        public CheckLinksViewModel(List<Game> games, bool hideOkOnLinkCheck) => CheckLinks = new CheckLinks(games, hideOkOnLinkCheck);

        public CheckLinks CheckLinks
        {
            get => _checkLinks;
            set => SetValue(ref _checkLinks, value);
        }

        public RelayCommand FilterCommand
            => new RelayCommand(() => CheckLinks.FilterLinks());

        public RelayCommand HelpCommand => new RelayCommand(() =>
            Process.Start(new ProcessStartInfo(
                "https://knarzwerk.de/en/playnite-extensions/link-utilities/check-links/")));

        public RelayCommand<IList<object>> RemoveCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<CheckGameLink>())
            {
                CheckLinks.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> ReplaceCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<CheckGameLink>())
            {
                CheckLinks.Replace(item);
            }
        }, items => items?.Any() ?? false);
    }
}