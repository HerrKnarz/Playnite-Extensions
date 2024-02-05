using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class Settings : ObservableObject
    {
        private bool _alwaysSaveManualMergeRules;
        private ObservableCollection<MetadataListObject> _defaultCategories = new ObservableCollection<MetadataListObject>();
        private ObservableCollection<MetadataListObject> _defaultTags = new ObservableCollection<MetadataListObject>();
        private int _editorWindowHeight = 600;
        private int _editorWindowWidth = 1200;
        private bool _ignoreHiddenGamesInGameCount;
        private bool _ignoreHiddenGamesInRemoveUnused;
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private bool _mergeMetadataOnMetadataUpdate;
        private MergeRules _mergeRules = new MergeRules();
        private CollectionViewSource _mergeRuleViewSource;
        private bool _removeUnusedOnStartup;
        private MergeRule _selectedMergeRule;
        private bool _setDefaultTagsOnlyIfEmpty = true;
        private bool _showTopPanelButton = true;
        private CollectionViewSource _sourceObjectsViewSource;
        private bool _writeDebugLog;


        public Settings()
        {
            MergeRuleViewSource = new CollectionViewSource
            {
                Source = MergeRules
            };

            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            MergeRuleViewSource.IsLiveSortingRequested = true;

            SourceObjectsViewSource = new CollectionViewSource();

            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            SourceObjectsViewSource.IsLiveSortingRequested = true;
        }

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

        public int EditorWindowHeight
        {
            get => _editorWindowHeight;
            set => SetValue(ref _editorWindowHeight, value);
        }

        public int EditorWindowWidth
        {
            get => _editorWindowWidth;
            set => SetValue(ref _editorWindowWidth, value);
        }

        public bool IgnoreHiddenGamesInGameCount
        {
            get => _ignoreHiddenGamesInGameCount;
            set => SetValue(ref _ignoreHiddenGamesInGameCount, value);
        }

        public bool IgnoreHiddenGamesInRemoveUnused
        {
            get => _ignoreHiddenGamesInRemoveUnused;
            set => SetValue(ref _ignoreHiddenGamesInRemoveUnused, value);
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

        [DontSerialize]
        public CollectionViewSource MergeRuleViewSource
        {
            get => _mergeRuleViewSource;
            set
            {
                _mergeRuleViewSource = value;
                OnPropertyChanged();
            }
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
            set
            {
                SetValue(ref _selectedMergeRule, value);
                SourceObjectsViewSource.Source = _selectedMergeRule.SourceObjects;
                SourceObjectsViewSource.View.Refresh();
            }
        }

        public bool SetDefaultTagsOnlyIfEmpty
        {
            get => _setDefaultTagsOnlyIfEmpty;
            set => SetValue(ref _setDefaultTagsOnlyIfEmpty, value);
        }

        public bool ShowTopPanelButton
        {
            get => _showTopPanelButton;
            set => SetValue(ref _showTopPanelButton, value);
        }

        [DontSerialize]
        public CollectionViewSource SourceObjectsViewSource
        {
            get => _sourceObjectsViewSource;
            set
            {
                _sourceObjectsViewSource = value;
                OnPropertyChanged();
            }
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
                MetadataListObjects items = new MetadataListObjects(Settings);

                items.LoadMetadata(false, FieldType.Category);

                Window window = SelectMetadataViewModel.GetWindow(plugin, items, ResourceProvider.GetString("LOCCategoriesLabel"));

                if (window == null)
                {
                    return;
                }

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
                StringSelectionDialogResult res = API.Instance.Dialogs.SelectString(ResourceProvider.GetString("LOCMetadataUtilitiesSettingsAddValue"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), "");

                if (!res.Result)
                {
                    return;
                }

                if (Settings.DefaultCategories.Any(x => x.Name == res.SelectedString))
                {
                    return;
                }

                Settings.DefaultCategories.Add(new MetadataListObject
                {
                    Name = res.SelectedString,
                    Type = FieldType.Category
                });

                Settings.DefaultCategories = new ObservableCollection<MetadataListObject>(Settings.DefaultCategories.OrderBy(x => x.Name));
            });

        public RelayCommand AddExistingDefaultTagsCommand
            => new RelayCommand(() =>
            {
                MetadataListObjects items = new MetadataListObjects(Settings);

                items.LoadMetadata(false, FieldType.Tag);

                Window window = SelectMetadataViewModel.GetWindow(plugin, items, ResourceProvider.GetString("LOCTagsLabel"));

                if (window == null)
                {
                    return;
                }

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
                StringSelectionDialogResult res = API.Instance.Dialogs.SelectString(ResourceProvider.GetString("LOCMetadataUtilitiesSettingsAddValue"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), "");

                if (!res.Result)
                {
                    return;
                }

                if (Settings.DefaultTags.Any(x => x.Name == res.SelectedString))
                {
                    return;
                }

                Settings.DefaultTags.Add(new MetadataListObject
                {
                    Name = res.SelectedString,
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
            MetadataListObject newItem = new MetadataListObject();

            Window window = AddNewObjectViewModel.GetWindow(plugin, newItem);

            if (window == null)
            {
                return;
            }

            if (window.ShowDialog() ?? false)
            {
                if (!((MergeRule)rule).SourceObjects.Any(x => x.Name == newItem.Name && x.Type == newItem.Type))
                {
                    ((MergeRule)rule).SourceObjects.Add(newItem);
                }
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

                MetadataListObjects metadataListObjects = new MetadataListObjects(Settings);
                metadataListObjects.LoadMetadata();

                foreach (MetadataListObject item in ruleToEdit.SourceObjects)
                {
                    MetadataListObject foundItem = metadataListObjects.FirstOrDefault(x => x.Name == item.Name && x.Type == item.Type);

                    if (foundItem != null)
                    {
                        foundItem.Selected = true;
                    }
                    else
                    {
                        item.Selected = true;
                        metadataListObjects.Add(item);
                    }
                }

                MergeRuleEditorViewModel viewModel = new MergeRuleEditorViewModel(plugin, metadataListObjects);
                viewModel.RuleName = ruleToEdit.Name;
                viewModel.RuleType = ruleToEdit.Type;

                MergeRuleEditorView editorView = new MergeRuleEditorView();

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCMetadataUtilitiesMergeRuleEditor"), 700, 700, false, true);
                window.Content = editorView;
                window.DataContext = viewModel;


                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                ruleToEdit.Name = viewModel.RuleName;
                ruleToEdit.Type = viewModel.RuleType;
                ruleToEdit.SourceObjects.Clear();

                foreach (MetadataListObject item in metadataListObjects.Where(x => x.Selected).ToList())
                {
                    ruleToEdit.SourceObjects.Add(new MetadataListObject
                    {
                        Id = item.Id,
                        EditName = item.EditName,
                        Name = item.Name,
                        Type = item.Type,
                        Selected = item.Selected
                    });
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
                    MessageBoxResult response = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergeOrReplace"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

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