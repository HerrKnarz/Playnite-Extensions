using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class CopyMetadataViewModel : ObservableObject
    {
        private CopyDataSet _copyDataSet;

        public CopyMetadataViewModel(Game sourceGame)
        {
            _copyDataSet = new CopyDataSet(sourceGame);
        }

        public static Window GetWindow(Game sourceGame)
        {
            try
            {
                var viewModel = new CopyMetadataViewModel(sourceGame);

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

        public CopyDataSet CopyDataSet
        {
            get => _copyDataSet;
            set => SetValue(ref _copyDataSet, value);
        }
    }
}
