using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class AddNewObjectViewModel : ObservableObject
    {
        //TODO: Try to add localization to combobox!

        private MetadataObject _newObject;
        private MetadataUtilities _plugin;

        public AddNewObjectViewModel(MetadataUtilities plugin, MetadataObject newObject)
        {
            Plugin = plugin;
            NewObject = newObject;
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set => SetValue(ref _plugin, value);
        }

        public MetadataObject NewObject
        {
            get => _newObject;
            set => SetValue(ref _newObject, value);
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = NewObject.Name?.Any() ?? false;
            win.Close();
        }, win => win != null);

        public static Window GetWindow(MetadataUtilities plugin, MetadataObject newObject)
        {
            try
            {
                AddNewObjectViewModel viewModel = new AddNewObjectViewModel(plugin, newObject);

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