using Playnite.SDK;

namespace CompanyCompanion
{
    public class MergeCompaniesViewModel : ViewModelBase
    {
        private MergeCompanies mergeCompanies;
        private bool cleanUpNames = true;
        private bool findSimilar = true;
        public MergeCompanies MergeCompanies
        {
            get
            {
                return mergeCompanies;
            }
            set
            {
                mergeCompanies = value;
                OnPropertyChanged("MergeCompanies");
            }
        }
        public bool CleanUpNames
        {
            get
            {
                return cleanUpNames;
            }
            set
            {
                cleanUpNames = value;
                OnPropertyChanged("CleanUpNames");
            }
        }
        public bool FindSimilar
        {
            get
            {
                return findSimilar;
            }
            set
            {
                findSimilar = value;
                OnPropertyChanged("FindSimilar");
            }
        }
        public MergeCompaniesViewModel()
        {
            MergeCompanies = new MergeCompanies();
        }
        public RelayCommand FindCompaniesCommand
        {
            get => new RelayCommand(() =>
            {
                MergeCompanies.GetMergeList(cleanUpNames, findSimilar);
            });
        }
    }
}
