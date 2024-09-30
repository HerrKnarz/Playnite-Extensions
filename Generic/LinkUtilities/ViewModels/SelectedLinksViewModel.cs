﻿using LinkUtilities.Linker;
using Playnite.SDK;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities.ViewModels
{
    internal class SelectedLinksViewModel : ViewModelBase
    {
        private ObservableCollection<SelectedLink> _links = new ObservableCollection<SelectedLink>();

        public SelectedLinksViewModel(Links links, bool add = true)
        {
            foreach (var linker in links.Where(linker => add ? linker.Settings.IsAddable != null : linker.Settings.IsSearchable != null).OrderBy(x => x.LinkName))
            {
                Links.Add(new SelectedLink(linker, add));
            }
        }

        public RelayCommand<object> CheckAllCommand => new RelayCommand<object>(a =>
        {
            foreach (var link in Links)
            {
                link.Selected = true;
            }
        });

        public ObservableCollection<SelectedLink> Links
        {
            get => _links;
            set
            {
                _links = value;
                OnPropertyChanged("Links");
            }
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