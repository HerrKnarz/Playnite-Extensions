using KNARZhelper;
using Playnite.SDK;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MetadataUtilities
{
    public static class EnumHelper
    {
        public static string GetEnumDisplayName(this Enum e)
            => ResourceProvider.GetString($"LOCMetadataUtilitiesEnum_{e}");
    }

    public class FieldTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((SettableFieldType)value).GetEnumDisplayName();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}