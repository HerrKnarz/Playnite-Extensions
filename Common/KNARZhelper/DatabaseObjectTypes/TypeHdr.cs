using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeHdr : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => true;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetByMetadataAddOn => false;
        public override bool CanBeSetInGame => true;
        public override bool IsList => false;

        public override string LabelSingular => ResourceProvider.GetString("LOCGameHdrTitle");

        public override FieldType Type => FieldType.Hdr;

        public override ItemValueType ValueType => ItemValueType.Boolean;

        public override bool AddDbObjectToGame(Game game, List<Guid> idList) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.EnableSystemHdr = true);

        public override bool AddDbObjectToGame(Game game, Guid id) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.EnableSystemHdr = true);

        public override bool DbObjectInGame(Game game, Guid id) => game.EnableSystemHdr;

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => x.EnableSystemHdr);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.EnableSystemHdr = false);

        public override bool FieldInGameIsEmpty(Game game) => !game.EnableSystemHdr;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Count(g => !(ignoreHidden && g.EnableSystemHdr) && g.EnableSystemHdr);

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.EnableSystemHdr) && g.EnableSystemHdr);

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.EnableSystemHdr) && g.EnableSystemHdr).ToList();

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => !API.Instance.MainView.UIDispatcher.Invoke(() => game.EnableSystemHdr = false);

        public override bool RemoveObjectFromGame(Game game, Guid id) => !API.Instance.MainView.UIDispatcher.Invoke(() => game.EnableSystemHdr = false);
    }
}