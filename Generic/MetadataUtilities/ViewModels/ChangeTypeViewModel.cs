﻿using KNARZhelper;
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
        private MetadataUtilities _plugin;
        private bool _saveAsRule;

        public ChangeTypeViewModel(MetadataUtilities plugin, MetadataObjects items)
        {
            Plugin = plugin;
            _metadataObjects = items;
            _newObjects = new MetadataObjects(plugin.Settings.Settings);
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

                    var rule = new MergeRule(_plugin.Settings.Settings, NewType, item.Name)
                    {
                        SourceObjects = new MetadataObjects(_plugin.Settings.Settings)
                        {
                            new MetadataObject(_plugin.Settings.Settings, item.Type, item.Name)
                            {
                                Id = item.Id
                            }
                        }
                    };

                    rule.Merge();

                    NewObjects.Add(new MetadataObject(_plugin.Settings.Settings, NewType, item.Name)
                    {
                        Id = rule.Id
                    });

                    if (!SaveAsRule)
                    {
                        continue;
                    }

                    _plugin.Settings.Settings.MergeRules.AddRule(rule);
                }

                _plugin.SavePluginSettings(_plugin.Settings.Settings);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            win.DialogResult = true;
            win.Close();
        }, win => win != null);

        public MetadataUtilities Plugin
        {
            get => _plugin;
            set
            {
                SetValue(ref _plugin, value);
                SaveAsRule = _plugin.Settings.Settings.AlwaysSaveManualMergeRules;
            }
        }

        public bool SaveAsRule
        {
            get => _saveAsRule;
            set => SetValue(ref _saveAsRule, value);
        }
    }
}