using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
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

        private ObservableCollection<FieldTypeMenuItem> _actionMenuItems = new ObservableCollection<FieldTypeMenuItem>();
        private ConditionalAction _conditionalAction;

        private ObservableCollection<FieldTypeMenuItem> _conditionMenuItems = new ObservableCollection<FieldTypeMenuItem>();

        public ConditionalActionEditorViewModel(ConditionalAction conditionalAction)
        {
            _conditionalAction = conditionalAction;

            ConditionMenuItems = new ObservableCollection<FieldTypeMenuItem>
            {
                new FieldTypeMenuItem(FieldType.Empty, ConditionCommand)
            };

            ConditionMenuItems.AddMissing(_fieldTypes
                .Select(x => new FieldTypeMenuItem(x.Type, ConditionCommand)));

            ActionMenuItems = new ObservableCollection<FieldTypeMenuItem>();

            ActionMenuItems.AddMissing(_fieldTypes
                .Where(x => x.CanBeSetInGame || x.CanBeClearedInGame)
                .Select(x => new FieldTypeMenuItem(x.Type, ActionCommand, false)));
        }

        public RelayCommand<FieldTypeContextItem> ActionCommand => new RelayCommand<FieldTypeContextItem>(type => AddActions(type));

        public ObservableCollection<FieldTypeMenuItem> ActionMenuItems
        {
            get => _actionMenuItems;
            set => SetValue(ref _actionMenuItems, value);
        }

        public ConditionalAction ConditionalAction
        {
            get => _conditionalAction;
            set => SetValue(ref _conditionalAction, value);
        }

        public RelayCommand<FieldTypeContextItem> ConditionCommand => new RelayCommand<FieldTypeContextItem>(type => AddConditions(type));

        public ObservableCollection<FieldTypeMenuItem> ConditionMenuItems
        {
            get => _conditionMenuItems;
            set => SetValue(ref _conditionMenuItems, value);
        }

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

            ControlCenter.Instance.Settings.ConditionActionWindowHeight = Convert.ToInt32(win.Height);
            ControlCenter.Instance.Settings.ConditionActionWindowWidth = Convert.ToInt32(win.Width);

            win.DialogResult = true;
            win.Close();
        });

        public static Window GetWindow(ConditionalAction conditionalAction)
        {
            try
            {
                var viewModel =
                    new ConditionalActionEditorViewModel(conditionalAction);

                var conditionalActionEditorView = new ConditionalActionEditorView();

                var window = WindowHelper.CreateSizedWindow(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogConditionalActionEditor"),
                    ControlCenter.Instance.Settings.ConditionActionWindowWidth, ControlCenter.Instance.Settings.ConditionActionWindowHeight);

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

        public void AddActions(FieldTypeContextItem contextItem)
        {
            var needsSorting = false;

            if (contextItem.ActionType == ActionType.ClearField)
            {
                needsSorting = CreateAction(contextItem);
            }
            else
            {
                switch (contextItem.FieldType.GetTypeManager().ValueType)
                {
                    case ItemValueType.ItemList:
                        needsSorting = CreateListAction(contextItem);
                        break;

                    case ItemValueType.Boolean:
                        needsSorting = CreateAction(contextItem);
                        break;

                    case ItemValueType.Integer:
                        needsSorting = CreateIntAction(contextItem);
                        break;

                    case ItemValueType.Date:
                        needsSorting = CreateDateAction(contextItem);
                        break;

                    case ItemValueType.Media:
                        needsSorting = CreateMediaAction(contextItem);
                        break;

                    case ItemValueType.String:
                        needsSorting = CreateStringAction(contextItem);
                        break;

                    case ItemValueType.Ulong:
                        needsSorting = CreateUlongAction(contextItem);
                        break;

                    case ItemValueType.None:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (needsSorting)
            {
                ConditionalAction.Actions.Sort(x => x.ToString);
            }
        }

        public void AddConditions(FieldTypeContextItem contextItem = null)
        {
            var needsSorting = false;

            if (contextItem.Comparator.IsOneOf(ComparatorType.IsEmpty, ComparatorType.IsNotEmpty, ComparatorType.GameIsNew))
            {
                needsSorting = CreateCondition(contextItem);
            }
            else
            {
                switch (contextItem.FieldType.GetTypeManager().ValueType)
                {
                    case ItemValueType.Integer:
                        needsSorting = CreateIntCondition(contextItem);
                        break;

                    case ItemValueType.Media:
                        switch (contextItem.ConditionPropertyType)
                        {
                            case ConditionPropertyType.Height:
                            case ConditionPropertyType.Width:
                                needsSorting = CreateIntCondition(contextItem, "px");
                                break;

                            case ConditionPropertyType.AspectRatio:
                                needsSorting = CreateAspectRatioCondition(contextItem);
                                break;

                            case ConditionPropertyType.FileSize:
                                needsSorting = CreateIntCondition(contextItem, "KB");
                                break;

                            case ConditionPropertyType.Extension:
                                needsSorting = CreateStringCondition(contextItem);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;

                    case ItemValueType.String:
                        needsSorting = CreateStringCondition(contextItem);
                        break;

                    case ItemValueType.Date:
                        needsSorting = CreateDateCondition(contextItem);
                        break;

                    case ItemValueType.Ulong:
                        needsSorting = CreateUlongCondition(contextItem);
                        break;

                    case ItemValueType.ItemList:
                        needsSorting = CreateListCondition(contextItem);
                        break;
                }
            }

            if (needsSorting)
            {
                ConditionalAction.Conditions.Sort(x => x.ToString);
            }
        }

        private bool CreateAction(FieldTypeContextItem contextItem)
        {
            if (!ConditionalAction.Actions.Any(x => x.ActionType == contextItem.ActionType && x.Type == contextItem.FieldType))
            {
                ConditionalAction.Actions.Add(new Action(contextItem.FieldType)
                {
                    ActionType = contextItem.ActionType
                });

                return true;
            }

            return false;
        }

        private bool CreateAspectRatioCondition(FieldTypeContextItem contextItem)
        {
            var intXValue = 0;
            var intYValue = 0;

            if (!SelectAspectRatioViewModel.ShowDialog(ref intXValue, ref intYValue))
            {
                return false;
            }

            var ratio = $"{intXValue}:{intYValue}";

            if (!ConditionalAction.Conditions.Any(
                x => x.Comparator == contextItem.Comparator &&
                x.ConditionPropertyType == contextItem.ConditionPropertyType &&
                x.Type == contextItem.FieldType &&
                x.StringValue == ratio))
            {
                ConditionalAction.Conditions.Add(new Condition(contextItem.FieldType)
                {
                    StringValue = ratio,
                    Comparator = contextItem.Comparator,
                    ConditionPropertyType = contextItem.ConditionPropertyType
                });

                return true;
            }

            return false;
        }

        private bool CreateCondition(FieldTypeContextItem contextItem)
        {
            if (!ConditionalAction.Conditions.Any(x => x.Comparator == contextItem.Comparator && x.Type == contextItem.FieldType))
            {
                ConditionalAction.Conditions.Add(new Condition(contextItem.FieldType)
                {
                    Comparator = contextItem.Comparator
                });

                return true;
            }

            return false;
        }

        private bool CreateDateAction(FieldTypeContextItem contextItem)
        {
            var dateValue = DateTime.Today;

            if (!SelectDateViewModel.ShowDialog(ref dateValue))
            {
                return false;
            }

            if (!ConditionalAction.Actions.Any(
                    x => x.ActionType == contextItem.ActionType &&
                         x.Type == contextItem.FieldType && x.DateValue == dateValue))
            {
                ConditionalAction.Actions.Add(new Action(contextItem.FieldType)
                {
                    DateValue = dateValue,
                    ActionType = contextItem.ActionType
                });

                return true;
            }

            return false;
        }

        private bool CreateDateCondition(FieldTypeContextItem contextItem)
        {
            var dateValue = DateTime.Today;

            if (!SelectDateViewModel.ShowDialog(ref dateValue))
            {
                return false;
            }

            if (!ConditionalAction.Conditions.Any(
                x => x.Comparator == contextItem.Comparator &&
                x.ConditionPropertyType == contextItem.ConditionPropertyType &&
                x.Type == contextItem.FieldType &&
                x.DateValue == dateValue))
            {
                ConditionalAction.Conditions.Add(new Condition(contextItem.FieldType)
                {
                    DateValue = dateValue,
                    Comparator = contextItem.Comparator,
                    ConditionPropertyType = contextItem.ConditionPropertyType
                });

                return true;
            }

            return false;
        }

        private bool CreateIntAction(FieldTypeContextItem contextItem)
        {
            var intValue = 0;

            if (!SelectIntViewModel.ShowDialog(ref intValue))
            {
                return false;
            }

            if (!ConditionalAction.Actions.Any(
                    x => x.ActionType == contextItem.ActionType &&
                         x.Type == contextItem.FieldType && x.IntValue == intValue))
            {
                ConditionalAction.Actions.Add(new Action(contextItem.FieldType)
                {
                    IntValue = intValue,
                    ActionType = contextItem.ActionType
                });

                return true;
            }

            return false;
        }

        private bool CreateIntCondition(FieldTypeContextItem contextItem, string unit = null)
        {
            var intValue = 0;

            if (!SelectIntViewModel.ShowDialog(ref intValue, unit))
            {
                return false;
            }

            if (!ConditionalAction.Conditions.Any(
                x => x.Comparator == contextItem.Comparator &&
                x.ConditionPropertyType == contextItem.ConditionPropertyType &&
                x.Type == contextItem.FieldType &&
                x.IntValue == intValue))
            {
                ConditionalAction.Conditions.Add(new Condition(contextItem.FieldType)
                {
                    IntValue = intValue,
                    Comparator = contextItem.Comparator,
                    ConditionPropertyType = contextItem.ConditionPropertyType
                });

                return true;
            }

            return false;
        }

        private bool CreateListAction(FieldTypeContextItem contextItem)
        {
            var items = ControlCenter.GetItemsFromAddDialog(contextItem.FieldType);

            if (items.Count == 0)
            {
                return false;
            }

            var addedItems = false;

            foreach (var item in items.Where(item =>
                         ConditionalAction.Actions.All(x =>
                             x.TypeAndName != item.TypeAndName || x.ActionType != contextItem.ActionType)))
            {
                ConditionalAction.Actions.Add(new Action(item.Type, item.Name)
                {
                    ActionType = contextItem.ActionType
                });

                addedItems = true;
            }

            return addedItems;
        }

        private bool CreateListCondition(FieldTypeContextItem contextItem)
        {
            var items = ControlCenter.GetItemsFromAddDialog(contextItem.FieldType);

            if (items.Count == 0)
            {
                return false;
            }

            var addedItems = false;

            foreach (var item in items.Where(item =>
                ConditionalAction.Conditions.All(x =>
                    x.TypeAndName != item.TypeAndName ||
                    x.Comparator != contextItem.Comparator ||
                    x.ConditionPropertyType != contextItem.ConditionPropertyType)))
            {
                ConditionalAction.Conditions.Add(new Condition(item.Type, item.Name)
                {
                    Comparator = contextItem.Comparator,
                    ConditionPropertyType = contextItem.ConditionPropertyType
                });

                addedItems = true;
            }

            return addedItems;
        }

        private bool CreateMediaAction(FieldTypeContextItem contextItem)
        {
            var mediaPath = API.Instance.Dialogs.SelectImagefile();

            if (!mediaPath.Any())
            {
                return false;
            }

            if (!ConditionalAction.Actions.Any(
                    x => x.ActionType == contextItem.ActionType &&
                         x.Type == contextItem.FieldType && x.StringValue == mediaPath))
            {
                ConditionalAction.Actions.Add(new Action(contextItem.FieldType)
                {
                    StringValue = mediaPath,
                    ActionType = contextItem.ActionType
                });

                return true;
            }

            return false;
        }

        private bool CreateStringAction(FieldTypeContextItem contextItem)
        {
            var dialogResult = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCMetadataUtilitiesDialogEnterValue"), default);

            if (!dialogResult.Result)
            {
                return false;
            }

            var stringValue = dialogResult.SelectedString;

            if (!stringValue.Any())
            {
                return false;
            }

            if (!ConditionalAction.Actions.Any(
                    x => x.ActionType == contextItem.ActionType &&
                         x.Type == contextItem.FieldType && x.StringValue == stringValue))
            {
                ConditionalAction.Actions.Add(new Action(contextItem.FieldType)
                {
                    StringValue = stringValue,
                    ActionType = contextItem.ActionType
                });

                return true;
            }

            return false;
        }

        private bool CreateStringCondition(FieldTypeContextItem contextItem)
        {
            var dialogResult = API.Instance.Dialogs.SelectString(
                ResourceProvider.GetString("LOCMetadataUtilitiesDialogRegExNotice"),
                ResourceProvider.GetString("LOCMetadataUtilitiesDialogEnterValue"), default);

            if (!dialogResult.Result)
            {
                return false;
            }

            if (!ConditionalAction.Conditions.Any(
                x => x.Comparator == contextItem.Comparator &&
                x.ConditionPropertyType == contextItem.ConditionPropertyType &&
                x.Type == contextItem.FieldType &&
                x.StringValue == dialogResult.SelectedString))
            {
                ConditionalAction.Conditions.Add(new Condition(contextItem.FieldType)
                {
                    StringValue = dialogResult.SelectedString,
                    Comparator = contextItem.Comparator,
                    ConditionPropertyType = contextItem.ConditionPropertyType
                });

                return true;
            }

            return false;
        }

        private bool CreateUlongAction(FieldTypeContextItem contextItem)
        {
            var ulongValue = 0;

            if (!SelectIntViewModel.ShowDialog(ref ulongValue))
            {
                return false;
            }

            if (!ConditionalAction.Actions.Any(
                    x => x.ActionType == contextItem.ActionType &&
                         x.Type == contextItem.FieldType && x.UlongValue == (ulong)ulongValue))
            {
                ConditionalAction.Actions.Add(new Action(contextItem.FieldType)
                {
                    UlongValue = (ulong)ulongValue,
                    ActionType = contextItem.ActionType
                });

                return true;
            }

            return false;
        }

        private bool CreateUlongCondition(FieldTypeContextItem contextItem)
        {
            var ulongValue = 0;

            if (!SelectIntViewModel.ShowDialog(ref ulongValue))
            {
                return false;
            }

            if (!ConditionalAction.Conditions.Any(
                x => x.Comparator == contextItem.Comparator &&
                x.ConditionPropertyType == contextItem.ConditionPropertyType &&
                x.Type == contextItem.FieldType &&
                x.UlongValue == (ulong)ulongValue))
            {
                ConditionalAction.Conditions.Add(new Condition(contextItem.FieldType)
                {
                    UlongValue = (ulong)ulongValue,
                    Comparator = contextItem.Comparator,
                    ConditionPropertyType = contextItem.ConditionPropertyType
                });

                return true;
            }

            return false;
        }
    }
}
