using KNARZhelper.DatabaseObjectTypes;
using System;

namespace KNARZhelper.Enum
{
    public enum FieldType
    {
        AgeRating = 5,
        Category = 0,
        Developer = 6,
        Feature = 1,
        Genre = 2,
        Platform = 7,
        Publisher = 8,
        Series = 3,
        Source = 9,
        Tag = 4,
    }

    public static class FieldTypeHelper
    {
        public static IDatabaseObjectType GetTypeManager(this FieldType e)
        {
            switch (e)
            {
                case FieldType.AgeRating:
                    return new TypeAgeRating();

                case FieldType.Category:
                    return new TypeCategory();

                case FieldType.Developer:
                    return new TypeDeveloper();

                case FieldType.Feature:
                    return new TypeFeature();

                case FieldType.Genre:
                    return new TypeGenre();

                case FieldType.Platform:
                    return new TypePlatform();

                case FieldType.Publisher:
                    return new TypePublisher();

                case FieldType.Series:
                    return new TypeSeries();

                case FieldType.Source:
                    return new TypeSource();

                case FieldType.Tag:
                    return new TypeTag();

                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }
    }
}