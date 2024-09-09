using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypePublisher : BaseType
    {
        public TypePublisher(bool adoptEvents = false)
        {
            if (!adoptEvents)
            {
                return;
            }

            API.Instance.Database.Companies.ItemUpdated += ItemUpdated;
        }

        public override event RenameObjectEventHandler RenameObject;

        // The field isn't really read only, but since developers and publishers both use the
        // metadata company we won't support modifying or deleting them for now. Adding new ones and
        // adding/removing from games is fine.
        public override bool CanBeAdded => false;

        public override bool CanBeClearedInGame => true;

        public override bool CanBeDeleted => false;

        public override bool CanBeEmptyInGame => true;

        public override bool CanBeModified => false;

        public override bool CanBeSetInGame => true;

        public override int Count => API.Instance.Database.Companies?.Count ?? 0;

        public override bool IsList => true;

        public override string LabelPlural => ResourceProvider.GetString("LOCPublishersLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCPublisherLabel");

        public override FieldType Type => FieldType.Publisher;

        public override Guid AddDbObject(string name) => API.Instance.Database.Companies.Add(name).Id;

        public override bool AddDbObjectToGame(Game game, List<Guid> idList) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            (game.PublisherIds ?? (game.PublisherIds = new List<Guid>())).AddMissing(idList));

        public override bool AddDbObjectToGame(Game game, Guid id) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            (game.PublisherIds ?? (game.PublisherIds = new List<Guid>())).AddMissing(id));

        public override bool DbObjectExists(string name) => API.Instance.Database.Companies?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectExists(Guid id) => API.Instance.Database.Companies?.Any(x => x.Id == id) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.PublisherIds?.Contains(id) ?? false;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.PublisherIds?.Contains(id) ?? false);

        public override bool DbObjectInUse(List<Game> games, Guid id) => games.Any(x => x.PublisherIds?.Contains(id) ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.PublisherIds?.Clear());

        public override bool FieldInGameIsEmpty(Game game) => (game.PublisherIds?.Count ?? 0) == 0;

        public override Guid GetDbObjectId(string name) =>
            API.Instance.Database.Companies?.FirstOrDefault(x => x.Name == name)?.Id ?? Guid.Empty;

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.Hidden) && (g.PublisherIds?.Contains(id) ?? false));

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.PublisherIds?.Contains(id) ?? false));

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && (g.PublisherIds?.Contains(id) ?? false)).ToList();

        public override List<DatabaseObject> LoadAllMetadata() => API.Instance.Database.Companies
            .Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        public override List<DatabaseObject> LoadGameMetadata(Game game) =>
            game.Publishers?.Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList() ?? new List<DatabaseObject>();

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => API.Instance.Database
            .Companies.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && (g.DeveloperIds?.Contains(x.Id) ?? false) && (g.PublisherIds?.Contains(x.Id) ?? false)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) =>
            API.Instance.Database.Companies?.Any(x => x.Name == name && x.Id != id) ?? false;

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => ids.Count != 0 && ids.Aggregate(false, (current, id) =>
                current | API.Instance.MainView.UIDispatcher.Invoke(() => game.PublisherIds?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, Guid id) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => game.PublisherIds?.Remove(id) ?? false);

        private void ItemUpdated(object sender, ItemUpdatedEventArgs<Company> args)
        {
            if (RenameObject == null)
            {
                return;
            }

            foreach (ItemUpdateEvent<Company> item in args.UpdatedItems.Where(item => item.OldData != null && item.OldData.Name != item.NewData.Name))
            {
                RenameObject?.Invoke(this, item.OldData.Name, item.NewData.Name);
            }
        }
    }
}