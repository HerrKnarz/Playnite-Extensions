using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace MetadataUtilities.ViewModels
{
    public class ChangeTypeViewModel : ObservableObject
    {
        private readonly MetadataObjects _metadataObjects;

        private MetadataObjects _newObjects;

        private FieldType _newType = FieldType.Category;
        private bool _saveAsRule;

        public ChangeTypeViewModel(MetadataObjects items)
        {
            _metadataObjects = items;
            _newObjects = new MetadataObjects();
            SaveAsRule = ControlCenter.Instance.Settings.AlwaysSaveManualMergeRules;
        }

        public Dictionary<FieldType, string> FieldValuePairs => FieldTypeHelper.ItemListFieldValues();

        public MetadataObjects NewObjects
        {
            get => _newObjects;
            set => SetValue(ref _newObjects, value);
        }

        public FieldType NewType
        {
            get => _newType;
            set => SetValue(ref _newType, value);
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(win =>
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (var item in _metadataObjects)
                {
                    if (item.Type == NewType)
                    {
                        continue;
                    }

                    var rule = new MergeRule(NewType, item.Name)
                    {
                        SourceObjects = new MetadataObjects()
                        {
                            new MetadataObject(item.Type, item.Name)
                            {
                                Id = item.Id
                            }
                        }
                    };

                    rule.Merge();

                    NewObjects.Add(new MetadataObject(NewType, item.Name)
                    {
                        Id = rule.Id
                    });

                    if (!SaveAsRule)
                    {
                        continue;
                    }

                    ControlCenter.Instance.Settings.MergeRules.AddRule(rule);
                }

                ControlCenter.Instance.SavePluginSettings();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

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