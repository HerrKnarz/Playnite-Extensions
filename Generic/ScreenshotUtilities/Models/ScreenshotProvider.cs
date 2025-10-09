using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    internal class ScreenshotProvider : ObservableObject
    {
        private Guid _id = Guid.NewGuid();
        private string _name;

        public ScreenshotProvider(string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _name = name;
        }

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
    }
}
