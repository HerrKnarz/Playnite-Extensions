using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using KNARZhelper.Enum;

namespace MetadataUtilities.ViewModels
{
    public class AddNewObjectViewModel : ObservableObject
    {
        private readonly Settings _settings;
        private bool _enableTypeSelection = true;
        private MetadataObject _newObject;

        public AddNewObjectViewModel(Settings settings, MetadataObject newObject, bool enableTypeSelection = true)
        {
            _settings = settings;
            NewObject = newObject;
            EnableTypeSelection = enableTypeSelection;
            Prefixes.Add(string.Empty);
            Prefixes.AddMissing(settings.Prefixes);
        }

        public bool EnableTypeSelection
        {
            get => _enableTypeSelection;
            set => SetValue(ref _enableTypeSelection, value);
        }

        public Dictionary<FieldType, string> FieldValuePairs => FieldTypeHelper.ItemListFieldValues();

        public MetadataObject NewObject
        {
            get => _newObject;
            set => SetValue(ref _newObject, value);
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = NewObject.Name?.Length != 0;
            win.Close();
        }, win => win != null);

        public ObservableCollection<string> Prefixes { get; } = new ObservableCollection<string>();

        public Visibility PrefixVisibility => _settings.Prefixes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public static Window GetWindow(Settings settings, MetadataObject newObject, bool enableTypeSelection = true, string caption = "")
        {
            try
            {
                AddNewObjectViewModel viewModel = new AddNewObjectViewModel(settings, newObject, enableTypeSelection);

                AddNewObjectView newObjectViewView = new AddNewObjectView();

                if (caption == string.Empty)
                {
                    caption = ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddNewObject");
                }

                Window window = WindowHelper.CreateFixedDialog(caption);
                window.Content = newObjectViewView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing add new item dialog", true);

                return null;
            }
        }
    }
}