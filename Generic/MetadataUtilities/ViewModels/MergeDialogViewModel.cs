using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class MergeDialogViewModel : ObservableObject
    {
        private MetadataObject _mergeTarget;
        private MetadataObjects _metadataObjects;
        private MetadataUtilities _plugin;
        private bool _saveAsRule;

        public MergeDialogViewModel(MetadataUtilities plugin, MetadataObjects items)
        {
            Plugin = plugin;
            MetadataObjects = items;
        }

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                SetValue(ref _plugin, value);
                SaveAsRule = _plugin.Settings.Settings.AlwaysSaveManualMergeRules;
            }
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

        public MetadataObject MergeTarget
        {
            get => _mergeTarget;
            set => SetValue(ref _mergeTarget, value);
        }

        public bool SaveAsRule
        {
            get => _saveAsRule;
            set => SetValue(ref _saveAsRule, value);
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            MergeRule rule = new MergeRule(_plugin.Settings.Settings)
            {
                Name = _mergeTarget.Name,
                Type = _mergeTarget.Type,
                Id = _mergeTarget.Id,
                SourceObjects = _metadataObjects
            };

            if (SaveAsRule)
            {
                _plugin.Settings.Settings.MergeRules.AddRule(rule);
                _plugin.SavePluginSettings(_plugin.Settings.Settings);
            }

            Plugin.MergeItems(null, rule);

            win.DialogResult = true;
            win.Close();
        }, win => win != null);
    }
}