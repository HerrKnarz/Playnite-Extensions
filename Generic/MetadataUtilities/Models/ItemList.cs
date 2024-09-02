using System;
using System.Collections.Generic;
using KNARZhelper.DatabaseObjectTypes;

namespace MetadataUtilities.Models
{
    public class ItemList
    {
        public List<Guid> Items { get; set; }
        public IDatabaseObjectType ObjectType { get; set; }
    }
}