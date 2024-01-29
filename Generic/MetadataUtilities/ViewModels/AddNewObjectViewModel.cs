using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class AddNewObjectViewModel : ViewModelBase
    {
        private MetadataListObject _newObject = new MetadataListObject();
        private MetadataUtilities _plugin;

        public Dictionary<FieldType, string> FieldTypeList { get; } = new Dictionary<FieldType, string>();


        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                InitializeView();
                OnPropertyChanged("Plugin");
            }
        }

        public MetadataListObject NewObject
        {
            get => _newObject;
            set
            {
                _newObject = value;
                OnPropertyChanged("NewObject");
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public RelayCommand<Window> CancelCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = false;
            win.Close();
        }, win => win != null);

        private void InitializeView()
        {
            foreach (FieldType type in Enum.GetValues(typeof(FieldType))
                .Cast<FieldType>())
            {
                FieldTypeList.Add(type, type.GetEnumDisplayName("MetadataUtilities"));
            }
        }
    }
}