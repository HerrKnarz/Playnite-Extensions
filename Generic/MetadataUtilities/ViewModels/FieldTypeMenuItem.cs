using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MetadataUtilities.ViewModels
{
    public class FieldTypeMenuItem : ObservableObject
    {
        private readonly RelayCommand<FieldTypeContextItem> _command;
        private readonly IMetadataFieldType _field;
        private ObservableCollection<FieldTypeContextItem> _contextActions = new ObservableCollection<FieldTypeContextItem>();
        private FieldType _fieldType;

        public FieldTypeMenuItem(FieldType fieldType, RelayCommand<FieldTypeContextItem> command, bool isCondition = true)
        {
            _command = command;
            FieldType = fieldType;
            ContextActions = new ObservableCollection<FieldTypeContextItem>();

            _field = FieldTypeHelper.GetTypeManager(fieldType);

            var isList = _field is BaseObjectType objectType && objectType.IsList;

            if (_field is null)
            {
                ContextActions.Add(
                    new FieldTypeContextItem
                    {
                        Name = ResourceProvider.GetString("LOCMetadataUtilitiesGameIsNew"),
                        Command = command,
                        Comparator = ComparatorType.GameIsNew,
                        FieldType = FieldType.Empty
                    }
                );

                return;
            }

            if (isCondition)
            {
                if (_field.ValueType == ItemValueType.Media)
                {
                    void AddActions(ConditionPropertyType propertyType, bool isNumber = true)
                    {
                        AddContextAction(ComparatorType.Equals, propertyType);
                        AddContextAction(ComparatorType.DoesntEqual, propertyType);

                        if (isNumber)
                        {
                            AddContextAction(ComparatorType.IsBiggerThan, propertyType);
                            AddContextAction(ComparatorType.IsSmallerThan, propertyType);
                        }

                        AddSeparator();
                    }

                    AddActions(ConditionPropertyType.Width);
                    AddActions(ConditionPropertyType.Height);
                    AddActions(ConditionPropertyType.AspectRatio);
                    AddActions(ConditionPropertyType.FileSize);
                    AddActions(ConditionPropertyType.Extension, false);
                }
                else
                {
                    if (_field.ValueType == ItemValueType.ItemList && isList)
                    {
                        AddContextAction(ComparatorType.Contains);
                        AddContextAction(ComparatorType.DoesNotContain);
                    }

                    if (_field.ValueType.IsOneOf(ItemValueType.ItemList, ItemValueType.String, ItemValueType.Integer, ItemValueType.Date, ItemValueType.Ulong) && !isList)
                    {
                        AddContextAction(ComparatorType.Equals);
                        AddContextAction(ComparatorType.DoesntEqual);
                    }

                    if (_field is INumberType)
                    {
                        AddContextAction(ComparatorType.IsSmallerThan);
                        AddContextAction(ComparatorType.IsBiggerThan);
                    }
                }

                if (_field.CanBeEmptyInGame)
                {
                    AddContextAction(ComparatorType.IsEmpty);
                    AddContextAction(ComparatorType.IsNotEmpty);
                }
            }
            else
            {
                if (_field.CanBeSetInGame && _field.ValueType != ItemValueType.LinkList)
                {
                    if (_field.ValueType == ItemValueType.ItemList && isList)
                    {
                        AddContextAction(ActionType.AddObject);
                    }
                    else
                    {
                        AddContextAction(ActionType.Set);
                    }
                }

                if (_field.CanBeSetInGame && _field.CanBeClearedInGame && _field.ValueType == ItemValueType.ItemList && isList)
                {
                    AddContextAction(ActionType.RemoveObject);
                }

                if (_field.CanBeClearedInGame)
                {
                    AddContextAction(ActionType.ClearField);
                }
            }
        }

        public ObservableCollection<FieldTypeContextItem> ContextActions
        {
            get => _contextActions;
            set => SetValue(ref _contextActions, value);
        }

        public FieldType FieldType
        {
            get => _fieldType;
            set => SetValue(ref _fieldType, value);
        }

        public string Name => _field?.LabelInGame ?? ResourceProvider.GetString("LOCMetadataUtilitiesSettingsTabGeneral");

        private void AddContextAction(ComparatorType comparatorType = ComparatorType.None, ConditionPropertyType conditionPropertyType = ConditionPropertyType.Value)
        {
            ContextActions.Add(new FieldTypeContextItem()
            {
                Comparator = comparatorType,
                ConditionPropertyType = conditionPropertyType,
                Command = _command,
                FieldType = FieldType,
                Name = conditionPropertyType != ConditionPropertyType.Value
                    ? $"{conditionPropertyType.GetEnumDisplayNameWithType()} {comparatorType.GetEnumDisplayName()}"
                    : comparatorType.GetEnumDisplayName()
            });
        }

        private void AddContextAction(ActionType actionType = ActionType.None)
        {
            ContextActions.Add(new FieldTypeContextItem()
            {
                ActionType = actionType,
                Command = _command,
                FieldType = FieldType,
                Name = actionType.GetEnumDisplayName()
            });
        }

        private void AddSeparator()
        {
            ContextActions.Add(new FieldTypeContextItem()
            {
                IsSeparator = true
            });
        }
    }
}
