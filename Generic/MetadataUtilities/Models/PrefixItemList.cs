using KNARZhelper;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class PrefixItemList : ObservableObject
    {
        private FieldType _fieldType;
        private MetadataObjects _items;
        private string _name;
        private string _prefix;

        public PrefixItemList(MetadataUtilities plugin, Game game, FieldType fieldType)
        {
            _fieldType = fieldType;

            _items = new MetadataObjects(plugin.Settings.Settings);

            if (!(fieldType.GetTypeManager() is IEditableObjectType type))
            {
                return;
            }

            _name = type.LabelPlural;

            _items.AddMissing(type.LoadGameMetadata(game).Select(x =>
                new MetadataObject(plugin.Settings.Settings)
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = type.Type
                }));
        }

        public FieldType FieldType
        {
            get => _fieldType;
            set => SetValue(ref _fieldType, value);
        }

        public MetadataObjects Items
        {
            get => _items;
            set => SetValue(ref _items, value);
        }

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public string Prefix
        {
            get => _prefix;
            set => SetValue(ref _prefix, value);
        }
    }
}
