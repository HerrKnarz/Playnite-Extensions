using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeCompletionStatus : BaseType
    {
        public override int Count => API.Instance.Database.CompletionStatuses.Count;
        public override bool IsList => false;
        public override string LabelPlural => ResourceProvider.GetString("LOCCompletionStatuses");
        public override string LabelSingular => ResourceProvider.GetString("LOCCompletionStatus");
        public override FieldType Type => FieldType.CompletionStatus;

        public override Guid AddDbObject(string name) => API.Instance.Database.CompletionStatuses.Add(name).Id;

        public override bool AddDbObjectToGame(Game game, Guid id)
        {
            if (game.CompletionStatusId == id)
            {
                return false;
            }

            API.Instance.MainView.UIDispatcher.Invoke(() => game.CompletionStatusId = id);

            return true;
        }

        public override bool DbObjectExists(string name) => API.Instance.Database.CompletionStatuses?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectExists(Guid id) => API.Instance.Database.CompletionStatuses?.Any(x => x.Id == id) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.CompletionStatusId == id;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.CompletionStatusId == id);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.CompletionStatusId = default);

        public override bool FieldInGameIsEmpty(Game game) => game.CompletionStatusId == default;

        public override Guid GetDbObjectId(string name) =>
            API.Instance.Database.CompletionStatuses?.FirstOrDefault(x => x.Name == name)?.Id ?? default;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.CompletionStatusId == id));

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && (g.CompletionStatusId == id)).ToList();

        public override List<DatabaseObject> LoadAllMetadata() => API.Instance.Database.CompletionStatuses
            .Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        public override List<DatabaseObject> LoadGameMetadata(Game game) =>
            new List<DatabaseObject> { new DatabaseObject() { Name = game.CompletionStatus.Name, Id = game.CompletionStatus.Id } };

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => API.Instance.Database
            .CompletionStatuses.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && (g.CompletionStatusId == x.Id)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) =>
            API.Instance.Database.CompletionStatuses?.Any(x => x.Name == name && x.Id != id) ?? false;

        public override bool RemoveDbObject(Guid id, bool checkIfUsed = true) =>
            // If we need to check first, we can simply call the replace method, that removes the
            // item itself, if no item is entered to replace the old one.
            DbObjectExists(id) && (checkIfUsed
                ? ReplaceDbObject(API.Instance.Database.Games.ToList(), id)?.Count() > 0
                : API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.CompletionStatuses?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids)
        {
            if (ids.Count == 0 || !ids.Contains(game.CompletionStatusId))
            {
                return false;
            }

            EmptyFieldInGame(game);

            return true;
        }

        public override bool RemoveObjectFromGame(Game game, Guid id)
        {
            if (game.CompletionStatusId != id)
            {
                return false;
            }

            EmptyFieldInGame(game);

            return true;
        }

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, FieldType? newType = null, Guid? newId = null, bool removeAfter = true)
        {
            if (id == default)
            {
                yield break;
            }

            foreach (Game game in games.Where(g => g.CompletionStatusId == id))
            {
                game.CompletionStatusId = default;

                if (newType != null && newId != null)
                {
                    AddDbObjectToGame(game, (Guid)newId);
                }

                API.Instance.MainView.UIDispatcher.Invoke(delegate
                {
                    API.Instance.Database.Games.Update(game);
                });

                yield return game.Id;
            }

            if (removeAfter && !DbObjectInUse(id))
            {
                RemoveDbObject(id, false);
            }
        }

        public override void UpdateDbObject(Guid id, string name)
        {
            CompletionStatus item = API.Instance.Database.CompletionStatuses?.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return;
            }

            item.Name = name;

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                API.Instance.Database.CompletionStatuses.Update(item);
            });
        }
    }
}