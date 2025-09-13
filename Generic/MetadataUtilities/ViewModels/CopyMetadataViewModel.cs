using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
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

                var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCMetadataUtilitiesMenuCopyMetadata"), 300);

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

        public RelayCommand CheckAllCommand => new RelayCommand(() =>
        {
            foreach (var item in _copyDataSet.Fields)
            {
                item.CopyData = true;
            }
        });

        public RelayCommand UnCheckAllCommand => new RelayCommand(() =>
        {
            foreach (var item in _copyDataSet.Fields)
            {
                item.CopyData = false;
            }
        });

        public RelayCommand CheckAllReplaceCommand => new RelayCommand(() =>
        {
            foreach (var item in _copyDataSet.Fields)
            {
                item.ReplaceData = true;
            }
        });

        public RelayCommand UnCheckAllReplaceCommand => new RelayCommand(() =>
        {
            foreach (var item in _copyDataSet.Fields)
            {
                item.ReplaceData = false;
            }
        });

        public RelayCommand CheckAllEmptyCommand => new RelayCommand(() =>
        {
            foreach (var item in _copyDataSet.Fields)
            {
                item.OnlyIfEmpty = true;
            }
        });

        public RelayCommand UnCheckAllEmptyCommand => new RelayCommand(() =>
        {
            foreach (var item in _copyDataSet.Fields)
            {
                item.OnlyIfEmpty = false;
            }
        });

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            ControlCenter.Instance.GameToCopy = _copyDataSet;

            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public CopyDataSet CopyDataSet
        {
            get => _copyDataSet;
            set => SetValue(ref _copyDataSet, value);
        }
    }
}
