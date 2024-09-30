namespace LinkUtilities.ViewModels
{
    internal class SelectedLink : ViewModelBase
    {
        private bool _selected;

        public SelectedLink(BaseClasses.Linker linker, bool add = true)
        {
            Linker = linker;
            _selected = add ? linker.Settings.IsAddable == true : linker.Settings.IsSearchable == true;
        }

        public BaseClasses.Linker Linker { get; }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }
    }
}