using KNARZhelper;
using KNARZhelper.BaseModels;
using KNARZhelper.DatabaseObjectTypes;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MetadataObject : BaseMetadataObject
    {
        public MetadataObject(FieldType type, string name = default) : base(type, name)
        {
            Type = type;
            Name = name;
        }

        [DontSerialize]
        public override bool IgnoreHiddenGamesInCount => ControlCenter.Instance.Settings.IgnoreHiddenGamesInGameCount;

        [DontSerialize]
        public Settings Settings => ControlCenter.Instance.Settings;

        public override FieldType Type
        {
            get => _type;
            set
            {
                SetValue(ref _type, value);

                TypeManager = value.GetTypeManager();
            }
        }

        public override string GetDisplayName()
        {
            return Settings?.PrefixItemTypes?.Any(x => (x.Name != default || x.FieldType != FieldType.Empty) && x.Prefix == Prefix) ?? false
            ? EditName
            : Name;
        }

        public override string GetPrefix()
        {
            if (Settings?.Prefixes == null)
            {
                return string.Empty;
            }

            foreach (var prefix in Settings.Prefixes)
            {
                if (Name?.StartsWith(prefix) ?? false)
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        public bool RemoveFromDb(bool checkIfUsed = true)
        {
            if (!(TypeManager is IEditableObjectType type))
            {
                return false;
            }

            if (checkIfUsed)
            {
                var gamesAffected = type.RemoveObjectFromGames(API.Instance.Database.Games.ToList(), Id);

                ControlCenter.UpdateGames(gamesAffected.ToList());
            }

            if (!type.RemoveDbObject(Id))
            {
                return false;
            }

            Id = default;

            return true;
        }

        public IEnumerable<Guid> ReplaceInDb(List<Game> games, FieldType? newType = null, Guid? newId = null)
        {
            IEnumerable<Guid> gameIds = new List<Guid>();

            if (!(TypeManager is IEditableObjectType type))
            {
                return gameIds;
            }

            gameIds = newType?.GetTypeManager() is IEditableObjectType newTypeManager
                ? type.ReplaceDbObject(games, Id, newTypeManager, newId)
                : type.ReplaceDbObject(games, Id);

            return gameIds;
        }

        public override void NotifyMissingItem()
        {
            if (_id == default && TypeManager is TypeLibrary && Name != "Playnite")
            {
                API.Instance.Notifications.Add("MetadataUtilities",
                    $"{ResourceProvider.GetString("LOCMetadataUtilitiesNotificationLibraryNotFound")} {Name}",
                    NotificationType.Info);
            }
        }
    }

    public class MetadataObjectEqualityComparer : IEqualityComparer<MetadataObject>
    {
        public bool Equals(MetadataObject x, MetadataObject y) => x?.TypeAndName.Equals(y?.TypeAndName) ?? false;

        public int GetHashCode(MetadataObject obj) => obj.TypeAndName.GetHashCode();
    }
}