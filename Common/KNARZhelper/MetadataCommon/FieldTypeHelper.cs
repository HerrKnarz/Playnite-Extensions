using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.MetadataCommon
{
    /// <summary>
    /// Helper class for field types.
    /// </summary>
    public static class FieldTypeHelper
    {
        /// <summary>
        /// Gets all metadata field types of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to get</typeparam>
        /// <param name="adoptEvents">Specifies if the field type should adopt events</param>
        /// <returns>List of field types</returns>
        public static IEnumerable<T> GetAllTypes<T>(bool adoptEvents = false) where T : IMetadataFieldType
        {
            foreach (var type in GetAllTypes(adoptEvents))
            {
                if (type is T specificType)
                {
                    yield return specificType;
                }
            }
        }

        /// <summary>
        /// Gets all metadata field types.
        /// </summary>
        /// <param name="adoptEvents">Specifies if the field type should adopt events</param>
        /// <returns>List of field types</returns>
        public static List<IMetadataFieldType> GetAllTypes(bool adoptEvents = false) =>
            new List<IMetadataFieldType>()
            {
                        new TypeAgeRating(adoptEvents),
                        new TypeBackground(),
                        new TypeCategory(adoptEvents),
                        new TypeCompletionStatus(adoptEvents),
                        new TypeCommunityScore(),
                        new TypeCover(),
                        new TypeCriticScore(),
                        new TypeDateAdded(),
                        new TypeDescription(),
                        new TypeDeveloper(adoptEvents),
                        new TypeFavorite(),
                        new TypeFeature(adoptEvents),
                        new TypeGenre(adoptEvents),
                        new TypeHdr(),
                        new TypeHidden(),
                        new TypeIcon(),
                        new TypeInstallSize(),
                        new TypeIsInstalled(),
                        new TypeLastPlayed(),
                        new TypeLibrary(),
                        new TypeLink(),
                        new TypeManual(),
                        new TypeName(),
                        new TypeNotes(),
                        new TypeOverrideInstallState(),
                        new TypePlatform(adoptEvents),
                        new TypePlayCount(),
                        new TypePublisher(adoptEvents),
                        new TypeRegion(adoptEvents),
                        new TypeReleaseDate(),
                        new TypeSeries(adoptEvents),
                        new TypeSource(adoptEvents),
                        new TypeSortingName(),
                        new TypeTag(adoptEvents),
                        new TypeTimePlayed(),
                        new TypeUserScore(),
                        new TypeVersion(),
            };

        /// <summary>
        /// Gets all item list metadata field types.
        /// </summary>
        /// <returns>List of field types</returns>
        public static IEnumerable<IEditableObjectType> GetItemListTypes() => GetAllTypes<IEditableObjectType>()
            .Where(x => x.ValueType == ItemValueType.ItemList && x.CanBeSetInGame && x.IsList && x.CanBeModified)
            .OrderBy(x => x.LabelSingular);

        /// <summary>
        /// Gets the type manager for the specified field type.
        /// </summary>
        /// <param name="e">Field type to get the type manager for</param>
        /// <returns>Type manager of the field type</returns>
        public static IMetadataFieldType GetTypeManager(this FieldType e)
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

                case FieldType.Link:
                    return new TypeLink();

                case FieldType.Manual:
                    return new TypeManual();

                case FieldType.Name:
                    return new TypeName();

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

                case FieldType.SortingName:
                    return new TypeSortingName();

                case FieldType.Source:
                    return new TypeSource();

                case FieldType.Tag:
                    return new TypeTag();

                case FieldType.TimePlayed:
                    return new TypeTimePlayed();

                case FieldType.UserScore:
                    return new TypeUserScore();

                case FieldType.Version:
                    return new TypeVersion();

                case FieldType.Empty:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        /// <summary>
        /// Dictionary of item list field types and their singular labels.
        /// </summary>
        /// <param name="withEmptyEntry">When true an empty entry will be added to the dictionary</param>
        /// <returns></returns>
        public static Dictionary<FieldType, string> ItemListFieldValues(bool withEmptyEntry = false)
        {
            var result = new Dictionary<FieldType, string>();

            if (withEmptyEntry)
            {
                result.Add(FieldType.Empty, "");
            }

            foreach (var item in GetItemListTypes())
            {
                result.Add(item.Type, item.LabelSingular);
            }

            return result;
        }
    }
}