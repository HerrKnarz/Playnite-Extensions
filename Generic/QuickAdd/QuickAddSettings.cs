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
        private List<Guid> _checkedCategories;
        private List<Guid> _checkedFeatures;
        private List<Guid> _checkedTags;
        private QuickCategories _quickCategories;
        private QuickFeatures _quickFeatures;

        private QuickTags _quickTags;

        public List<Guid> CheckedTags
        {
            get => _checkedTags;
            set => SetValue(ref _checkedTags, value);
        }

        public List<Guid> CheckedFeatures
        {
            get => _checkedFeatures;
            set => SetValue(ref _checkedFeatures, value);
        }

        public List<Guid> CheckedCategories
        {
            get => _checkedCategories;
            set => SetValue(ref _checkedCategories, value);
        }

        [DontSerialize]
        public QuickTags QuickTags
        {
            get => _quickTags;
            set => SetValue(ref _quickTags, value);
        }

        [DontSerialize]
        public QuickFeatures QuickFeatures
        {
            get => _quickFeatures;
            set => SetValue(ref _quickFeatures, value);
        }

        [DontSerialize]
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
            Settings.CheckedTags = Settings.QuickTags.Where(x => x.Checked)?.Select(x => x.Id).ToList();
            Settings.CheckedFeatures = Settings.QuickFeatures.Where(x => x.Checked)?.Select(x => x.Id).ToList();
            Settings.CheckedCategories = Settings.QuickCategories.Where(x => x.Checked)?.Select(x => x.Id).ToList();

            _plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}