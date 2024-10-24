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
        private string _icon;
        private MetadataObjects _items;
        private string _name;
        private int _position;
        private string _prefix;

        public PrefixItemList(Game game, FieldType fieldType, string icon)
        {
            _fieldType = fieldType;
            _icon = icon;
            _name = fieldType.GetTypeManager().LabelPlural;
            _prefix = default;

            PrepareData(game);

            _name = _items.Count > 1 ? fieldType.GetTypeManager().LabelPlural : fieldType.GetTypeManager().LabelSingular;
        }

        public PrefixItemList(Game game, PrefixItemList itemList)
        {
            _fieldType = itemList.FieldType;
            _icon = itemList.Icon;
            _name = itemList.Name;
            _prefix = itemList.Prefix;
            _position = itemList.Position;

            PrepareData(game);
        }

        public PrefixItemList()
        {
        }

        public FieldType FieldType
        {
            get => _fieldType;
            set => SetValue(ref _fieldType, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetValue(ref _icon, value);
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

        public int Position
        {
            get => _position;
            set => SetValue(ref _position, value);
        }

        public string Prefix
        {
            get => _prefix;
            set => SetValue(ref _prefix, value);
        }

        public void PrepareData(Game game)
        {
            _items = new MetadataObjects();

            if (!(_fieldType.GetTypeManager() is IEditableObjectType type))
            {
                return;
            }

            var prefixes = new List<string> { _prefix };

            if (_prefix == default)
            {
                prefixes.Add(string.Empty);
                prefixes.AddMissing(ControlCenter.Instance.Settings.PrefixItemTypes.Where(p => p.Name == default).Select(p => p.Prefix));
            }

            _items.AddMissing(type.LoadGameMetadata(game)
                .Where(x => !ControlCenter.Instance.Settings.UnusedItemsWhiteList.Any(y => y.HideInDetails && y.Type == type.Type && y.Name == x.Name))
                .Select(x =>
                    new MetadataObject(type.Type, x.Name)
                    {
                        Id = x.Id
                    }).Where(x => prefixes.Contains(x.Prefix)).OrderBy(x => x.DisplayName));
        }
    }
}
