using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace MetadataUtilities
{
    public class MergeDialogViewModel : ObservableObject
    {
        private MetadataListObject _mergeTarget;
        private MetadataListObjects _metadataListObjects;
        private MetadataUtilities _plugin;
        private bool _saveAsRule;

        public MergeDialogViewModel(MetadataUtilities plugin, MetadataListObjects items)
        {
            Plugin = plugin;
            MetadataListObjects = items;
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

        public MetadataListObjects MetadataListObjects
        {
            get => _metadataListObjects;
            set
            {
                SetValue(ref _metadataListObjects, value);
                MergeTarget = _metadataListObjects.FirstOrDefault();
            }
        }

        public MetadataListObject MergeTarget
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
            Cursor.Current = Cursors.WaitCursor;
            try
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
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            win.DialogResult = true;
            win.Close();
        }, win => win != null);
    }
}