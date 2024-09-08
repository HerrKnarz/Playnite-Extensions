using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using Action = MetadataUtilities.Models.Action;
using Condition = MetadataUtilities.Models.Condition;

namespace MetadataUtilities.ViewModels
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly Dictionary<FieldType, string> _fieldTypes = FieldTypeHelper.ItemListFieldValues();
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

            Settings.ResetReferences();

            FieldTypeButtons.AddMissing(_fieldTypes
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.Value,
                        FieldType = x.Key
                    }
                ));

            MergeRuleViewSource = new CollectionViewSource();

            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("TypeLabel", ListSortDirection.Ascending));
            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            MergeRuleViewSource.IsLiveSortingRequested = true;

            SourceObjectsViewSource = new CollectionViewSource();

            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("TypeLabel", ListSortDirection.Ascending));
            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            SourceObjectsViewSource.IsLiveSortingRequested = true;
        }

        public static RelayCommand<object> RestartRequired => new RelayCommand<object>((sender) =>
                {
                    try
                    {
                        Window winParent = MiscHelper.FindParent<Window>((FrameworkElement)sender);

                        if (winParent.DataContext?.GetType().GetProperty("IsRestartRequired") != null)
                        {
                            ((dynamic)winParent.DataContext).IsRestartRequired = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });

        public RelayCommand AddConActionCommand => new RelayCommand(() =>
        {
            ConditionalAction conditionalAction = new ConditionalAction();

            Window window = ConditionalActionEditorViewModel.GetWindow(Settings, conditionalAction);

            if (window == null)
            {
                return;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return;
            }

            Settings.ConditionalActions.Add(conditionalAction);
            Settings.ConditionalActions = Settings.ConditionalActions.OrderBy(x => x.Name).ToObservable();
        });

        public RelayCommand AddExistingDefaultCategoriesCommand =>
            new RelayCommand(() => Settings.DefaultCategories.AddItems(FieldType.Category));

        public RelayCommand AddExistingDefaultTagsCommand =>
            new RelayCommand(() => Settings.DefaultTags.AddItems(FieldType.Tag));

        public RelayCommand AddNewDefaultCategoryCommand
            => new RelayCommand(() => Settings.DefaultCategories.AddNewItem(FieldType.Category, "", false));

        public RelayCommand AddNewDefaultTagCommand
            => new RelayCommand(() => Settings.DefaultTags.AddNewItem(FieldType.Tag, "", false));

        public RelayCommand AddNewMergeRuleCommand => new RelayCommand(() => EditMergeRule());

        public RelayCommand<object> AddNewMergeSourceCommand => new RelayCommand<object>(rule =>
        {
            FieldType type = FieldType.Tag;
            string prefix = string.Empty;

            if (SourceObjectsViewSource.View?.CurrentItem != null)
            {
                MetadataObject templateItem = (MetadataObject)SourceObjectsViewSource.View.CurrentItem;

                type = templateItem.Type;
                prefix = templateItem.Prefix;
            }

            MetadataObject newItem = ((MergeRule)rule).SourceObjects.AddNewItem(type, prefix);

            if (newItem != null)
            {
                SourceObjectsViewSource.View?.MoveCurrentTo(newItem);
            }
        }, rule => rule != null);

        public RelayCommand AddNewUnusedCommand => new RelayCommand(() => Settings.UnusedItemsWhiteList.AddNewItem(FieldType.Tag));

        public RelayCommand AddNewUnwantedCommand => new RelayCommand(() => Settings.UnwantedItems.AddNewItem(FieldType.Tag));

        public RelayCommand AddPrefixCommand
            => new RelayCommand(() =>
            {
                StringSelectionDialogResult res = API.Instance.Dialogs.SelectString(ResourceProvider.GetString("LOCAddNewItem"), ResourceProvider.GetString("LOCMetadataUtilitiesName"), "");

                if (!res.Result)
                {
                    return;
                }

                Settings.Prefixes.AddMissing(res.SelectedString);
                Settings.Prefixes = new ObservableCollection<string>(Settings.Prefixes.OrderBy(x => x));
            });

        public RelayCommand<FieldType> AddQuickAddCommand
            => new RelayCommand<FieldType>(AddQuickAddItems);

        public RelayCommand<FieldType> AddUnusedCommand
            => new RelayCommand<FieldType>(type => Settings.UnusedItemsWhiteList.AddItems(type));

        public RelayCommand<FieldType> AddUnwantedCommand
            => new RelayCommand<FieldType>(type => Settings.UnwantedItems.AddItems(type));

        public RelayCommand<object> EditConActionCommand => new RelayCommand<object>(conAction =>
        {
            if (conAction == null)
                return;

            ConditionalAction conditionalActionToEdit = new ConditionalAction();
            ConditionalAction conditionalActionOriginal = (ConditionalAction)conAction;

            conditionalActionToEdit.Name = conditionalActionOriginal.Name;
            conditionalActionToEdit.Type = conditionalActionOriginal.Type;
            conditionalActionToEdit.CanBeExecutedManually = conditionalActionOriginal.CanBeExecutedManually;
            conditionalActionToEdit.IgnoreConditionOnManual = conditionalActionOriginal.IgnoreConditionOnManual;
            conditionalActionToEdit.Enabled = conditionalActionOriginal.Enabled;
            conditionalActionToEdit.SortNo = conditionalActionOriginal.SortNo;

            foreach (Condition condition in conditionalActionOriginal.Conditions)
            {
                conditionalActionToEdit.Conditions.Add(new Condition(_settings)
                {
                    Name = condition.Name,
                    Type = condition.Type,
                    Comparator = condition.Comparator,
                    DateValue = condition.DateValue,
                    IntValue = condition.IntValue
                });
            }

            foreach (Action action in conditionalActionOriginal.Actions)
            {
                conditionalActionToEdit.Actions.Add(new Action(_settings)
                {
                    Name = action.Name,
                    Type = action.Type,
                    ActionType = action.ActionType,
                    DateValue = action.DateValue,
                    IntValue = action.IntValue
                });
            }

            Window window = ConditionalActionEditorViewModel.GetWindow(Settings, conditionalActionToEdit);

            if (window == null)
            {
                return;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return;
            }

            Settings.ConditionalActions.Remove(conditionalActionOriginal);
            Settings.ConditionalActions.Add(conditionalActionToEdit);

            Settings.ConditionalActions =
                Settings.ConditionalActions.OrderBy(x => x.SortNo).ThenBy(x => x.Name).ToObservable();
        }, conAction => conAction != null);

        public RelayCommand<object> EditMergeRuleCommand
            => new RelayCommand<object>(rule => EditMergeRule((MergeRule)rule), rule => rule != null);

        public ObservableCollection<FieldTypeContextAction> FieldTypeButtons { get; set; } =
                                                                                                                                                                                                                                                                            new ObservableCollection<FieldTypeContextAction>();

        public RelayCommand HelpMergingCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/merge-rules/")));

        public RelayCommand HelpPrefixesCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/other-functionality/#prefixes")));

        public RelayCommand HelpQuickAddCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/quick-add/")));

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

        public RelayCommand<IList<object>> RemoveConActionCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (ConditionalAction conAction in items.ToList().Cast<ConditionalAction>())
            {
                Settings.ConditionalActions.Remove(conAction);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveDefaultCategoryCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, Settings.DefaultCategories),
                items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveDefaultTagCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, Settings.DefaultTags),
                items => items?.Count != 0);

        public RelayCommand<object> RemoveMergeRuleCommand =>
            new RelayCommand<object>(rule => Settings.MergeRules.Remove((MergeRule)rule), rule => rule != null);

        public RelayCommand<IList<object>> RemoveMergeSourceCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, SelectedMergeRule.SourceObjects),
                items => items?.Count != 0);

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

        public RelayCommand<IList<object>> RemoveUnusedFromListCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, Settings.UnusedItemsWhiteList),
                items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveUnwantedFromListCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, Settings.UnwantedItems),
                items => items?.Count != 0);

        public MergeRule SelectedMergeRule
        {
            get => _selectedMergeRule;
            set
            {
                SetValue(ref _selectedMergeRule, value);

                SourceObjectsViewSource.Source = _selectedMergeRule == null
                    ? new MetadataObjects(Settings)
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

        public void AddQuickAddItems(FieldType type)
        {
            List<MetadataObject> items = MetadataFunctions.GetItemsFromAddDialog(type, Settings);

            if (items.Count == 0)
            {
                return;
            }

            AddItemsToQuickAddList(items);
        }

        public void BeginEdit()
        {
            MergeRules = Settings.MergeRules;
            MergeRuleViewSource.Source = MergeRules;
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;
            Settings.ResetReferences();
        }

        public void EndEdit() => _plugin.SavePluginSettings(Settings);

        public void RemoveFromList(IList<object> items, MetadataObjects list) => list.RemoveItems(items.ToList().Cast<MetadataObject>());

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

                MergeRuleEditorViewModel viewModel = new MergeRuleEditorViewModel(_plugin, metadataObjects)
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
                // exists => We ask if we should merge, replace or cancel.
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
    }
}