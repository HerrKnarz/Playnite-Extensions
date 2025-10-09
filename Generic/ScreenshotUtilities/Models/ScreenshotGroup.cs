using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    internal class ScreenshotGroup : ObservableObject
    {
        private string _description;
        private Guid _id = Guid.NewGuid();
        private string _name;
        private ScreenshotProvider _provider;
        private RangeObservableCollection<Screenshot> _screenshots = new RangeObservableCollection<Screenshot>();
        private Screenshot _selectedScreenshot;
        private int _sortOrder = 0;

        public ScreenshotGroup(string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _name = name;
        }

        [SerializationPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        [DontSerialize]
        public string DisplayName => Provider == null || string.IsNullOrEmpty(Provider.Name) ? Name
            : string.IsNullOrEmpty(Name) ? (Provider?.Name)
            : $"{Provider?.Name}: {Name}";

        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [SerializationPropertyName("provider")]
        public ScreenshotProvider Provider
        {
            get => _provider;
            set => SetValue(ref _provider, value);
        }

        [SerializationPropertyName("screenshots")]
        public RangeObservableCollection<Screenshot> Screenshots
        {
            get => _screenshots;
            set
            {
                SetValue(ref _screenshots, value);

                SelectedScreenshot = value != null && value.Count > 0 ? value[0] : null;
            }
        }

        [DontSerialize]
        public Screenshot SelectedScreenshot
        {
            get => _selectedScreenshot;
            set => SetValue(ref _selectedScreenshot, value);
        }

        [SerializationPropertyName("sortOrder")]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }
    }
}
