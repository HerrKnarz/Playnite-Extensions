using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeDeveloper : BaseCompanyType
    {
        public TypeDeveloper(bool adoptEvents = false) : base(adoptEvents)
        {
        }

        public override string LabelPlural => ResourceProvider.GetString("LOCDevelopersLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCDeveloperLabel");

        public override FieldType Type => FieldType.Developer;

        public override List<DatabaseObject> LoadGameMetadata(Game game) => LoadGameMetadata(game.Developers);

        internal override List<Guid> GameGuids(Game game, bool writeable = false) =>
            writeable
                ? game.DeveloperIds ?? (game.DeveloperIds = new List<Guid>())
            : game.DeveloperIds ?? new List<Guid>();
    }
}