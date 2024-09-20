using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Action = MetadataUtilities.Models.Action;
using Condition = MetadataUtilities.Models.Condition;

namespace MetadataUtilities.ViewModels
{
    public class ConditionalActionEditorViewModel : ObservableObject
    {
        private readonly List<IMetadataFieldType> _fieldTypes = FieldTypeHelper.GetAllTypes();
        private readonly Settings _settings;
        private ConditionalAction _conditionalAction;

        public ConditionalActionEditorViewModel(Settings settings, ConditionalAction conditionalAction)
        {
            _settings = settings;
            _conditionalAction = conditionalAction;

            ContextMenuActionsAdd.AddMissing(_fieldTypes
                .Where(x => x.CanBeSetInGame)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddActionAddCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuActionsRemove.AddMissing(_fieldTypes
                .Where(x => x.CanBeSetInGame && x.CanBeClearedInGame && x.ValueType == ItemValueType.ItemList)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddActionRemoveCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuActionsClear.AddMissing(_fieldTypes.Where(x => x.CanBeClearedInGame)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddActionClearCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuConditionsContains.AddMissing(_fieldTypes
                .Where(x => x.ValueType == ItemValueType.ItemList || x.ValueType == ItemValueType.String ||
                            x.ValueType == ItemValueType.Integer || x.ValueType == ItemValueType.Date)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddConditionContainsCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuConditionsContainsNot.AddMissing(_fieldTypes
                .Where(x => x.ValueType == ItemValueType.ItemList || x.ValueType == ItemValueType.String ||
                            x.ValueType == ItemValueType.Integer || x.ValueType == ItemValueType.Date)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddConditionContainsNotCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuConditionsEmpty.AddMissing(_fieldTypes.Where(x => x.CanBeEmptyInGame)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddConditionIsEmptyCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuConditionsNotEmpty.AddMissing(_fieldTypes.Where(x => x.CanBeEmptyInGame)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddConditionIsNotEmptyCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuConditionsBiggerThan.AddMissing(_fieldTypes
                .Where(x => x.ValueType == ItemValueType.Date || x.ValueType == ItemValueType.Integer)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddConditionIsBiggerThanCommand,
                        FieldType = x.Type
                    }
                ));

            ContextMenuConditionsSmallerThan.AddMissing(_fieldTypes
                .Where(x => x.ValueType == ItemValueType.Date || x.ValueType == ItemValueType.Integer)
                .Select(x =>
                    new FieldTypeContextAction
                    {
                        Name = x.LabelSingular,
                        Action = AddConditionIsSmallerThanCommand,
                        FieldType = x.Type
                    }
                ));
        }

        public RelayCommand<FieldType> AddActionAddCommand => new RelayCommand<FieldType>(type =>
            AddActions(type, ActionType.AddObject));

        public RelayCommand<FieldType> AddActionClearCommand => new RelayCommand<FieldType>(type =>
            AddActions(type, ActionType.ClearField));

        public RelayCommand<FieldType> AddActionRemoveCommand => new RelayCommand<FieldType>(type =>
            AddActions(type, ActionType.RemoveObject));

        public RelayCommand<FieldType> AddConditionContainsCommand => new RelayCommand<FieldType>(type =>
            AddConditions(type, ComparatorType.Contains));

        public RelayCommand<FieldType> AddConditionContainsNotCommand => new RelayCommand<FieldType>(type =>
            AddConditions(type, ComparatorType.DoesNotContain));

        public RelayCommand<FieldType> AddConditionIsBiggerThanCommand => new RelayCommand<FieldType>(type =>
            AddConditions(type, ComparatorType.IsBiggerThan));

        public RelayCommand<FieldType> AddConditionIsEmptyCommand => new RelayCommand<FieldType>(type =>
            AddConditions(type, ComparatorType.IsEmpty));

        public RelayCommand<FieldType> AddConditionIsNotEmptyCommand => new RelayCommand<FieldType>(type =>
            AddConditions(type, ComparatorType.IsNotEmpty));

        public RelayCommand<FieldType> AddConditionIsSmallerThanCommand => new RelayCommand<FieldType>(type =>
            AddConditions(type, ComparatorType.IsSmallerThan));

        public ConditionalAction ConditionalAction
        {
            get => _conditionalAction;
            set => SetValue(ref _conditionalAction, value);
        }

        public ObservableCollection<FieldTypeContextAction> ContextMenuActionsAdd { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuActionsClear { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuActionsRemove { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuConditionsBiggerThan { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuConditionsContains { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuConditionsContainsNot { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuConditionsEmpty { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuConditionsNotEmpty { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public ObservableCollection<FieldTypeContextAction> ContextMenuConditionsSmallerThan { get; set; } =
            new ObservableCollection<FieldTypeContextAction>();

        public RelayCommand<IList<object>> RemoveActionCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<Action>())
            {
                ConditionalAction.Actions.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveConditionCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (var item in items.ToList().Cast<Condition>())
            {
                ConditionalAction.Conditions.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<Window> SaveCommand => new RelayCommand<Window>(win =>
        {
            if (ConditionalAction.Name == null || ConditionalAction.Name?.Length == 0)
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoNameSet"),
                    string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ConditionalAction.Actions.Count == 0)
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoActionsSet"),
                    string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ConditionalAction.Enabled && ConditionalAction.Conditions.Count == 0)
            {
                if (API.Instance.Dialogs.ShowMessage(
                        ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoConditionsSet"), string.Empty,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }

            win.DialogResult = true;
            win.Close();
        });

        public static Window GetWindow(Settings settings, ConditionalAction conditionalAction)
        {
            try
            {
                var viewModel =
                    new ConditionalActionEditorViewModel(settings, conditionalAction);

                var conditionalActionEditorView = new ConditionalActionEditorView();

                var window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogConditionalActionEditor"), 800, 500);
                window.Content = conditionalActionEditorView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing edit conditional action dialog", true);

                return null;
            }
        }

        public void AddActions(FieldType fieldType, ActionType actionType)
        {
            if (actionType == ActionType.ClearField)
            {
                if (!ConditionalAction.Actions.Any(x => x.ActionType == actionType && x.Type == fieldType))
                {
                    ConditionalAction.Actions.Add(new Action(_settings)
                    {
                        Name = string.Empty,
                        Type = fieldType,
                        ActionType = actionType
                    });
                }

                return;
            }

            switch (fieldType.GetTypeManager().ValueType)
            {
                case ItemValueType.ItemList:
                    var items = MetadataFunctions.GetItemsFromAddDialog(fieldType, _settings);

                    if (items.Count == 0)
                    {
                        return;
                    }

                    foreach (var item in items.Where(item =>
                                 ConditionalAction.Actions.All(x =>
                                     x.TypeAndName != item.TypeAndName || x.ActionType != actionType)))
                    {
                        ConditionalAction.Actions.Add(new Action(_settings)
                        {
                            Name = item.Name,
                            Type = item.Type,
                            ActionType = actionType
                        });
                    }

                    break;

                case ItemValueType.Boolean:
                    if (!ConditionalAction.Actions.Any(
                            x => x.ActionType == actionType &&
                                 x.Type == fieldType))
                    {
                        ConditionalAction.Actions.Add(new Action(_settings)
                        {
                            Name = string.Empty,
                            Type = fieldType,
                            ActionType = actionType
                        });
                    }

                    break;

                case ItemValueType.Integer:
                    var intValue = 0;

                    if (!SelectIntViewModel.ShowDialog(ref intValue))
                    {
                        return;
                    }

                    if (!ConditionalAction.Actions.Any(
                            x => x.ActionType == actionType &&
                                 x.Type == fieldType && x.IntValue == intValue))
                    {
                        ConditionalAction.Actions.Add(new Action(_settings)
                        {
                            Name = string.Empty,
                            IntValue = intValue,
                            Type = fieldType,
                            ActionType = actionType
                        });
                    }

                    break;

                case ItemValueType.Date:
                    var dateValue = DateTime.Today;

                    if (!SelectDateViewModel.ShowDialog(ref dateValue))
                    {
                        return;
                    }

                    if (!ConditionalAction.Actions.Any(
                            x => x.ActionType == actionType &&
                                 x.Type == fieldType && x.DateValue == dateValue))
                    {
                        ConditionalAction.Actions.Add(new Action(_settings)
                        {
                            Name = string.Empty,
                            DateValue = dateValue,
                            Type = fieldType,
                            ActionType = actionType
                        });
                    }

                    break;

                case ItemValueType.Media:
                case ItemValueType.None:
                case ItemValueType.String:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ConditionalAction.Actions = ConditionalAction.Actions.OrderBy(x => x.ToString).ToObservable();
        }

        public void AddConditions(FieldType fieldType, ComparatorType comparatorType)
        {
            switch (comparatorType)
            {
                case ComparatorType.IsEmpty:
                case ComparatorType.IsNotEmpty:
                {
                    if (!ConditionalAction.Conditions.Any(x => x.Comparator == comparatorType && x.Type == fieldType))
                    {
                        ConditionalAction.Conditions.Add(new Condition(_settings)
                        {
                            Name = string.Empty,
                            Type = fieldType,
                            Comparator = comparatorType
                        });
                    }

                    return;
                }
                case ComparatorType.IsBiggerThan:
                case ComparatorType.IsSmallerThan:
                {
                    if (fieldType.GetTypeManager().ValueType == ItemValueType.Integer)
                    {
                        CreateIntCondition(fieldType, comparatorType);

                        return;
                    }

                    if (fieldType.GetTypeManager().ValueType != ItemValueType.Date)
                    {
                        return;
                    }

                    CreateDateCondition(fieldType, comparatorType);

                    return;
                }
                case ComparatorType.Contains:
                    break;

                case ComparatorType.DoesNotContain:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(comparatorType), comparatorType, null);
            }

            if (fieldType.GetTypeManager().ValueType == ItemValueType.Integer)
            {
                CreateIntCondition(fieldType, comparatorType);

                return;
            }

            if (fieldType.GetTypeManager().ValueType == ItemValueType.Date)
            {
                CreateDateCondition(fieldType, comparatorType);

                return;
            }

            if (fieldType.GetTypeManager().ValueType == ItemValueType.String)
            {
                CreateStringCondition(fieldType, comparatorType);

                return;
            }

            var items = MetadataFunctions.GetItemsFromAddDialog(fieldType, _settings);

            if (items.Count == 0)
            {
                return;
            }

            foreach (var item in items.Where(item =>
                         ConditionalAction.Conditions.All(x =>
                             x.TypeAndName != item.TypeAndName || x.Comparator != comparatorType)))
            {
                ConditionalAction.Conditions.Add(new Condition(_settings)
                {
                    Name = item.Name,
                    Type = item.Type,
                    Comparator = comparatorType
                });
            }

            ConditionalAction.Conditions = ConditionalAction.Conditions.OrderBy(x => x.ToString).ToObservable();
        }

        private void CreateDateCondition(FieldType fieldType, ComparatorType comparatorType)
        {
            var dateValue = DateTime.Today;

            if (!SelectDateViewModel.ShowDialog(ref dateValue))
            {
                return;
            }

            if (!ConditionalAction.Conditions.Any(
                    x => x.Comparator == comparatorType &&
                         x.Type == fieldType && x.DateValue == dateValue))
            {
                ConditionalAction.Conditions.Add(new Condition(_settings)
                {
                    Name = string.Empty,
                    DateValue = dateValue,
                    Type = fieldType,
                    Comparator = comparatorType
                });
            }
        }

        private void CreateIntCondition(FieldType fieldType, ComparatorType comparatorType)
        {
            var intValue = 0;

            if (!SelectIntViewModel.ShowDialog(ref intValue))
            {
                return;
            }

            if (!ConditionalAction.Conditions.Any(
                    x => x.Comparator == comparatorType &&
                         x.Type == fieldType && x.IntValue == intValue))
            {
                ConditionalAction.Conditions.Add(new Condition(_settings)
                {
                    Name = string.Empty,
                    IntValue = intValue,
                    Type = fieldType,
                    Comparator = comparatorType
                });
            }
        }

        private void CreateStringCondition(FieldType fieldType, ComparatorType comparatorType)
        {
            var dialogResult = API.Instance.Dialogs.SelectString(
                ResourceProvider.GetString("LOCMetadataUtilitiesDialogRegExNotice"),
                ResourceProvider.GetString("LOCMetadataUtilitiesDialogEnterValue"), default);

            if (!dialogResult.Result)
            {
                return;
            }

            if (!ConditionalAction.Conditions.Any(
                    x => x.Comparator == comparatorType &&
                         x.Type == fieldType && x.StringValue == dialogResult.SelectedString))
            {
                ConditionalAction.Conditions.Add(new Condition(_settings)
                {
                    Name = string.Empty,
                    StringValue = dialogResult.SelectedString,
                    Type = fieldType,
                    Comparator = comparatorType
                });
            }
        }
    }
}