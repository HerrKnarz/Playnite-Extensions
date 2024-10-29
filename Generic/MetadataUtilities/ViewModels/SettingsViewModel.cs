using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace MetadataUtilities.ViewModels
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly List<IMetadataFieldType> _allFieldTypes = FieldTypeHelper.GetAllTypes();
        private readonly Dictionary<FieldType, string> _fieldTypes = FieldTypeHelper.ItemListFieldValues();
        private readonly MetadataUtilities _plugin;
        private MergeRules _mergeRules;
        private CollectionViewSource _mergeRuleViewSource;
        private string _searchTerm = string.Empty;
        private MergeRule _selectedMergeRule;
        private Settings _settings;
        private CollectionViewSource _sourceObjectsViewSource;

        public SettingsViewModel(MetadataUtilities plugin)
        {
            _plugin = plugin;

            _mergeRules = Settings?.MergeRules;

            var savedSettings = plugin.LoadPluginSettings<Settings>();

            Settings = savedSettings ?? new Settings();

            if (Settings.PrefixItemTypes.Count == 0 && Settings.Prefixes.Count > 0)
            {
                Settings.PrefixItemTypes.AddMissing(Settings.Prefixes.Select(p => new PrefixItemList { Prefix = p, FieldType = FieldType.Empty }));
            }

            Settings.SetMissingTypes();

            FieldTypeButtons.AddMissing(_fieldTypes
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.Value,
                        FieldType = x.Key
                    }
                ));

            FieldTypeButtonsExtended.AddMissing(_allFieldTypes.Where(x => x.CanBeSetInGame && x.CanBeClearedInGame && x.ValueType == ItemValueType.ItemList)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelPlural,
                        FieldType = x.Type
                    }
                ));

            FieldTypeButtonsUnwanted.AddMissing(_allFieldTypes.Where(x => x.CanBeSetInGame && x.CanBeClearedInGame && x.CanBeSetByMetadataAddOn && x.ValueType == ItemValueType.ItemList)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelPlural,
                        FieldType = x.Type
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

        public RelayCommand AddConActionCommand => new RelayCommand(() =>
        {
            var conditionalAction = new ConditionalAction();

            var window = ConditionalActionEditorViewModel.GetWindow(conditionalAction);

            if (window == null)
            {
                return;
            }

            if (!(window.ShowDialog() ?? false))
            {
                return;
            }

            Settings.ConditionalActions.Add(conditionalAction);
            Settings.ConditionalActions.Sort(x => x.Name);
        });

        public RelayCommand AddDefaultWhiteListItemsCommand => new RelayCommand(AddDefaultWhiteListItems);

        public RelayCommand AddNewMergeRuleCommand => new RelayCommand(() => EditMergeRule());

        public RelayCommand<object> AddNewMergeSourceCommand => new RelayCommand<object>(rule =>
        {
            var type = FieldType.Tag;
            var prefix = string.Empty;

            if (SourceObjectsViewSource.View?.CurrentItem != null)
            {
                var templateItem = (MetadataObject)SourceObjectsViewSource.View.CurrentItem;

                type = templateItem.Type;
                prefix = templateItem.Prefix;
            }

            var newItem = ((MergeRule)rule).SourceObjects.AddNewItem(type, prefix);

            if (newItem != null)
            {
                SourceObjectsViewSource.View?.MoveCurrentTo(newItem);
            }
        }, rule => rule != null);

        public RelayCommand AddNewUnusedCommand => new RelayCommand(() => AddNewWhiteListItem(FieldType.Tag));

        public RelayCommand AddNewUnwantedCommand => new RelayCommand(() => Settings.UnwantedItems.AddNewItem(FieldType.Tag));

        public RelayCommand AddPrefixCommand
            => new RelayCommand(() => Settings.PrefixItemTypes.Add(new PrefixItemList()));

        public RelayCommand<FieldType> AddQuickAddCommand
            => new RelayCommand<FieldType>(AddQuickAddItems);

        public RelayCommand<FieldType> AddUnusedCommand
            => new RelayCommand<FieldType>(AddWhiteListItems);

        public RelayCommand<FieldType> AddUnwantedCommand
            => new RelayCommand<FieldType>(type => Settings.UnwantedItems.AddItems(type));

        public RelayCommand<object> EditConActionCommand => new RelayCommand<object>(conAction =>
        {
            if (conAction == null)
                return;

            var conditionalActionOriginal = (ConditionalAction)conAction;
            var conditionalActionToEdit = conditionalActionOriginal.DeepClone();

            var window = ConditionalActionEditorViewModel.GetWindow(conditionalActionToEdit);

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

            Settings.ConditionalActions.Sort(x => x.Name);
        }, conAction => conAction != null);

        private Settings EditingClone { get; set; }

        public RelayCommand<object> EditMergeRuleCommand
            => new RelayCommand<object>(rule => EditMergeRule((MergeRule)rule), rule => rule != null);

        public ObservableCollection<FieldTypeContextAction> FieldTypeButtons { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> FieldTypeButtonsExtended { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> FieldTypeButtonsUnwanted { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public Dictionary<FieldType, string> FieldValuePairs => FieldTypeHelper.ItemListFieldValues(true);

        public RelayCommand HelpConActionCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/conditional-actions/")));

        public RelayCommand HelpMergingCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/merge-rules/")));

        public RelayCommand HelpPrefixesCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/prefixes")));

        public RelayCommand HelpQuickAddCommand
            => new RelayCommand(()
                => Process.Start(new ProcessStartInfo("https://knarzwerk.de/en/playnite-extensions/metadata-utilities/quick-add/")));

        public RelayCommand<object> MergeItemsCommand
                    => new RelayCommand<object>(rule => ((MergeRule)rule).MergeItems(), rule => rule != null);

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
            foreach (var conAction in items.ToList().Cast<ConditionalAction>())
            {
                Settings.ConditionalActions.Remove(conAction);
            }
        }, items => items?.Count != 0);

        public RelayCommand<object> RemoveMergeRuleCommand =>
            new RelayCommand<object>(rule => Settings.MergeRules.Remove((MergeRule)rule), rule => rule != null);

        public RelayCommand<IList<object>> RemoveMergeSourceCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, SelectedMergeRule.SourceObjects),
                items => items?.Count != 0);

        public RelayCommand<IList<object>> RemovePrefixCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<PrefixItemList>())
            {
                Settings.PrefixItemTypes.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveQuickAddFromListCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<QuickAddObject>())
            {
                Settings.QuickAddObjects.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveUnusedFromListCommand =>
            new RelayCommand<IList<object>>(items =>
                {
                    foreach (var item in items.ToList())
                    {
                        if (item is WhiteListItem whiteListItem)
                        {
                            Settings.UnusedItemsWhiteList.Remove(whiteListItem);
                        }
                    }
                },
                items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveUnwantedFromListCommand =>
            new RelayCommand<IList<object>>(items => RemoveFromList(items, Settings.UnwantedItems),
                items => items?.Count != 0);

        public static RelayCommand<object> RestartRequired => new RelayCommand<object>((sender) =>
                {
                    try
                    {
                        var winParent = MiscHelper.FindParent<Window>((FrameworkElement)sender);

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

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetValue(ref _searchTerm, value);
                MergeRuleViewSource.View.Filter = Filter;
            }
        }

        public MergeRule SelectedMergeRule
        {
            get => _selectedMergeRule;
            set
            {
                SetValue(ref _selectedMergeRule, value);

                SourceObjectsViewSource.Source = _selectedMergeRule == null
                    ? new MetadataObjects()
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

        public RelayCommand SortPrefixesCommand => new RelayCommand(() =>
            Settings.PrefixItemTypes =
                Settings.PrefixItemTypes.OrderBy(x => x.Position).ThenBy(x => x.Name).ToObservable());

        public CollectionViewSource SourceObjectsViewSource
        {
            get => _sourceObjectsViewSource;
            set
            {
                _sourceObjectsViewSource = value;
                OnPropertyChanged();
            }
        }

        public void BeginEdit()
        {
            MergeRules = Settings.MergeRules;
            MergeRuleViewSource.Source = MergeRules;

            if (MergeRuleViewSource.View != null)
            {
                MergeRuleViewSource.View.Filter = Filter;
            }

            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;
            Settings.SetMissingTypes();
        }

        public void EndEdit()
        {
            Settings.Prefixes.Clear();

            Settings.Prefixes.AddMissing(Settings.PrefixItemTypes.Select(p => p.Prefix).Distinct());

            _plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        public void AddDefaultWhiteListItems()
        {
            var defaults = Serialization.FromJsonFile<List<WhiteListItem>>(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Resources",
                "WhiteListDefaults.json"));

            if (defaults == null)
            {
                return;
            }

            foreach (var item in defaults.Where(item => Settings.UnusedItemsWhiteList.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Settings.UnusedItemsWhiteList.Add(item);
            }

            Settings.UnusedItemsWhiteList.Sort(x => x.TypeAndName);
        }

        public void AddItemsToQuickAddList(List<MetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (var item in items.Where(item => Settings.QuickAddObjects.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Settings.QuickAddObjects.Add(new QuickAddObject(item.Type, item.Name));
            }

            Settings.QuickAddObjects.Sort(x => x.TypeAndName);
        }

        public void AddItemsToWhiteList(List<MetadataObject> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            foreach (var item in items.Where(item => Settings.UnusedItemsWhiteList.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Settings.UnusedItemsWhiteList.Add(new WhiteListItem(item.Type, item.Name));
            }

            Settings.UnusedItemsWhiteList.Sort(x => x.TypeAndName);
        }

        public void AddNewWhiteListItem(FieldType type)
        {
            var newItem = ControlCenter.AddNewItem(type);

            if (Settings.UnusedItemsWhiteList.Any(x => x.TypeAndName == newItem.TypeAndName))
            {
                return;
            }

            Settings.UnusedItemsWhiteList.Add(new WhiteListItem(newItem.Type, newItem.Name));

            Settings.UnusedItemsWhiteList.Sort(x => x.TypeAndName);
        }

        public void AddQuickAddItems(FieldType type)
        {
            var items = ControlCenter.GetItemsFromAddDialog(type);

            if (items.Count == 0)
            {
                return;
            }

            AddItemsToQuickAddList(items);
        }

        public void AddWhiteListItems(FieldType type)
        {
            var items = ControlCenter.GetItemsFromAddDialog(type);

            if (items.Count == 0)
            {
                return;
            }

            AddItemsToWhiteList(items);
        }

        public void RemoveFromList(IList<object> items, MetadataObjects list) => list.RemoveItems(items.ToList().Cast<MetadataObject>());

        private void EditMergeRule(MergeRule rule = null)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var isNewRule = rule == null;

                var ruleToEdit = new MergeRule(FieldType.Category);

                if (rule != null)
                {
                    ruleToEdit.Type = rule.Type;
                    ruleToEdit.Name = rule.Name;

                    foreach (var sourceItem in rule.SourceObjects)
                    {
                        ruleToEdit.SourceObjects.Add(new MetadataObject(sourceItem.Type, sourceItem.Name)
                        {
                            GameCount = 0
                        });
                    }
                }

                var metadataObjects = new MetadataObjects();

                if (isNewRule)
                {
                    metadataObjects.LoadMetadata();
                }
                else
                {
                    var temp = new MetadataObjects();
                    temp.LoadMetadata();

                    foreach (var item in ruleToEdit.SourceObjects)
                    {
                        var foundItem = temp.FirstOrDefault(x => x.Name == item.Name && x.Type == item.Type);

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

                var viewModel = new MergeRuleEditorViewModel(metadataObjects)
                {
                    RuleName = ruleToEdit.Name,
                    RuleType = ruleToEdit.Type
                };

                var editorView = new MergeRuleEditorView();

                var window = WindowHelper.CreateSizedWindow(
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

                foreach (var item in metadataObjects.Where(x => x.Selected).ToList())
                {
                    ruleToEdit.SourceObjects.Add(new MetadataObject(item.Type, item.Name)
                    {
                        Id = item.Id,
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
                    var response = API.Instance.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergeOrReplace"),
                        ResourceProvider.GetString("LOCMetadataUtilitiesName"), MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);
                    Cursor.Current = Cursors.WaitCursor;

                    switch (response)
                    {
                        case MessageBoxResult.Yes:
                            Settings.MergeRules.AddRule(ruleToEdit, true);
                            Settings.MergeRules.Remove(rule);
                            MergeRuleViewSource.View.Filter = Filter;
                            MergeRuleViewSource.View.MoveCurrentTo(ruleToEdit);
                            return;

                        case MessageBoxResult.No:
                            Settings.MergeRules.AddRule(ruleToEdit);
                            Settings.MergeRules.Remove(rule);
                            MergeRuleViewSource.View.Filter = Filter;
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
                    MergeRuleViewSource.View.Filter = Filter;
                    MergeRuleViewSource.View.MoveCurrentTo(ruleToEdit);
                }

                // Case 4: The rule is new and no other with that target exists => we simply add the
                // new one.
                Settings.MergeRules.AddRule(ruleToEdit);
                MergeRuleViewSource.View.Filter = Filter;
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

        private bool Filter(object item) => item is MergeRule rule && (rule.Name.RegExIsMatch(SearchTerm) ||
                                                                       rule.SourceObjects.Any(x =>
                                                                           x.Name.RegExIsMatch(SearchTerm)));
    }
}