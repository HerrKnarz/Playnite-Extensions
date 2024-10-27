using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeLibrary : IObjectType, IValueType
    {
        private readonly List<DatabaseObject> _libraries = new List<DatabaseObject>();

        public TypeLibrary()
        {
            _libraries.AddRange(API.Instance.Addons.Plugins
                .Where(x => x is LibraryPlugin)
                .Select(x => new DatabaseObject() { Name = ((LibraryPlugin)x).Name, Id = x.Id }));

            _libraries.Add(new DatabaseObject() { Id = default, Name = "Playnite" });
        }

        public bool CanBeAdded => false;
        public bool CanBeClearedInGame => false;
        public bool CanBeDeleted => false;
        public bool CanBeEmptyInGame => false;
        public bool CanBeModified => false;
        public bool CanBeSetByMetadataAddOn => false;
        public bool CanBeSetInGame => false;
        public int Count => _libraries.Count;
        public bool IsList => false;
        public string LabelPlural => ResourceProvider.GetString("LOCLibraries");
        public string LabelSingular => ResourceProvider.GetString("LOCLibrary");
        public FieldType Type => FieldType.Library;
        public ItemValueType ValueType => ItemValueType.ItemList;

        public bool AddValueToGame<T>(Game game, T value) => false;

        public bool DbObjectExists(string name) => _libraries?.Any(x => x.Name == name) ?? false;

        public bool DbObjectExists(Guid id) => _libraries?.Any(x => x.Id == id) ?? false;

        public bool DbObjectInGame(Game game, Guid id) => game.PluginId == id;

        public bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false) => API.Instance.Database.Games.Any(x => !(ignoreHiddenGames && x.Hidden) && x.PluginId == id);

        public bool GameContainsValue<T>(Game game, T value) => value is Guid id && game.PluginId == id;

        public Guid GetDbObjectId(string name) =>
            _libraries?.FirstOrDefault(x => x.Name == name)?.Id ?? default;

        public int GetGameCount(Guid id, bool ignoreHiddenGames = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.PluginId == id));

        public int GetGameCount(List<Game> games, Guid id, bool ignoreHiddenGames = false) =>
            games.Count(g => !(ignoreHiddenGames && g.Hidden) && (g.PluginId == id));

        public List<Game> GetGames(Guid id, bool ignoreHiddenGames = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden) && (g.PluginId == id)).ToList();

        public List<DatabaseObject> LoadAllMetadata(HashSet<Guid> itemsToIgnore) => _libraries.Where(x => !itemsToIgnore.Contains(x.Id)).ToList();

        public List<DatabaseObject> LoadGameMetadata(Game game, HashSet<Guid> itemsToIgnore)
        {
            DatabaseObject library = null;

            if (!itemsToIgnore.Contains(game.PluginId))
            {
                library = _libraries.FirstOrDefault(x => x.Id == game.PluginId);
            }

            return library == null
                ? new List<DatabaseObject>()
                : new List<DatabaseObject>
                {
                    new DatabaseObject()
                    {
                        Name = library.Name,
                        Id = library.Id
                    }
                };
        }

        public List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => _libraries.Where(x =>
            !API.Instance.Database.Games.Any(g => !(ignoreHiddenGames && g.Hidden) && (g.PluginId == x.Id))).ToList();

        public bool NameExists(string name, Guid id) => true;
    }
}