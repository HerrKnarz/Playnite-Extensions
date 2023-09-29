using Playnite.SDK;
using Playnite.SDK.Data;
using QuickAdd.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickAdd
{
    public class QuickAddSettings : ObservableObject
    {
        private QuickDbObjects _quickCategories;
        private QuickDbObjects _quickFeatures;
        private QuickDbObjects _quickTags;

        public QuickAddSettings()
        {
            _quickCategories = QuickDbObjects.GetObjects(null, FieldType.Category);
            _quickFeatures = QuickDbObjects.GetObjects(null, FieldType.Feature);
            _quickTags = QuickDbObjects.GetObjects(null, FieldType.Tag);
        }

        public QuickDbObjects QuickCategories
        {
            get => _quickCategories;
            set => SetValue(ref _quickCategories, value);
        }

        public QuickDbObjects QuickFeatures
        {
            get => _quickFeatures;
            set => SetValue(ref _quickFeatures, value);
        }

        public QuickDbObjects QuickTags
        {
            get => _quickTags;
            set => SetValue(ref _quickTags, value);
        }
    }

    public class QuickAddSettingsViewModel : ObservableObject, ISettings
    {
        private readonly QuickAdd _plugin;

        private QuickAddSettings _settings;

        public QuickAddSettingsViewModel(QuickAdd plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            _plugin = plugin;

            // Load saved settings.
            QuickAddSettings savedSettings = plugin.LoadPluginSettings<QuickAddSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            Settings = savedSettings ?? new QuickAddSettings();
        }

        private QuickAddSettings EditingClone { get; set; }

        public QuickAddSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);

            RefreshList(FieldType.Category);
            RefreshList(FieldType.Feature);
            RefreshList(FieldType.Tag);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;

            RefreshList(FieldType.Category);
            RefreshList(FieldType.Feature);
            RefreshList(FieldType.Tag);
        }

        public void EndEdit() => _plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        public void RefreshList(FieldType type)
        {
            switch (type)
            {
                case FieldType.Category:
                    Settings.QuickCategories = QuickDbObjects.GetObjects(Settings.QuickCategories.ToList(), type);

                    break;
                case FieldType.Feature:
                    Settings.QuickFeatures = QuickDbObjects.GetObjects(Settings.QuickFeatures.ToList(), type);

                    break;
                case FieldType.Tag:
                    Settings.QuickTags = QuickDbObjects.GetObjects(Settings.QuickTags.ToList(), type);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}