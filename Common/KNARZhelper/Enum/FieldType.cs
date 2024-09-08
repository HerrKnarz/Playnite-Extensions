using KNARZhelper.DatabaseObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;

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
        Favorite = 24,
        Feature = 1,
        Genre = 2,
        Hdr = 29,
        Hidden = 25,
        Icon = 14,
        InstallSize = 31,
        IsInstalled = 26,
        LastPlayed = 23,
        Library = 11,
        Notes = 22,
        Platform = 7,
        PlayCount = 27,
        Publisher = 8,
        OverrideInstallState = 30,
        Region = 12,
        ReleaseDate = 19,
        Series = 3,
        Source = 9,
        Tag = 4,
        TimePlayed = 28,
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
                        new TypeFavorite(),
                        new TypeFeature(),
                        new TypeGenre(),
                        new TypeHdr(),
                        new TypeHidden(),
                        new TypeIcon(),
                        new TypeInstallSize(),
                        new TypeIsInstalled(),
                        new TypeLastPlayed(),
                        new TypeLibrary(),
                        new TypeNotes(),
                        new TypeOverrideInstallState(),
                        new TypePlatform(),
                        new TypePlayCount(),
                        new TypePublisher(),
                        new TypeRegion(),
                        new TypeReleaseDate(),
                        new TypeSeries(),
                        new TypeSource(),
                        new TypeTag(),
                        new TypeTimePlayed(),
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

                case FieldType.Favorite:
                    return new TypeFavorite();

                case FieldType.Feature:
                    return new TypeFeature();

                case FieldType.Genre:
                    return new TypeGenre();

                case FieldType.Hdr:
                    return new TypeHdr();

                case FieldType.Hidden:
                    return new TypeHidden();

                case FieldType.Icon:
                    return new TypeIcon();

                case FieldType.InstallSize:
                    return new TypeInstallSize();

                case FieldType.IsInstalled:
                    return new TypeIsInstalled();

                case FieldType.LastPlayed:
                    return new TypeLastPlayed();

                case FieldType.Library:
                    return new TypeLibrary();

                case FieldType.Notes:
                    return new TypeNotes();

                case FieldType.OverrideInstallState:
                    return new TypeOverrideInstallState();

                case FieldType.Platform:
                    return new TypePlatform();

                case FieldType.PlayCount:
                    return new TypePlayCount();

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

                case FieldType.TimePlayed:
                    return new TypeTimePlayed();

                case FieldType.UserScore:
                    return new TypeUserScore();

                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        public static Dictionary<FieldType, string> ItemListFieldValues() => GetAllTypes()
            .Where(x => x.ValueType == ItemValueType.ItemList && x.CanBeSetInGame && x.IsList && x.CanBeModified)
            .ToDictionary(type => type.Type, type => type.LabelSingular);
    }
}