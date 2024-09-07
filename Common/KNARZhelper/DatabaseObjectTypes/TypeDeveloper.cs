using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeDeveloper : BaseType
    {
        // The field isn't really read only, but since developers and publishers both use the
        // metadata company we won't support modifying or deleting them for now. Adding new ones and
        // adding/removing from games is fine.
        public override bool CanBeDeleted => false;

        public override bool CanBeModified => false;
        public override int Count => API.Instance.Database.Companies.Count;
        public override bool IsList => true;
        public override string LabelPlural => ResourceProvider.GetString("LOCDevelopersLabel");
        public override string LabelSingular => ResourceProvider.GetString("LOCDeveloperLabel");
        public override FieldType Type => FieldType.Developer;

        public override Guid AddDbObject(string name) => API.Instance.Database.Companies.Add(name).Id;

        public override bool AddDbObjectToGame(Game game, List<Guid> idList) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.DeveloperIds ?? (game.DeveloperIds = new List<Guid>())).AddMissing(idList);

        public override bool AddDbObjectToGame(Game game, Guid id) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.DeveloperIds ?? (game.DeveloperIds = new List<Guid>())).AddMissing(id);

        public override bool DbObjectExists(string name) => API.Instance.Database.Companies?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectExists(Guid id) => API.Instance.Database.Companies?.Any(x => x.Id == id) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.DeveloperIds?.Contains(id) ?? false;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.DeveloperIds?.Contains(id) ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.DeveloperIds?.Clear());

        public override bool FieldInGameIsEmpty(Game game) => (game.DeveloperIds?.Count ?? 0) == 0;

        public override Guid GetDbObjectId(string name) =>
            API.Instance.Database.Companies?.FirstOrDefault(x => x.Name == name)?.Id ?? Guid.Empty;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.DeveloperIds?.Contains(id) ?? false));

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && (g.DeveloperIds?.Contains(id) ?? false)).ToList();

        public override List<DatabaseObject> LoadAllMetadata() => API.Instance.Database.Companies
            .Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        public override List<DatabaseObject> LoadGameMetadata(Game game) =>
            game.Developers?.Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList() ?? new List<DatabaseObject>();

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => API.Instance.Database
            .Companies.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && (g.DeveloperIds?.Contains(x.Id) ?? false) && (g.PublisherIds?.Contains(x.Id) ?? false)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) =>
            API.Instance.Database.Companies?.Any(x => x.Name == name && x.Id != id) ?? false;

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => ids.Count != 0 && ids.Aggregate(false, (current, id) =>
                current | API.Instance.MainView.UIDispatcher.Invoke(() => game.DeveloperIds?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, Guid id) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => game.DeveloperIds?.Remove(id) ?? false);
    }
}