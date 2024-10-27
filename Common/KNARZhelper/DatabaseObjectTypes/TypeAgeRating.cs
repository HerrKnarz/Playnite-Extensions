using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeAgeRating : BaseListType
    {
        private readonly IItemCollection<AgeRating> _collection = API.Instance.Database.AgeRatings;

        public TypeAgeRating(bool adoptEvents = false)
        {
            if (!adoptEvents)
            {
                return;
            }

            API.Instance.Database.AgeRatings.ItemUpdated += ItemUpdated;
        }

        public override event RenameObjectEventHandler RenameObject;

        public override int Count => _collection?.Count ?? 0;

        public override string LabelPlural => ResourceProvider.GetString("LOCAgeRatingsLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCAgeRatingLabel");

        public override FieldType Type => FieldType.AgeRating;

        public override Guid AddDbObject(string name) => AddDbObject(name, _collection);

        public override bool DbObjectExists(string name) => DbObjectExists(name, _collection);

        public override bool DbObjectExists(Guid id) => DbObjectExists(id, _collection);

        public override Guid GetDbObjectId(string name) => GetDbObjectId(name, _collection);

        public override List<DatabaseObject> LoadAllMetadata(HashSet<Guid> itemsToIgnore) => LoadAllMetadata(_collection, itemsToIgnore);

        public override List<DatabaseObject> LoadGameMetadata(Game game, HashSet<Guid> itemsToIgnore = null) => LoadGameMetadata(game.AgeRatings, itemsToIgnore);

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => LoadUnusedMetadata(ignoreHiddenGames, _collection);

        public override bool NameExists(string name, Guid id) => NameExists(name, id, _collection);

        public override bool RemoveDbObject(Guid id) => RemoveDbObject(id, _collection);

        public override void UpdateDbObject(Guid id, string name) => UpdateDbObject(id, name, _collection);

        internal override List<Guid> GameGuids(Game game, bool writeable = false) =>
            writeable
                ? game.AgeRatingIds ?? (game.AgeRatingIds = new List<Guid>())
                : game.AgeRatingIds ?? new List<Guid>();

        private void ItemUpdated(object sender, ItemUpdatedEventArgs<AgeRating> args)
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