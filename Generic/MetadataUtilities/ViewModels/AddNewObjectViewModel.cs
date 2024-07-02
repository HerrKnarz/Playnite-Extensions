using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;

namespace MetadataUtilities.ViewModels
{
    public class AddNewObjectViewModel : ObservableObject
    {
        private bool _enableTypeSelection = true;
        private MetadataObject _newObject;
        private MetadataUtilities _plugin;

        public AddNewObjectViewModel(MetadataUtilities plugin, MetadataObject newObject, bool enableTypeSelection = true)
        {
            Plugin = plugin;
            NewObject = newObject;
            EnableTypeSelection = enableTypeSelection;
            Prefixes.Add(string.Empty);
            Prefixes.AddMissing(Plugin.Settings.Settings.Prefixes);
        }

        public bool EnableTypeSelection
        {
            get => _enableTypeSelection;
            set => SetValue(ref _enableTypeSelection, value);
        }

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

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public ObservableCollection<string> Prefixes { get; } = new ObservableCollection<string>();

        public Visibility PrefixVisibility => _plugin.Settings.Settings.Prefixes?.Any() ?? false ? Visibility.Visible : Visibility.Collapsed;

        public static Window GetWindow(MetadataUtilities plugin, MetadataObject newObject, bool enableTypeSelection = true)
        {
            try
            {
                AddNewObjectViewModel viewModel = new AddNewObjectViewModel(plugin, newObject, enableTypeSelection);

                AddNewObjectView newObjectViewView = new AddNewObjectView();

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddNewObject"));
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