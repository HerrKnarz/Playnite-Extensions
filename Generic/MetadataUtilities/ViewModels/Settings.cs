using KNARZhelper;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MetadataUtilities
{
    public class Settings : ObservableObject
    {
        private bool _alwaysSaveManualMergeRules;
        private ObservableCollection<MetadataListObject> _defaultCategories = new ObservableCollection<MetadataListObject>();
        private ObservableCollection<MetadataListObject> _defaultTags = new ObservableCollection<MetadataListObject>();
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private bool _mergeMetadataOnMetadataUpdate;
        private MergeRules _mergeRules = new MergeRules();
        private bool _removeUnusedOnStartup;
        private MergeRule _selectedMergeRule;
        private bool _setDefaultTagsOnlyIfEmpty = true;
        private bool _writeDebugLog;

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

        public DateTime LastAutoLibUpdate
        {
            get => _lastAutoLibUpdate;
            set => SetValue(ref _lastAutoLibUpdate, value);
        }

        public bool MergeMetadataOnMetadataUpdate
        {
            get => _mergeMetadataOnMetadataUpdate;
            set => SetValue(ref _mergeMetadataOnMetadataUpdate, value);
        }

        public MergeRules MergeRules
        {
            get => _mergeRules;
            set => SetValue(ref _mergeRules, value);
        }

        public bool RemoveUnusedOnStartup
        {
            get => _removeUnusedOnStartup;
            set => SetValue(ref _removeUnusedOnStartup, value);
        }

        [DontSerialize]
        public MergeRule SelectedMergeRule
        {
            get => _selectedMergeRule;
            set => SetValue(ref _selectedMergeRule, value);
        }

        public bool SetDefaultTagsOnlyIfEmpty
        {
            get => _setDefaultTagsOnlyIfEmpty;
            set => SetValue(ref _setDefaultTagsOnlyIfEmpty, value);
        }

        public bool WriteDebugLog
        {
            get => _writeDebugLog;
            set => SetValue(ref _writeDebugLog, value);
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

        public RelayCommand AddExistingDefaultCategoriesCommand
            => new RelayCommand(() =>
            {
                MetadataListObjects items = new MetadataListObjects();

                items.LoadMetadata(false, FieldType.Category);

                SelectMetadataView selectMetadataView = new SelectMetadataView(plugin, items);

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCCategoriesLabel"));

                window.Content = selectMetadataView;

                if (window.ShowDialog() ?? false)
                {
                    foreach (MetadataListObject item in items.Where(x => x.Selected))
                    {
                        if (Settings.DefaultCategories.Any(x => x.Name == item.Name))
                        {
                            continue;
                        }

                        Settings.DefaultCategories.Add(new MetadataListObject
                        {
                            Name = item.Name,
                            Type = FieldType.Category
                        });
                    }
                }

                Settings.DefaultCategories = new ObservableCollection<MetadataListObject>(Settings.DefaultCategories.OrderBy(x => x.Name));
            });

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

        public RelayCommand AddExistingDefaultTagsCommand
            => new RelayCommand(() =>
            {
                MetadataListObjects items = new MetadataListObjects();

                items.LoadMetadata(false, FieldType.Tag);

                SelectMetadataView selectMetadataView = new SelectMetadataView(plugin, items);

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCTagsLabel"));

                window.Content = selectMetadataView;

                if (window.ShowDialog() ?? false)
                {
                    foreach (MetadataListObject item in items.Where(x => x.Selected))
                    {
                        if (Settings.DefaultTags.Any(x => x.Name == item.Name))
                        {
                            continue;
                        }

                        Settings.DefaultTags.Add(new MetadataListObject
                        {
                            Name = item.Name,
                            Type = FieldType.Category
                        });
                    }
                }

                Settings.DefaultTags = new ObservableCollection<MetadataListObject>(Settings.DefaultTags.OrderBy(x => x.Name));
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

        public RelayCommand AddNewMergeRuleCommand
            => new RelayCommand(() =>
            {
                EditMergeRule();
            });

        public RelayCommand<object> EditMergeRuleCommand => new RelayCommand<object>(rule =>
        {
            EditMergeRule((MergeRule)rule);
        }, rule => rule != null);

        public RelayCommand<object> RemoveMergeRuleCommand => new RelayCommand<object>(rule =>
        {
            Settings.MergeRules.Remove((MergeRule)rule);
        }, rule => rule != null);

        public RelayCommand<object> AddNewMergeSourceCommand => new RelayCommand<object>(rule =>
        {
            try
            {
                MetadataListObject newItem = new MetadataListObject();

                AddNewObjectView newObjectViewView = new AddNewObjectView(plugin, newItem);

                Window window = WindowHelper.CreateFixedDialog(ResourceProvider.GetString("LOCMetadataUtilitiesDialogAddNewObject"));

                window.Content = newObjectViewView;

                if (window.ShowDialog() ?? false)
                {
                    if (!((MergeRule)rule).SourceObjects.Any(x => x.Name == newItem.Name && x.Type == newItem.Type))
                    {
                        ((MergeRule)rule).SourceObjects.Add(newItem);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing merge dialog", true);
            }
        }, rule => rule != null);

        public RelayCommand<IList<object>> RemoveMergeSourceCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataListObject item in items.ToList().Cast<MetadataListObject>())
            {
                Settings.SelectedMergeRule.SourceObjects.Remove(item);
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

        private void EditMergeRule(MergeRule rule = null)
        {
            try
            {
                bool isNewRule = rule == null;

                MergeRule ruleToEdit = new MergeRule();

                if (rule != null)
                {
                    ruleToEdit.Type = rule.Type;
                    ruleToEdit.Name = rule.Name;

                    foreach (MetadataListObject sourceItem in rule.SourceObjects)
                    {
                        ruleToEdit.SourceObjects.Add(new MetadataListObject
                        {
                            Name = sourceItem.Name,
                            EditName = sourceItem.Name,
                            Type = sourceItem.Type,
                            GameCount = 0
                        });
                    }
                }

                MetadataEditorView editorView = new MetadataEditorView(plugin, true, ruleToEdit);

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCMetadataUtilitiesMergeRuleEditor"), 700, 600, false, true);

                window.Content = editorView;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                // Case 1: The rule wasn't renamed => We simply replace the SourceObjects.
                if (!isNewRule && rule.Name == ruleToEdit.Name && rule.Type == ruleToEdit.Type)
                {
                    rule.SourceObjects.Clear();
                    rule.SourceObjects.AddMissing(ruleToEdit.SourceObjects);

                    return;
                }

                // Case 2: The rule was renamed or is new and another one for that target already exists => We ask if merge, replace or cancel.
                if (Settings.MergeRules.Any(x => x.Name == ruleToEdit.Name && x.Type == ruleToEdit.Type))
                {
                    MessageBoxResult response = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergeOrReplace"), string.Empty, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    switch (response)
                    {
                        case MessageBoxResult.Yes:
                            Settings.MergeRules.AddRule(ruleToEdit, true);
                            Settings.MergeRules.Remove(rule);
                            return;
                        case MessageBoxResult.No:
                            Settings.MergeRules.AddRule(ruleToEdit);
                            Settings.MergeRules.Remove(rule);
                            return;
                        default:
                            return;
                    }
                }

                // Case 3: The rule was renamed, but no other with that target exists => We replace it.
                if (!isNewRule)
                {
                    rule.Type = ruleToEdit.Type;
                    rule.Name = ruleToEdit.Name;

                    rule.SourceObjects.Clear();
                    rule.SourceObjects.AddMissing(ruleToEdit.SourceObjects);
                }

                // Case 4: The rule is new and no other with that target exists => we simply add the new one.
                Settings.MergeRules.AddRule(ruleToEdit);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Merge Rule Editor", true);
            }
        }
    }
}