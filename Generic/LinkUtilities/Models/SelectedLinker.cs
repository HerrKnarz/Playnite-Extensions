using System.Collections.Generic;

namespace LinkUtilities.Models
{
    internal class SelectedLinker : ObservableObject
    {
        private bool _selected;

        public SelectedLinker(BaseClasses.Linker linker, bool add = true)
        {
            Linker = linker;
            Selected = add ? linker.Settings.IsAddable == true : linker.Settings.IsSearchable == true;
        }

        public BaseClasses.Linker Linker { get; }

        public bool Selected
        {
            get => _selected;
            set => SetValue(ref _selected, value);
        }
    }
}