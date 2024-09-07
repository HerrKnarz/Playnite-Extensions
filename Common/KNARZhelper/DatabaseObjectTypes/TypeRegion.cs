using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeRegion : BaseType
    {
        public override bool CanBeModified => false;
        public override int Count => API.Instance.Database.Regions?.Count ?? 0;
        public override bool IsList => true;
        public override string LabelPlural => ResourceProvider.GetString("LOCRegionsLabel");
        public override string LabelSingular => ResourceProvider.GetString("LOCRegionLabel");
        public override FieldType Type => FieldType.Region;

        public override Guid AddDbObject(string name) => API.Instance.Database.Regions.Add(name).Id;

        public override bool AddDbObjectToGame(Game game, List<Guid> idList) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.RegionIds ?? (game.RegionIds = new List<Guid>())).AddMissing(idList);

        public override bool AddDbObjectToGame(Game game, Guid id) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.RegionIds ?? (game.RegionIds = new List<Guid>())).AddMissing(id);

        public override bool DbObjectExists(string name) => API.Instance.Database.Regions?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectExists(Guid id) => API.Instance.Database.Regions?.Any(x => x.Id == id) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.RegionIds?.Contains(id) ?? false;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.RegionIds?.Contains(id) ?? false);

        public override bool DbObjectInUse(List<Game> games, Guid id) => games.Any(x => x.RegionIds?.Contains(id) ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.RegionIds?.Clear());

        public override bool FieldInGameIsEmpty(Game game) => (game.RegionIds?.Count ?? 0) == 0;

        public override Guid GetDbObjectId(string name) =>
            API.Instance.Database.Regions?.FirstOrDefault(x => x.Name == name)?.Id ?? Guid.Empty;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.RegionIds?.Contains(id) ?? false));

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.Hidden) && (g.RegionIds?.Contains(id) ?? false));

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && (g.RegionIds?.Contains(id) ?? false)).ToList();

        public override List<DatabaseObject> LoadAllMetadata() => API.Instance.Database.Regions
            .Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList();

        public override List<DatabaseObject> LoadGameMetadata(Game game) =>
            game.Regions?.Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList() ?? new List<DatabaseObject>();

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => API.Instance.Database
            .Regions.Where(x => !API.Instance.Database.Games.Any(g =>
                !(ignoreHiddenGames && g.Hidden) && (g.RegionIds?.Contains(x.Id) ?? false)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool NameExists(string name, Guid id) =>
            API.Instance.Database.Regions?.Any(x => x.Name == name && x.Id != id) ?? false;

        public override bool RemoveDbObject(Guid id, bool checkIfUsed = true) =>
            // If we need to check first, we can simply call the replace method, that removes the
            // item itself, if no item is entered to replace the old one.
            DbObjectExists(id) && (checkIfUsed
                ? ReplaceDbObject(API.Instance.Database.Games.ToList(), id)?.Count() > 0
                : API.Instance.MainView.UIDispatcher.Invoke(() => API.Instance.Database.Regions?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => ids.Count != 0 && ids.Aggregate(false, (current, id) =>
                current | API.Instance.MainView.UIDispatcher.Invoke(() => game.RegionIds?.Remove(id) ?? false));

        public override bool RemoveObjectFromGame(Game game, Guid id) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => game.RegionIds?.Remove(id) ?? false);

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, FieldType? newType = null, Guid? newId = null, bool removeAfter = true)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            foreach (Game game in games.Where(g => g.RegionIds?.Contains(id) ?? false))
            {
                if (!game.RegionIds.Remove(id))
                {
                    continue;
                }

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
            Region item = API.Instance.Database.Regions?.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                return;
            }

            item.Name = name;

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                API.Instance.Database.Regions.Update(item);
            });
        }
    }
}