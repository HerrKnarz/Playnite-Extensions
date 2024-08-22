using System.Windows.Input;
using KNARZhelper.Enum;

namespace MetadataUtilities.ViewModels
{
    public class FieldTypeContextAction
    {
        public ICommand Action { get; set; }
        public FieldType FieldType { get; set; }
        public string Name { get; set; }
    }
}