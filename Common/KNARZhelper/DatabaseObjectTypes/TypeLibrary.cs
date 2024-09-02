using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeLibrary : BaseType
    {
        private readonly List<DatabaseObject> _libraries = new List<DatabaseObject>();

        public TypeLibrary()
        {
            _libraries.AddRange(API.Instance.Addons.Plugins
                .Where(x => x is LibraryPlugin)
                .Select(x => new DatabaseObject() { Name = ((LibraryPlugin)x).Name, Id = x.Id }));

            _libraries.Add(new DatabaseObject() { Id = default, Name = "Playnite" });
        }

        public override bool CanBeAdded => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => false;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => false;
        public override bool IsList => false;
        public override string LabelPlural => ResourceProvider.GetString("LOCLibraries");
        public override string LabelSingular => ResourceProvider.GetString("LOCLibrary");
        public override FieldType Type => FieldType.Library;

        public override bool DbObjectExists(string name) => _libraries?.Any(x => x.Name == name) ?? false;

        public override bool DbObjectExists(Guid id) => _libraries?.Any(x => x.Id == id) ?? false;

        public override bool DbObjectInGame(Game game, Guid id) => game.PluginId == id;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.PluginId == id);

        public override void EmptyFieldInGame(Game game)
        {
        }

        public override bool FieldInGameIsEmpty(Game game) => false;

        public override Guid GetDbObjectId(string name) =>
            _libraries?.FirstOrDefault(x => x.Name == name)?.Id ?? default;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.Hidden) && (g.PluginId == id));

        public override List<DatabaseObject> LoadAllMetadata() => _libraries;

        public override List<DatabaseObject> LoadGameMetadata(Game game)
        {
            DatabaseObject library = _libraries.FirstOrDefault(x => x.Id == game.PluginId);

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

        public override List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames) => _libraries.Where(x =>
            !API.Instance.Database.Games.Any(g => !(ignoreHiddenGames && g.Hidden) && (g.PluginId == x.Id))).ToList();

        public override bool NameExists(string name, Guid id) => true;
    }
}