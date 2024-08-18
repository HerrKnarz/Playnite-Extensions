using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypePublisher : BaseType
    {
        // The field isn't really read only, but since developers and publishers both use the
        // metadata company we won't support modifying or deleting them for now. Adding new ones and
        // adding/removing from games is fine.
        public override bool CanBeDeleted => false;

        public override bool CanBeModified => false;

        public override bool IsList => true;
        public override FieldType Type => FieldType.Publisher;

        public override Guid AddDbObject(string name) => API.Instance.Database.Companies.Add(name).Id;

        public override bool AddDbObjectToGame(Game game, List<Guid> idList) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.PublisherIds ?? (game.PublisherIds = new List<Guid>())).AddMissing(idList);

        public override bool AddDbObjectToGame(Game game, Guid id) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.PublisherIds ?? (game.PublisherIds = new List<Guid>())).AddMissing(id);

        public override bool DbObjectExists(string name) => API.Instance.Database.Companies?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.PublisherIds?.Contains(id) ?? false;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.PublisherIds?.Contains(id) ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.PublisherIds?.Clear());

        public override bool FieldInGameIsEmpty(Game game) => !game.PublisherIds?.Any() ?? true;

        public override Guid GetDbObjectId(string name) =>
            API.Instance.Database.Companies?.FirstOrDefault(x => x.Name == name)?.Id ?? Guid.Empty;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.PublisherIds?.Contains(id) ?? false));

        public override bool NameExists(string name, Guid id) =>
            API.Instance.Database.Companies?.Any(x => x.Name == name && x.Id != id) ?? false;

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => ids.Count != 0 && ids.Aggregate(false, (current, id) =>
                current | API.Instance.MainView.UIDispatcher.Invoke(() => game.PublisherIds?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, Guid id) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => game.PublisherIds?.Remove(id) ?? false);
    }
}