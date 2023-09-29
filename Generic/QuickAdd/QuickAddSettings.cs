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
        private QuickDBObjects _quickCategories;
        private QuickDBObjects _quickFeatures;
        private QuickDBObjects _quickTags;

        public QuickAddSettings()
        {
            _quickCategories = QuickDBObjects.GetObjects(null, FieldType.Category);
            _quickFeatures = QuickDBObjects.GetObjects(null, FieldType.Feature);
            _quickTags = QuickDBObjects.GetObjects(null, FieldType.Tag);
        }

        public QuickDBObjects QuickCategories
        {
            get => _quickCategories;
            set => SetValue(ref _quickCategories, value);
        }

        public QuickDBObjects QuickFeatures
        {
            get => _quickFeatures;
            set => SetValue(ref _quickFeatures, value);
        }

        public QuickDBObjects QuickTags
        {
            get => _quickTags;
            set => SetValue(ref _quickTags, value);
        }

        [DontSerialize]
        public List<Guid> CheckedCategories => QuickCategories?.Where(x => x.Add)?.Select(x => x.Id).ToList();

        [DontSerialize]
        public List<Guid> CheckedFeatures => QuickFeatures?.Where(x => x.Add)?.Select(x => x.Id).ToList();

        [DontSerialize]
        public List<Guid> CheckedTags => QuickTags?.Where(x => x.Add)?.Select(x => x.Id).ToList();
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

            Settings.QuickCategories = QuickDBObjects.GetObjects(Settings.CheckedCategories, FieldType.Category);
            Settings.QuickFeatures = QuickDBObjects.GetObjects(Settings.CheckedFeatures, FieldType.Feature);
            Settings.QuickTags = QuickDBObjects.GetObjects(Settings.CheckedTags, FieldType.Tag);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;

            Settings.QuickCategories = QuickDBObjects.GetObjects(Settings.CheckedCategories, FieldType.Category);
            Settings.QuickFeatures = QuickDBObjects.GetObjects(Settings.CheckedFeatures, FieldType.Feature);
            Settings.QuickTags = QuickDBObjects.GetObjects(Settings.CheckedTags, FieldType.Tag);
        }

        public void EndEdit()
        {
            _plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}