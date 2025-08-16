using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class CopyDataSet : ObservableObject
    {
        private ObservableCollection<CopyField> _fields = new ObservableCollection<CopyField>();

        public CopyDataSet(Game sourceGame, params CopyField[] metadataFields)
        {
            SourceGame = sourceGame;
            _fields.AddMissing(metadataFields);
        }

        public CopyDataSet(Game sourceGame)
        {
            SourceGame = sourceGame;

            foreach (var fieldType in FieldTypeHelper.GetAllTypes().Where(x => x.CanBeSetInGame))
            {
                _fields.Add(new CopyField(fieldType.Type));
            }
        }

        public bool CopyToGame(Game targetGame)
        {
            var mustUpdate = false;

            foreach (var field in Fields.Where(x => x.CopyData))
            {
                var fieldType = field.FieldType.GetTypeManager();

                switch (fieldType.ValueType)
                {
                    case ItemValueType.Integer:
                        if (!field.ReplaceData && fieldType is BaseIntegerType intType)
                        {
                            mustUpdate |= intType.AddValueToGame(targetGame, intType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.Date:
                        if (!field.ReplaceData && fieldType is BaseDateType dateType)
                        {
                            mustUpdate |= dateType.AddValueToGame(targetGame, dateType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.Boolean:
                        if (!field.ReplaceData && fieldType is BaseBooleanType boolType)
                        {
                            mustUpdate |= boolType.AddValueToGame(targetGame, boolType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.String:
                        if (!field.ReplaceData && fieldType is BaseStringType stringType)
                        {
                            mustUpdate |= stringType.AddValueToGame(targetGame, stringType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.ItemList:
                    case ItemValueType.Media:
                    case ItemValueType.None:
                    default:
                        if (fieldType is IEditableObjectType type)
                        {
                            IClearAbleType clearableType = null;

                            if (fieldType is IClearAbleType)
                            {
                                clearableType = fieldType as IClearAbleType;
                            }

                            if (field.ReplaceData && clearableType != null)
                            {
                                clearableType.EmptyFieldInGame(targetGame);
                            }

                            if (type.IsList)
                            {
                                mustUpdate |= type.AddValueToGame(targetGame, type.LoadGameMetadata(SourceGame).Select(x => x.Id).ToList());
                            }
                            else
                            if (field.ReplaceData || (clearableType != null && clearableType.FieldInGameIsEmpty(targetGame)))
                            {
                                mustUpdate |= type.AddValueToGame(targetGame, type.LoadGameMetadata(SourceGame).Select(x => x.Id).FirstOrDefault());
                            }
                        }

                        break;
                }
            }

            return mustUpdate;
        }

        [DontSerialize]
        public Game SourceGame { get; set; }

        public ObservableCollection<CopyField> Fields
        {
            get => _fields;
            set => SetValue(ref _fields, value);
        }
    }
}
