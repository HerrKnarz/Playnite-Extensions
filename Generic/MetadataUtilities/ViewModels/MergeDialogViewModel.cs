using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetadataUtilities.ViewModels
{
    public class MergeDialogViewModel : ObservableObject
    {
        private MetadataObject _mergeTarget;
        private MetadataObjects _metadataObjects;
        private bool _saveAsRule;

        public MergeDialogViewModel(MetadataObjects items)
        {
            MetadataObjects = items;
            SaveAsRule = ControlCenter.Instance.Settings.AlwaysSaveManualMergeRules;
        }

        public MetadataObject MergeTarget
        {
            get => _mergeTarget;
            set => SetValue(ref _mergeTarget, value);
        }

        public MetadataObjects MetadataObjects
        {
            get => _metadataObjects;
            set
            {
                SetValue(ref _metadataObjects, value);
                MergeTarget = _metadataObjects.FirstOrDefault();
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            ControlCenter.Instance.MergeMetadataObjects(_mergeTarget, _metadataObjects, SaveAsRule);

            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public bool SaveAsRule
        {
            get => _saveAsRule;
            set => SetValue(ref _saveAsRule, value);
        }
    }
}