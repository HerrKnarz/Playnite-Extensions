using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace MetadataUtilities
{
    public class ChangeTypeViewModel : ObservableObject
    {
        //TODO: Try to add localization to combobox!

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
            foreach (MetadataListObject item in _metadataListObjects)
            {
                if (item.Type == NewType)
                {
                    continue;
                }

                Guid newId = DatabaseObjectHelper.AddDbObject(NewType, item.Name);

                DatabaseObjectHelper.ReplaceDbObject(item.Type, item.Id, NewType, newId);

                NewObjects.Add(new MetadataListObject
                {
                    Type = NewType,
                    EditName = item.EditName,
                    Id = newId
                });

                if (!SaveAsRule)
                {
                    continue;
                }

                MergeRule rule = new MergeRule
                {
                    Type = NewType,
                    EditName = item.EditName,
                    Id = newId,
                    SourceObjects = new ObservableCollection<MetadataListObject>
                    {
                        new MetadataListObject
                        {
                            Type = item.Type,
                            EditName = item.EditName,
                            Id = item.Id
                        }
                    }
                };

                _plugin.Settings.Settings.MergeRules.AddRule(rule);
            }

            _plugin.SavePluginSettings(_plugin.Settings.Settings);

            win.DialogResult = true;
            win.Close();
        }, win => win != null);
    }
}