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
        CommunityScore = 16,
        Cover = 15,
        CriticScore = 17,
        DateAdded = 20,
        Description = 21,
        Developer = 6,
        Feature = 1,
        Genre = 2,
        Icon = 14,
        Library = 11,
        Platform = 7,
        Publisher = 8,
        Region = 12,
        ReleaseDate = 19,
        Series = 3,
        Source = 9,
        Tag = 4,
        UserScore = 18,
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
                new TypeCommunityScore(),
                new TypeCover(),
                new TypeCriticScore(),
                new TypeDateAdded(),
                new TypeDescription(),
                new TypeDeveloper(),
                new TypeFeature(),
                new TypeGenre(),
                new TypeIcon(),
                new TypeLibrary(),
                new TypePlatform(),
                new TypePublisher(),
                new TypeRegion(),
                new TypeReleaseDate(),
                new TypeSeries(),
                new TypeSource(),
                new TypeTag(),
                new TypeUserScore(),
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

                case FieldType.CommunityScore:
                    return new TypeCommunityScore();

                case FieldType.Cover:
                    return new TypeCover();

                case FieldType.CriticScore:
                    return new TypeCriticScore();

                case FieldType.DateAdded:
                    return new TypeDateAdded();

                case FieldType.Description:
                    return new TypeDescription();

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

                case FieldType.ReleaseDate:
                    return new TypeReleaseDate();

                case FieldType.Series:
                    return new TypeSeries();

                case FieldType.Source:
                    return new TypeSource();

                case FieldType.Tag:
                    return new TypeTag();

                case FieldType.UserScore:
                    return new TypeUserScore();

                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }
    }
}