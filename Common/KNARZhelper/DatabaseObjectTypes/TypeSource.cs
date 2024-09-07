using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeSource : BaseType
    {
        public override int Count => API.Instance.Database.Sources?.Count ?? 0;
        public override bool IsList => false;
        public override string LabelPlural => ResourceProvider.GetString("LOCSourcesLabel");
        public override string LabelSingular => ResourceProvider.GetString("LOCSourceLabel");

        public override FieldType Type => FieldType.Source;

        public override Guid AddDbObject(string name) => API.Instance.Database.Sources.Add(name).Id;

        public override bool AddDbObjectToGame(Game game, Guid id)
        {
            if (game.SourceId == id)
            {
                return false;
            }

            API.Instance.MainView.UIDispatcher.Invoke(() => game.SourceId = id);

            return true;
        }

        public override bool DbObjectExists(string name) => API.Instance.Database.Sources?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectExists(Guid id) => API.Instance.Database.Sources?.Any(x => x.Id == id) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.SourceId == id;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.SourceId == id);

        public override bool DbObjectInUse(List<Game> games, Guid id) => games.Any(x => x.SourceId == id);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.SourceId = default);

        public override bool FieldInGameIsEmpty(Game game) => game.SourceId == default;

        public override Guid GetDbObjectId(string name) =>
            API.Instance.Database.Sources?.FirstOrDefault(x => x.Name == name)?.Id ?? default;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.SourceId == id));

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.Hidden) && (g.SourceId == id));

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && (g.SourceId == id)).ToList();

        public override List<DatabaseObject> LoadAllMetadata() => API.Instance.Database.Sources
            .Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        public override List<DatabaseObject> LoadGameMetadata(Game game) =>
            new List<DatabaseObject> { new DatabaseObject() { Name = game.Source.Name, Id = game.Source.Id } };

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => API.Instance.Database
            .Sources.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && (g.SourceId == x.Id)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) =>
            API.Instance.Database.Sources?.Any(x => x.Name == name && x.Id != id) ?? false;

        public override bool RemoveDbObject(Guid id, bool checkIfUsed = true) =>
            // If we need to check first, we can simply call the replace method, that removes the
            // item itself, if no item is entered to replace the old one.
            DbObjectExists(id) && (checkIfUsed
                ? ReplaceDbObject(API.Instance.Database.Games.ToList(), id)?.Count() > 0
                : API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Sources?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids)
        {
            if (ids.Count == 0 || !ids.Contains(game.SourceId))
            {
                return false;
            }

            EmptyFieldInGame(game);

            return true;
        }

        public override bool RemoveObjectFromGame(Game game, Guid id)
        {
            if (game.SourceId != id)
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

            foreach (Game game in games.Where(g => g.SourceId == id))
            {
                game.SourceId = default;

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
            GameSource item = API.Instance.Database.Sources?.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return;
            }

            item.Name = name;

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                API.Instance.Database.Sources.Update(item);
            });
        }
    }
}