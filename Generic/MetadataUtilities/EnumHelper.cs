using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MetadataUtilities
{
    public static class EnumHelper
    {
        public static string GetEnumDisplayIcon(this Enum e)
        {
            switch (e)
            {
                case ActionType actionType:
                    switch (actionType)
                    {
                        case ActionType.AddObject:
                        case ActionType.Set:
                            return "\xf105";

                        case ActionType.RemoveObject:
                            return "\xf104";

                        case ActionType.ClearField:
                            return "\xf110";

                        case ActionType.None:
                            return string.Empty;

                        default:
                            return string.Empty;
                    }
                case ComparatorType comparatorType:
                    switch (comparatorType)
                    {
                        case ComparatorType.Contains:
                            return "\xf100";

                        case ComparatorType.DoesNotContain:
                            return "\xf099";

                        case ComparatorType.IsEmpty:
                            return "\xf100";

                        case ComparatorType.IsNotEmpty:
                            return "\xf099";

                        case ComparatorType.IsBiggerThan:
                            return "\xf108";

                        case ComparatorType.IsSmallerThan:
                            return "\xf109";

                        case ComparatorType.GameIsNew:
                            return "\xf101";

                        case ComparatorType.Equals:
                            return "\xf106";

                        case ComparatorType.DoesntEqual:
                            return "\xf107";

                        case ComparatorType.None:
                            return string.Empty;

                        default:
                            return string.Empty;
                    }
                default:
                    return string.Empty;
            }
        }

        public static string GetEnumDisplayName(this Enum e)
                    => ResourceProvider.GetString($"LOCMetadataUtilitiesEnum_{e}");

        public static string GetEnumDisplayNameWithType(this Enum e)
            => ResourceProvider.GetString($"LOCMetadataUtilitiesEnum_{e.GetType().Name}_{e}");
    }

    public class FieldTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null ? ((FieldType)value).GetTypeManager().LabelSingular : default(object);

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
