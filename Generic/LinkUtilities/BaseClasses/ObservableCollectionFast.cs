using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LinkUtilities.BaseClasses
{
    //TODO: Replace with ObservableRangeCollection in Playnite 11!

    public class ObservableCollectionFast<T> : ObservableCollection<T>
    {
        public ObservableCollectionFast()
        { }

        public ObservableCollectionFast(IEnumerable<T> collection) : base(collection) { }

        public ObservableCollectionFast(List<T> list) : base(list) { }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            Items.Clear();

            AddRange(range);
        }
    }
}