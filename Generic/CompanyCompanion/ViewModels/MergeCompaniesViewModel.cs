using Playnite.SDK;

namespace CompanyCompanion
{
    /// <summary>
    /// View model for the _merge companies window
    /// </summary>
    public class MergeCompaniesViewModel : ViewModelBase
    {
        private MergeCompanies _mergeCompanies;
        private bool _cleanUpNames = true;
        private bool _findSimilar = true;

        public CompanyCompanion Plugin
        {
            set => InitializeView(value);
        }

        public MergeCompanies MergeCompanies
        {
            get => _mergeCompanies;
            set
            {
                _mergeCompanies = value;
                OnPropertyChanged("MergeCompanies");
            }
        }

        private void InitializeView(CompanyCompanion plugin) => MergeCompanies = new MergeCompanies(plugin);

        public bool CleanUpNames
        {
            get => _cleanUpNames;
            set
            {
                _cleanUpNames = value;
                OnPropertyChanged("CleanUpNames");
            }
        }

        public bool FindSimilar
        {
            get => _findSimilar;
            set
            {
                _findSimilar = value;
                OnPropertyChanged("FindSimilar");
            }
        }

        public RelayCommand FindCompaniesCommand => new RelayCommand(() => MergeCompanies.GetMergeList(_cleanUpNames, _findSimilar));
    }
}