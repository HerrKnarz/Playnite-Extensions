using LinkUtilities.Linker;
using Playnite.SDK;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities
{
    internal class SelectedLinksViewModel : ViewModelBase
    {
        private ObservableCollection<SelectedLink> _links = new ObservableCollection<SelectedLink>();

        public SelectedLinksViewModel(Links links, bool add = true)
        {
            foreach (BaseClasses.Linker linker in links.Where(linker => add ? linker.Settings.IsAddable != null : linker.Settings.IsSearchable != null))
            {
                Links.Add(new SelectedLink(linker, add));
            }
        }

        public ObservableCollection<SelectedLink> Links
        {
            get => _links;
            set
            {
                _links = value;
                OnPropertyChanged("Links");
            }
        }
    }
}