using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class CopyDataSet
    {
        private List<IMetadataFieldType> _fieldTypes = new List<IMetadataFieldType>();

        public CopyDataSet(Game sourceGame, params IMetadataFieldType[] metadataFieldTypes)
        {
            SourceGame = sourceGame;
            _fieldTypes.AddRange(metadataFieldTypes);
        }

        public CopyDataSet(Game sourceGame)
        {
            SourceGame = sourceGame;
            _fieldTypes = FieldTypeHelper.GetAllTypes().Where(x => x.CanBeSetInGame).ToList();
        }

        public bool CopyToGame(Game targetGame, bool replaceData = false)
        {
            bool mustUpdate = false;

            foreach (IMetadataFieldType fieldType in FieldTypes)
            {
                // TODO: Use this function in an action to repeat it for all selected games
                switch (fieldType.ValueType)
                {
                    case ItemValueType.Integer:
                        if (!replaceData && fieldType is BaseIntegerType intType)
                        {
                            mustUpdate |= intType.AddValueToGame(targetGame, intType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.Date:
                        if (!replaceData && fieldType is BaseDateType dateType)
                        {
                            mustUpdate |= dateType.AddValueToGame(targetGame, dateType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.Boolean:
                        if (!replaceData && fieldType is BaseBooleanType boolType)
                        {
                            mustUpdate |= boolType.AddValueToGame(targetGame, boolType.GetValue(SourceGame));
                        }

                        break;

                    case ItemValueType.String:
                        if (!replaceData && fieldType is BaseStringType stringType)
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
                            if (replaceData && fieldType is IClearAbleType clearableType)
                            {
                                clearableType.EmptyFieldInGame(targetGame);
                            }

                            if (replaceData || type.IsList)
                            {
                                mustUpdate |= type.AddValueToGame(targetGame, type.LoadGameMetadata(SourceGame));
                            }
                        }

                        break;
                }
            }

            return mustUpdate;
        }

        public Game SourceGame { get; set; }

        public List<IMetadataFieldType> FieldTypes
        {
            get => _fieldTypes;
            set => _fieldTypes = value ?? new List<IMetadataFieldType>();
        }
    }
}
