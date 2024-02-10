using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;

namespace MetadataUtilities
{
    public class ChangeTypeViewModel : ObservableObject
    {
        private readonly MetadataListObjects _metadataListObjects;

        private MetadataListObjects _newObjects;

        private FieldType _newType = FieldType.Category;
        private MetadataUtilities _plugin;
        private bool _saveAsRule;

        public ChangeTypeViewModel(MetadataUtilities plugin, MetadataListObjects items)
        {
            Plugin = plugin;
            _metadataListObjects = items;
            _newObjects = new MetadataListObjects(plugin.Settings.Settings);
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

        public MetadataListObjects NewObjects
        {
            get => _newObjects;
            set => SetValue(ref _newObjects, value);
        }

        public FieldType NewType
        {
            get => _newType;
            set => SetValue(ref _newType, value);
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
                foreach (MetadataListObject item in _metadataListObjects)
                {
                    if (item.Type == NewType)
                    {
                        continue;
                    }

                    MergeRule rule = new MergeRule(_plugin.Settings.Settings)
                    {
                        Type = NewType,
                        Name = item.Name,
                        SourceObjects = new ObservableCollection<MetadataListObject>
                        {
                            new MetadataListObject(_plugin.Settings.Settings)
                            {
                                Type = item.Type,
                                Name = item.Name,
                                Id = item.Id
                            }
                        }
                    };

                    rule.Merge();

                    NewObjects.Add(new MetadataListObject(_plugin.Settings.Settings)
                    {
                        Type = NewType,
                        Name = item.Name,
                        Id = rule.Id
                    });

                    if (!SaveAsRule)
                    {
                        continue;
                    }

                    _plugin.Settings.Settings.MergeRules.AddRule(rule);
                }

                _plugin.SavePluginSettings(_plugin.Settings.Settings);
                _plugin.Settings.MergeRuleViewSource.View.Refresh();
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