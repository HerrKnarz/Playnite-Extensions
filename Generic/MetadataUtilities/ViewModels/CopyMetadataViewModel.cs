using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class CopyMetadataViewModel : ObservableObject
    {
        private ObservableCollection<CopyField> _fields = new ObservableCollection<CopyField>();

        public CopyMetadataViewModel()
        {
            foreach (var fieldType in FieldTypeHelper.GetAllTypes().Where(x => x.CanBeSetInGame).ToList())
            {
                Fields.Add(new CopyField(fieldType));
            }
        }

        public static Window GetWindow()
        {
            try
            {
                var viewModel = new CopyMetadataViewModel();

                var copyMetadataView = new CopyMetadataView();

                var window = WindowHelper.CreateSizeToContentWindow("Copy Metadata");

                window.Content = copyMetadataView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing copy metadata dialog", true);

                return null;
            }
        }

        public ObservableCollection<CopyField> Fields
        {
            get => _fields;
            set => SetValue(ref _fields, value);
        }
    }
}
