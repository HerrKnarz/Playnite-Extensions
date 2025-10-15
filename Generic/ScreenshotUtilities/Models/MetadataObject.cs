using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;

namespace ScreenshotUtilities.Models
{
    public class MetadataObject : BaseMetadataObject
    {
        public MetadataObject(FieldType type, string name = default) : base(type, name)
        {
            Type = type;
            Name = name;
        }

        public override FieldType Type
        {
            get => _type;
            set
            {
                SetValue(ref _type, value);

                if (_type == FieldType.Category)
                {
                    TypeManager = new TypeCategory();
                }
                else if (_type == FieldType.Tag)
                {
                    TypeManager = new TypeTag();
                }
            }
        }
    }
}
