using MetadataUtilities.Models;
using Playnite.SDK;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class MergeDialogViewModel : ViewModelBase
    {
        private MetadataListObject _mergeTarget;
        private MetadataListObjects _metadataListObjects;
        private MetadataUtilities _plugin;
        private bool _saveAsRule;

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                SaveAsRule = _plugin.Settings.Settings.AlwaysSaveManualMergeRules;
                OnPropertyChanged("Plugin");
            }
        }

        public MetadataListObjects MetadataListObjects
        {
            get => _metadataListObjects;
            set
            {
                _metadataListObjects = value;

                MergeTarget = _metadataListObjects.FirstOrDefault();

                OnPropertyChanged("MetadataListObjects");
            }
        }

        public MetadataListObject MergeTarget
        {
            get => _mergeTarget;
            set
            {
                _mergeTarget = value;
                OnPropertyChanged("MergeTarget");
            }
        }

        public bool SaveAsRule
        {
            get => _saveAsRule;
            set
            {
                _saveAsRule = value;
                OnPropertyChanged("SaveAsRule");
            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            if (SaveAsRule)
            {
                MergeRule rule = new MergeRule
                {
                    Name = _mergeTarget.Name,
                    Type = _mergeTarget.Type,
                    Id = _mergeTarget.Id,
                    SourceObjects = _metadataListObjects
                };

                _plugin.Settings.Settings.MergeRules.AddRule(rule);
            }

            _plugin.SavePluginSettings(_plugin.Settings.Settings);

            MetadataListObjects.MergeItems(MergeTarget.Type, MergeTarget.Id);
            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public RelayCommand<Window> CancelCommand => new RelayCommand<Window>(win =>
        {
            win.DialogResult = false;
            win.Close();
        }, win => win != null);
    }
}