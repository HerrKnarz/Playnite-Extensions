using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace ScreenshotUtilities.Models
{
    internal class Screenshot : ObservableObject
    {
        private string _description;
        private Guid _id = Guid.NewGuid();
        private bool _isDownloaded = false;
        private string _name;
        private string _path;
        private int _sortOrder = 0;

        public Screenshot(string path = "", string name = "", Guid id = default)
        {
            _id = id == default ? _id : id;
            _path = path;
            _name = name == string.Empty ? System.IO.Path.GetFileNameWithoutExtension(path) : name;
        }

        [SerializationPropertyName("description")]
        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        [SerializationPropertyName("id")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        [SerializationPropertyName("isDownloaded")]
        public bool IsDownloaded
        {
            get => _isDownloaded;
            set => SetValue(ref _isDownloaded, value);
        }

        [SerializationPropertyName("name")]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [SerializationPropertyName("path")]
        public string Path
        {
            get => _path;
            set => SetValue(ref _path, value);
        }

        [SerializationPropertyName("sortOrder")]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value);
        }
    }
}
