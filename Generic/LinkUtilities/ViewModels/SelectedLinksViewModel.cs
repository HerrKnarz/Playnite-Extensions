using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities.ViewModels
{
    internal class SelectedLinksViewModel : ObservableObject
    {
        private ObservableCollection<SelectedLinker> _links = new ObservableCollection<SelectedLinker>();

        public SelectedLinksViewModel(Links links, bool add = true)
        {
            foreach (var linker in links.Where(linker => add ? linker.Settings.IsAddable != null : linker.Settings.IsSearchable != null).OrderBy(x => x.LinkName))
            {
                Links.Add(new SelectedLinker(linker, add));
            }
        }

        public RelayCommand<object> CheckAllCommand => new RelayCommand<object>(a =>
        {
            foreach (var link in Links)
            {
                link.Selected = true;
            }
        });

        public ObservableCollection<SelectedLinker> Links
        {
            get => _links;
            set => SetValue(ref _links, value);
        }

        public RelayCommand<object> ReverseCheckCommand => new RelayCommand<object>(a =>
        {
            foreach (var link in Links)
            {
                link.Selected = !link.Selected;
            }
        });

        public RelayCommand<object> UncheckAllCommand => new RelayCommand<object>(a =>
        {
            foreach (var link in Links)
            {
                link.Selected = false;
            }
        });
    }
}