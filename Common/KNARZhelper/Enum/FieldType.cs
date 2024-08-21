using KNARZhelper.DatabaseObjectTypes;
using System;
using System.Collections.Generic;

namespace KNARZhelper.Enum
{
    public enum FieldType
    {
        AgeRating = 5,
        Background = 13,
        Category = 0,
        CompletionStatus = 10,
        Cover = 15,
        Developer = 6,
        Feature = 1,
        Genre = 2,
        Icon = 14,
        Library = 11,
        Platform = 7,
        Publisher = 8,
        Region = 12,
        Series = 3,
        Source = 9,
        Tag = 4,
    }

    public static class FieldTypeHelper
    {
        public static List<IDatabaseObjectType> GetAllTypes() =>
            new List<IDatabaseObjectType>()
            {
                new TypeAgeRating(),
                new TypeBackground(),
                new TypeCategory(),
                new TypeCompletionStatus(),
                new TypeCover(),
                new TypeDeveloper(),
                new TypeFeature(),
                new TypeGenre(),
                new TypeIcon(),
                new TypeLibrary(),
                new TypePlatform(),
                new TypePublisher(),
                new TypeRegion(),
                new TypeSeries(),
                new TypeSource(),
                new TypeTag()
            };

        public static IDatabaseObjectType GetTypeManager(this FieldType e)
        {
            switch (e)
            {
                case FieldType.AgeRating:
                    return new TypeAgeRating();

                case FieldType.Background:
                    return new TypeBackground();

                case FieldType.Category:
                    return new TypeCategory();

                case FieldType.CompletionStatus:
                    return new TypeCompletionStatus();

                case FieldType.Cover:
                    return new TypeCover();

                case FieldType.Developer:
                    return new TypeDeveloper();

                case FieldType.Feature:
                    return new TypeFeature();

                case FieldType.Genre:
                    return new TypeGenre();

                case FieldType.Icon:
                    return new TypeIcon();

                case FieldType.Library:
                    return new TypeLibrary();

                case FieldType.Platform:
                    return new TypePlatform();

                case FieldType.Publisher:
                    return new TypePublisher();

                case FieldType.Region:
                    return new TypeRegion();

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