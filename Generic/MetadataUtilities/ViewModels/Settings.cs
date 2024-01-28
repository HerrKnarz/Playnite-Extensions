using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities
{
    public class Settings : ObservableObject
    {
        private bool _alwaysSaveManualMergeRules;
        private ObservableCollection<MetadataListObject> _defaultCategories = new ObservableCollection<MetadataListObject>();
        private ObservableCollection<MetadataListObject> _defaultTags = new ObservableCollection<MetadataListObject>();
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private MergeRules _mergeRules = new MergeRules();
        private bool _setDefaultTagsOnlyIfEmpty;

        public bool AlwaysSaveManualMergeRules
        {
            get => _alwaysSaveManualMergeRules;
            set => SetValue(ref _alwaysSaveManualMergeRules, value);
        }


        public ObservableCollection<MetadataListObject> DefaultCategories
        {
            get => _defaultCategories;
            set => SetValue(ref _defaultCategories, value);
        }

        public ObservableCollection<MetadataListObject> DefaultTags
        {
            get => _defaultTags;
            set => SetValue(ref _defaultTags, value);
        }

        public MergeRules MergeRules
        {
            get => _mergeRules;
            set => SetValue(ref _mergeRules, value);
        }

        public DateTime LastAutoLibUpdate
        {
            get => _lastAutoLibUpdate;
            set => SetValue(ref _lastAutoLibUpdate, value);
        }

        public bool SetDefaultTagsOnlyIfEmpty
        {
            get => _setDefaultTagsOnlyIfEmpty;
            set => SetValue(ref _setDefaultTagsOnlyIfEmpty, value);
        }
    }

    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly MetadataUtilities plugin;

        private Settings _settings;

        public SettingsViewModel(MetadataUtilities plugin)
        {
            this.plugin = plugin;

            Settings savedSettings = plugin.LoadPluginSettings<Settings>();

            Settings = savedSettings ?? new Settings();
        }

        private Settings EditingClone { get; set; }

        public Settings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddNewDefaultCategoryCommand
            => new RelayCommand(() =>
            {
                string value = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCMetadataUtilitiesSettingsAddValue"), "").SelectedString;

                if (Settings.DefaultCategories.Any(x => x.Name == value))
                {
                    return;
                }

                Settings.DefaultCategories.Add(new MetadataListObject
                {
                    Name = value,
                    Type = FieldType.Category
                });

                Settings.DefaultCategories = new ObservableCollection<MetadataListObject>(Settings.DefaultCategories.OrderBy(x => x.Name));
            });

        public RelayCommand AddNewDefaultTagCommand
            => new RelayCommand(() =>
            {
                string value = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCMetadataUtilitiesSettingsAddValue"), "").SelectedString;

                if (Settings.DefaultTags.Any(x => x.Name == value))
                {
                    return;
                }

                Settings.DefaultTags.Add(new MetadataListObject
                {
                    Name = value,
                    Type = FieldType.Tag
                });

                Settings.DefaultTags = new ObservableCollection<MetadataListObject>(Settings.DefaultTags.OrderBy(x => x.Name));
            });

        public RelayCommand<IList<object>> RemoveDefaultCategoryCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataListObject item in items.ToList().Cast<MetadataListObject>())
            {
                Settings.DefaultCategories.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public RelayCommand<IList<object>> RemoveDefaultTagCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataListObject item in items.ToList().Cast<MetadataListObject>())
            {
                Settings.DefaultTags.Remove(item);
            }
        }, items => items?.Any() ?? false);

        public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() => Settings = EditingClone;

        public void EndEdit() => plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}