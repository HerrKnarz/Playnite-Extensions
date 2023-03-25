using System.ComponentModel;

namespace CompanyCompanion
{
    /// <summary>
    /// Base class for the view models
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }
}
