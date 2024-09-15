using KNARZhelper.DatabaseObjectTypes;
using System;
using System.Collections.Generic;

namespace MetadataUtilities.Models
{
    public class ItemList
    {
        public List<Guid> Items { get; set; }
        public IEditableObjectType ObjectType { get; set; }
    }
}