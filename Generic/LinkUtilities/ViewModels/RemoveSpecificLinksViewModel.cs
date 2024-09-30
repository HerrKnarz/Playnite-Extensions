using KNARZhelper;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LinkUtilities.ViewModels
{
    internal class RemoveSpecificLinksViewModel : ObservableObject
    {
        private readonly List<Game> _games;
        private readonly LinkUtilities _plugin;
        private ObservableCollection<SelectedLink> _links = new ObservableCollection<SelectedLink>();

        public RemoveSpecificLinksViewModel(List<Game> games, LinkUtilities plugin)
        {
            _games = games;
            _plugin = plugin;

            AssembleLinks();
        }

        public ObservableCollection<SelectedLink> Links
        {
            get => _links;
            set => SetValue(ref _links, value);
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            if (Links.Any(l => l.Selected))
            {
                RemoveSpecificLinks.Instance().Links = Links.ToList();
                _plugin.DoForAll(_games, RemoveSpecificLinks.Instance(), true);
                RemoveSpecificLinks.Instance().Links.Clear();
            }

            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public static Window GetWindow(List<Game> games, LinkUtilities plugin)
        {
            try
            {
                var viewModel = new RemoveSpecificLinksViewModel(games, plugin);

                var view = new RemoveSpecificLinksView();

                var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesMenuRemoveSpecificLinks"), 100, 100);

                window.Content = view;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Remove Specific Links dialog", true);

                return null;
            }
        }

        private void AssembleLinks()
        {
            var linkNames = new List<string>();

            foreach (var game in _games)
            {
                linkNames.AddRange(game.Links.Select(l => l.Name));
            }

            Links.AddMissing(linkNames.Distinct().Select(l => new SelectedLink() { Name = l }).OrderBy(l => l.Name));
        }
    }
}
