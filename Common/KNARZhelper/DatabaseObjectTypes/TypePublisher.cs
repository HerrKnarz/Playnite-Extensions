using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypePublisher : BaseCompanyType
    {
        public TypePublisher(bool adoptEvents = false) : base(adoptEvents)
        {
        }

        public override string LabelPlural => ResourceProvider.GetString("LOCPublishersLabel");

        public override string LabelSingular => ResourceProvider.GetString("LOCPublisherLabel");

        public override FieldType Type => FieldType.Publisher;

        public override List<DatabaseObject> LoadGameMetadata(Game game) => LoadGameMetadata(game.Publishers);

        internal override List<Guid> GameGuids(Game game, bool writeable = false) =>
            writeable
                ? game.PublisherIds ?? (game.PublisherIds = new List<Guid>())
                : game.PublisherIds ?? new List<Guid>();
    }
}