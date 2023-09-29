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
        private QuickCategories _quickCategories;
        private QuickFeatures _quickFeatures;
        private QuickTags _quickTags;

        [DontSerialize]
        public List<Guid> CheckedFeatures => QuickFeatures?.Where(x => x.Add)?.Select(x => x.Id).ToList();

        [DontSerialize]
        public List<Guid> CheckedTags => QuickTags?.Where(x => x.Add)?.Select(x => x.Id).ToList();

        [DontSerialize]
        public List<Guid> CheckedCategories => QuickCategories?.Where(x => x.Add)?.Select(x => x.Id).ToList();

        public QuickFeatures QuickFeatures
        {
            get => _quickFeatures;
            set => SetValue(ref _quickFeatures, value);
        }

        public QuickTags QuickTags
        {
            get => _quickTags;
            set => SetValue(ref _quickTags, value);
        }

        public QuickCategories QuickCategories
        {
            get => _quickCategories;
            set => SetValue(ref _quickCategories, value);
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

            Settings.QuickTags = QuickTags.GetTags(Settings.CheckedTags);
            Settings.QuickFeatures = QuickFeatures.GetFeatures(Settings.CheckedFeatures);
            Settings.QuickCategories = QuickCategories.GetCategories(Settings.CheckedCategories);
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

            Settings.QuickTags = QuickTags.GetTags(Settings.CheckedTags);
            Settings.QuickFeatures = QuickFeatures.GetFeatures(Settings.CheckedFeatures);
            Settings.QuickCategories = QuickCategories.GetCategories(Settings.CheckedCategories);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;

            Settings.QuickTags = QuickTags.GetTags(Settings.CheckedTags);
            Settings.QuickFeatures = QuickFeatures.GetFeatures(Settings.CheckedFeatures);
            Settings.QuickCategories = QuickCategories.GetCategories(Settings.CheckedCategories);
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