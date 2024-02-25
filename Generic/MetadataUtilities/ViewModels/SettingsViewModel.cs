using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Data;

namespace MetadataUtilities.ViewModels
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly MetadataUtilities _plugin;
        private MergeRules _mergeRules;
        private CollectionViewSource _mergeRuleViewSource;
        private MergeRule _selectedMergeRule;
        private Settings _settings;
        private CollectionViewSource _sourceObjectsViewSource;

        public SettingsViewModel(MetadataUtilities plugin)
        {
            _plugin = plugin;

            _mergeRules = Settings?.MergeRules;

            Settings savedSettings = plugin.LoadPluginSettings<Settings>();

            Settings = savedSettings ?? new Settings();

            MergeRuleViewSource = new CollectionViewSource();

            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            MergeRuleViewSource.IsLiveSortingRequested = true;

            SourceObjectsViewSource = new CollectionViewSource();

            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            SourceObjectsViewSource.IsLiveSortingRequested = true;
        }

        public RelayCommand AddExistingDefaultCategoriesCommand
            => new RelayCommand(() =>
            {
                MetadataObjects items = new MetadataObjects(Settings);

                items.LoadMetadata(false, FieldType.Category);

                Window window = SelectMetadataViewModel.GetWindow(_plugin, items, ResourceProvider.GetString("LOCCategoriesLabel"));

                if (window == null)
                {
                    return;
                }

                if (window.ShowDialog() ?? false)
                {
                    foreach (MetadataObject item in items.Where(x => x.Selected))
                    {
                        if (Settings.DefaultCategories.Any(x => x.Name == item.Name))
                        {
                            continue;
                        }

                        Settings.DefaultCategories.Add(new MetadataObject(_settings)
                        {
                            Name = item.Name,
                            Type = FieldType.Category
                        });
                    }
                }

                Settings.DefaultCategories = new ObservableCollection<MetadataObject>(Settings.DefaultCategories.OrderBy(x => x.Name));
            });

        public RelayCommand AddExistingDefaultTagsCommand
            => new RelayCommand(() =>
            {
                MetadataObjects items = new MetadataObjects(Settings);

                items.LoadMetadata(false, FieldType.Tag);

                Window window = SelectMetadataViewModel.GetWindow(_plugin, items, ResourceProvider.GetString("LOCTagsLabel"));

                if (window == null)
                {
                    return;
                }

                if (window.ShowDialog() ?? false)
                {
                    foreach (MetadataObject item in items.Where(x => x.Selected))
                    {
                        if (Settings.DefaultTags.Any(x => x.Name == item.Name))
                        {
                            continue;
                        }

                        Settings.DefaultTags.Add(new MetadataObject(_settings)
                        {
                            Name = item.Name,
                            Type = FieldType.Category
                        });
                    }
                }

                Settings.DefaultTags = new ObservableCollection<MetadataObject>(Settings.DefaultTags.OrderBy(x => x.Name));
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

                Settings.DefaultCategories.Add(new MetadataObject(_settings)
                {
                    Name = res.SelectedString,
                    Type = FieldType.Category
                });

                Settings.DefaultCategories = new ObservableCollection<MetadataObject>(Settings.DefaultCategories.OrderBy(x => x.Name));
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

                Settings.DefaultTags.Add(new MetadataObject(_settings)
                {
                    Name = res.SelectedString,
                    Type = FieldType.Tag
                });

                Settings.DefaultTags = new ObservableCollection<MetadataObject>(Settings.DefaultTags.OrderBy(x => x.Name));
            });

        public RelayCommand AddNewMergeRuleCommand => new RelayCommand(() => EditMergeRule());

        public RelayCommand<object> AddNewMergeSourceCommand => new RelayCommand<object>(rule =>
        {
            MetadataObject newItem = new MetadataObject(_settings);

            if (SourceObjectsViewSource.View?.CurrentItem != null)
            {
                MetadataObject templateItem = (MetadataObject)SourceObjectsViewSource.View.CurrentItem;

                newItem.Type = templateItem.Type;
                newItem.Prefix = templateItem.Prefix;
            }

            Window window = AddNewObjectViewModel.GetWindow(_plugin, newItem);

            if (window == null)
            {
                return;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return;
            }

            if (((MergeRule)rule).SourceObjects.Any(x => x.Name == newItem.Name && x.Type == newItem.Type))
            {
                return;
            }

            ((MergeRule)rule).SourceObjects.Add(newItem);
            SourceObjectsViewSource.View?.MoveCurrentTo(newItem);
        }, rule => rule != null);

        public RelayCommand AddPrefixCommand
            => new RelayCommand(() =>
            {
                StringSelectionDialogResult res = API.Instance.Dialogs.SelectString(ResourceProvider.GetString("LOCMetadataUtilitiesSettingsAddValue"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), "");

                if (!res.Result)
                {
                    return;
                }

                Settings.Prefixes.AddMissing(res.SelectedString);
                Settings.Prefixes = new ObservableCollection<string>(Settings.Prefixes.OrderBy(x => x));
            });

        public RelayCommand AddQuickAddCategoriesCommand
            => new RelayCommand(() => AddQuickAddItems(FieldType.Category));

        public RelayCommand AddQuickAddFeaturesCommand
            => new RelayCommand(() => AddQuickAddItems(FieldType.Feature));

        public RelayCommand AddQuickAddGenresCommand
            => new RelayCommand(() => AddQuickAddItems(FieldType.Genre));

        public RelayCommand AddQuickAddSeriesCommand
            => new RelayCommand(() => AddQuickAddItems(FieldType.Series));

        public RelayCommand AddQuickAddTagsCommand
            => new RelayCommand(() => AddQuickAddItems(FieldType.Tag));

        public RelayCommand AddUnwantedCategoriesCommand
            => new RelayCommand(() => AddUnwantedItems(FieldType.Category));

        public RelayCommand AddUnwantedFeaturesCommand
            => new RelayCommand(() => AddUnwantedItems(FieldType.Feature));

        public RelayCommand AddUnwantedGenresCommand
            => new RelayCommand(() => AddUnwantedItems(FieldType.Genre));

        public RelayCommand AddUnwantedSeriesCommand
            => new RelayCommand(() => AddUnwantedItems(FieldType.Series));

        public RelayCommand AddUnwantedTagsCommand
            => new RelayCommand(() => AddUnwantedItems(FieldType.Tag));

        public RelayCommand<object> EditMergeRuleCommand
            => new RelayCommand<object>(rule => EditMergeRule((MergeRule)rule), rule => rule != null);

        public RelayCommand<object> MergeItemsCommand
            => new RelayCommand<object>(rule => _plugin.MergeItems(null, (MergeRule)rule), rule => rule != null);

        public MergeRules MergeRules
        {
            get => _mergeRules;
            set => SetValue(ref _mergeRules, value);
        }

        public CollectionViewSource MergeRuleViewSource
        {
            get => _mergeRuleViewSource;
            set
            {
                _mergeRuleViewSource = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<IList<object>> RemoveDefaultCategoryCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
            {
                Settings.DefaultCategories.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveDefaultTagCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
            {
                Settings.DefaultTags.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<object> RemoveMergeRuleCommand => new RelayCommand<object>(rule =>
        {
            Settings.MergeRules.Remove((MergeRule)rule);
        }, rule => rule != null);

        public RelayCommand<IList<object>> RemoveMergeSourceCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
            {
                SelectedMergeRule.SourceObjects.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemovePrefixCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (string item in items.ToList().Cast<string>())
            {
                Settings.Prefixes.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveQuickAddFromListCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (QuickAddObject item in items.ToList().Cast<QuickAddObject>())
            {
                Settings.QuickAddObjects.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveUnwantedFromListCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (MetadataObject item in items.ToList().Cast<MetadataObject>())
            {
                Settings.UnwantedItems.Remove(item);
            }
        }, items => items?.Count != 0);

        public MergeRule SelectedMergeRule
        {
            get => _selectedMergeRule;
            set
            {
                SetValue(ref _selectedMergeRule, value);

                SourceObjectsViewSource.Source = _selectedMergeRule == null
                    ? new ObservableCollection<MetadataObject>()
                    : _selectedMergeRule.SourceObjects;

                SourceObjectsViewSource.View.Refresh();
            }
        }

        public Settings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public CollectionViewSource SourceObjectsViewSource
        {
            get => _sourceObjectsViewSource;
            set
            {
                _sourceObjectsViewSource = value;
                OnPropertyChanged();
            }
        }

        private Settings EditingClone { get; set; }

        public void AddItemsToQuickAddList(List<MetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (MetadataObject item in items.Where(item => Settings.QuickAddObjects.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Settings.QuickAddObjects.Add(new QuickAddObject(_settings)
                {
                    Name = item.Name,
                    Type = item.Type
                });
            }

            Settings.QuickAddObjects = new ObservableCollection<QuickAddObject>(Settings.QuickAddObjects.OrderBy(x => x.TypeAndName));
        }

        public void AddItemsToUnwantedList(List<MetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (MetadataObject item in items.Where(item => Settings.UnwantedItems.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Settings.UnwantedItems.Add(new MetadataObject(_settings)
                {
                    Name = item.Name,
                    Type = item.Type
                });
            }

            Settings.UnwantedItems = new ObservableCollection<MetadataObject>(Settings.UnwantedItems.OrderBy(x => x.TypeAndName));
        }

        public void AddQuickAddItems(FieldType type)
        {
            List<MetadataObject> items = GetItemsFromAddDialog(type);

            if (items.Count == 0)
            {
                return;
            }

            AddItemsToQuickAddList(items);
        }

        public void AddUnwantedItems(FieldType type)
        {
            List<MetadataObject> items = GetItemsFromAddDialog(type);

            if (items.Count == 0)
            {
                return;
            }

            AddItemsToUnwantedList(items);
        }

        public void BeginEdit()
        {
            MergeRules = Settings.MergeRules;
            MergeRuleViewSource.Source = MergeRules;
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit() => Settings = EditingClone;

        public void EndEdit() => _plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        private void EditMergeRule(MergeRule rule = null)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                bool isNewRule = rule == null;

                MergeRule ruleToEdit = new MergeRule(_settings);

                if (rule != null)
                {
                    ruleToEdit.Type = rule.Type;
                    ruleToEdit.Name = rule.Name;

                    foreach (MetadataObject sourceItem in rule.SourceObjects)
                    {
                        ruleToEdit.SourceObjects.Add(new MetadataObject(_settings)
                        {
                            Name = sourceItem.Name,
                            Type = sourceItem.Type,
                            GameCount = 0
                        });
                    }
                }

                MetadataObjects metadataObjects = new MetadataObjects(Settings);

                if (isNewRule)
                {
                    metadataObjects.LoadMetadata();
                }
                else
                {
                    MetadataObjects temp = new MetadataObjects(Settings);
                    temp.LoadMetadata();

                    foreach (MetadataObject item in ruleToEdit.SourceObjects)
                    {
                        MetadataObject foundItem = temp.FirstOrDefault(x => x.Name == item.Name && x.Type == item.Type);

                        if (foundItem != null)
                        {
                            foundItem.Selected = true;
                        }
                        else
                        {
                            item.Selected = true;
                            item.Id = new Guid();
                            temp.Add(item);
                        }
                    }

                    metadataObjects.AddMissing(temp.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
                }

                HashSet<FieldType> filteredTypes = new HashSet<FieldType>();

                if (ruleToEdit.SourceObjects.Any(x => x.Type == FieldType.Category))
                {
                    filteredTypes.Add(FieldType.Category);
                }

                if (ruleToEdit.SourceObjects.Any(x => x.Type == FieldType.Feature))
                {
                    filteredTypes.Add(FieldType.Feature);
                }

                if (ruleToEdit.SourceObjects.Any(x => x.Type == FieldType.Genre))
                {
                    filteredTypes.Add(FieldType.Genre);
                }

                if (ruleToEdit.SourceObjects.Any(x => x.Type == FieldType.Series))
                {
                    filteredTypes.Add(FieldType.Series);
                }

                if (ruleToEdit.SourceObjects.Any(x => x.Type == FieldType.Tag))
                {
                    filteredTypes.Add(FieldType.Tag);
                }

                MergeRuleEditorViewModel viewModel = new MergeRuleEditorViewModel(_plugin, metadataObjects, filteredTypes)
                {
                    RuleName = ruleToEdit.Name,
                    RuleType = ruleToEdit.Type
                };

                MergeRuleEditorView editorView = new MergeRuleEditorView();

                Window window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOCMetadataUtilitiesMergeRuleEditor"), 700, 700, false, true);
                window.Content = editorView;
                window.DataContext = viewModel;

                if (!(window.ShowDialog() ?? false))
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                ruleToEdit.Name = viewModel.RuleName;
                ruleToEdit.Type = viewModel.RuleType;
                ruleToEdit.SourceObjects.Clear();

                foreach (MetadataObject item in metadataObjects.Where(x => x.Selected).ToList())
                {
                    ruleToEdit.SourceObjects.Add(new MetadataObject(_settings)
                    {
                        Id = item.Id,
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

                // Case 2: The rule was renamed or is new and another one for that target already
                // exists => We ask if merge, replace or cancel.
                if (Settings.MergeRules.Any(x => x.Name == ruleToEdit.Name && x.Type == ruleToEdit.Type))
                {
                    Cursor.Current = Cursors.Default;
                    MessageBoxResult response = API.Instance.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergeOrReplace"),
                        ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);
                    Cursor.Current = Cursors.WaitCursor;

                    switch (response)
                    {
                        case MessageBoxResult.Yes:
                            Settings.MergeRules.AddRule(ruleToEdit, true);
                            Settings.MergeRules.Remove(rule);
                            MergeRuleViewSource.View.MoveCurrentTo(ruleToEdit);
                            return;

                        case MessageBoxResult.No:
                            Settings.MergeRules.AddRule(ruleToEdit);
                            Settings.MergeRules.Remove(rule);
                            MergeRuleViewSource.View.MoveCurrentTo(ruleToEdit);
                            return;

                        case MessageBoxResult.None:
                        case MessageBoxResult.OK:
                        case MessageBoxResult.Cancel:
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
                    MergeRuleViewSource.View.MoveCurrentTo(ruleToEdit);
                }

                // Case 4: The rule is new and no other with that target exists => we simply add the
                // new one.
                Settings.MergeRules.AddRule(ruleToEdit);
                MergeRuleViewSource.View.MoveCurrentTo(ruleToEdit);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing Merge Rule Editor", true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private List<MetadataObject> GetItemsFromAddDialog(FieldType type)
        {
            MetadataObjects items = new MetadataObjects(Settings);

            items.LoadMetadata(false, type);

            string label;

            switch (type)
            {
                case FieldType.Category:
                    label = ResourceProvider.GetString("LOCCategoriesLabel");
                    break;

                case FieldType.Feature:
                    label = ResourceProvider.GetString("LOCFeaturesLabel");
                    break;

                case FieldType.Genre:
                    label = ResourceProvider.GetString("LOCGenresLabel");
                    break;

                case FieldType.Series:
                    label = ResourceProvider.GetString("LOCSeriesLabel");
                    break;

                case FieldType.Tag:
                    label = ResourceProvider.GetString("LOCTagsLabel");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Window window = SelectMetadataViewModel.GetWindow(_plugin, items, label);

            return (window?.ShowDialog() ?? false)
                ? items.Where(x => x.Selected).ToList()
                : items.ToList();
        }
    }
}