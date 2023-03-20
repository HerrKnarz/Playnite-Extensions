using Playnite.SDK;

namespace CompanyCompanion
{
    /// <summary>
    /// View model for the merge companies window
    /// </summary>
    public class MergeCompaniesViewModel : ViewModelBase
    {
        private MergeCompanies mergeCompanies;
        private bool cleanUpNames = true;
        private bool findSimilar = true;

        public CompanyCompanion Plugin { set => InitializeView(value); }

        public MergeCompanies MergeCompanies
        {
            get => mergeCompanies;
            set
            {
                mergeCompanies = value;
                OnPropertyChanged("MergeCompanies");
            }
        }

        public void InitializeView(CompanyCompanion plugin)
        {
            MergeCompanies = new MergeCompanies(plugin);
        }
        public bool CleanUpNames
        {
            get => cleanUpNames;
            set
            {
                cleanUpNames = value;
                OnPropertyChanged("CleanUpNames");
            }
        }
        public bool FindSimilar
        {
            get => findSimilar;
            set
            {
                findSimilar = value;
                OnPropertyChanged("FindSimilar");
            }
        }
        public MergeCompaniesViewModel()
        {
        }
        public RelayCommand FindCompaniesCommand
        {
            get => new RelayCommand(() => MergeCompanies.GetMergeList(cleanUpNames, findSimilar));
        }
    }
}
