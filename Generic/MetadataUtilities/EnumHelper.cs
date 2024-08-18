using KNARZhelper.Enum;
using MetadataUtilities.Models;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null ? ((FieldType)value).GetEnumDisplayName() : default(object);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    public class LogicTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null ? ((LogicType)value).GetEnumDisplayName() : default(object);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}