﻿using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    public class TypeDeveloper : BaseCompanyType
    {
        public TypeDeveloper(bool adoptEvents = false) : base(adoptEvents)
        {
        }

        public override string LabelPlural => ResourceProvider.GetString("LOCDevelopersLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCDeveloperLabel");

        public override FieldType Type => FieldType.Developer;

        public override List<DatabaseObject> LoadGameMetadata(Game game, HashSet<Guid> itemsToIgnore = null) => LoadGameMetadata(game.Developers, itemsToIgnore);

        internal override List<Guid> GameGuids(Game game, bool writeable = false) =>
            writeable
                ? game.DeveloperIds ?? (game.DeveloperIds = new List<Guid>())
            : game.DeveloperIds ?? new List<Guid>();
    }
}