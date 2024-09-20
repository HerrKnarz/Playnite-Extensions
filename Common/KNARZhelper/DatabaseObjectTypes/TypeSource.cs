using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeSource : BaseSingleObjectType
    {
        private readonly IItemCollection<GameSource> _collection = API.Instance.Database.Sources;

        public TypeSource(bool adoptEvents = false)
        {
            if (!adoptEvents)
            {
                return;
            }

            API.Instance.Database.Sources.ItemUpdated += ItemUpdated;
        }

        public override event RenameObjectEventHandler RenameObject;

        public override bool CanBeSetByMetadataAddOn => false;

        public override int Count => _collection?.Count ?? 0;

        public override bool IsList => false;

        public override string LabelPlural => ResourceProvider.GetString("LOCSourcesLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCSourceLabel");

        public override FieldType Type => FieldType.Source;

        public override Guid AddDbObject(string name) => AddDbObject(name, _collection);

        public override bool DbObjectExists(string name) => DbObjectExists(name, _collection);

        public override bool DbObjectExists(Guid id) => DbObjectExists(id, _collection);

        public override Guid GetDbObjectId(string name) => GetDbObjectId(name, _collection);

        public override List<DatabaseObject> LoadAllMetadata() => LoadAllMetadata(_collection);

        public override List<DatabaseObject> LoadGameMetadata(Game game) => LoadGameMetadata(game.Source);

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => LoadUnusedMetadata(ignoreHiddenGames, _collection);

        public override bool NameExists(string name, Guid id) => NameExists(name, id, _collection);

        public override bool RemoveDbObject(Guid id, bool checkIfUsed = true) => RemoveDbObject(id, checkIfUsed, _collection);

        public override void UpdateDbObject(Guid id, string name) => UpdateDbObject(id, name, _collection);

        internal override Guid GetValue(Game game) => game.SourceId;

        internal override void SetValue(Game game, Guid id) => game.SourceId = id;

        private void ItemUpdated(object sender, ItemUpdatedEventArgs<GameSource> args)
        {
            if (RenameObject == null)
            {
                return;
            }

            foreach (var item in args.UpdatedItems.Where(item => item.OldData != null && item.OldData.Name != item.NewData.Name))
            {
                RenameObject?.Invoke(this, item.OldData.Name, item.NewData.Name);
            }
        }
    }
}