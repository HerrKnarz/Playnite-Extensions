using KNARZhelper;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MetadataListObjects : ObservableCollection<MetadataListObject>
    {
        public void LoadMetadata(bool showGameNumber = true, FieldType? type = null)
        {
            List<MetadataListObject> temporaryList = new List<MetadataListObject>();

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                ResourceProvider.GetString("LOCLoadingLabel"),
                false
            )
            {
                IsIndeterminate = true
            };

            API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
            {
                try
                {
                    if (type == null || type == FieldType.Category)
                    {
                        temporaryList.AddRange(API.Instance.Database.Categories.Select(category
                            => new MetadataListObject
                            {
                                Id = category.Id,
                                Name = category.Name,
                                EditName = category.Name,
                                GameCount = showGameNumber ? API.Instance.Database.Games.Count(g => g.CategoryIds?.Any(t => t == category.Id) ?? false) : 0,
                                Type = FieldType.Category
                            }));
                    }

                    if (type == null || type == FieldType.Feature)
                    {
                        temporaryList.AddRange(API.Instance.Database.Features.Select(feature
                            => new MetadataListObject
                            {
                                Id = feature.Id,
                                Name = feature.Name,
                                EditName = feature.Name,
                                GameCount = showGameNumber ? API.Instance.Database.Games.Count(g => g.FeatureIds?.Any(t => t == feature.Id) ?? false) : 0,
                                Type = FieldType.Feature
                            }));
                    }

                    if (type == null || type == FieldType.Genre)
                    {
                        temporaryList.AddRange(API.Instance.Database.Genres.Select(genre
                            => new MetadataListObject
                            {
                                Id = genre.Id,
                                Name = genre.Name,
                                EditName = genre.Name,
                                GameCount = showGameNumber ? API.Instance.Database.Games.Count(g => g.GenreIds?.Any(t => t == genre.Id) ?? false) : 0,
                                Type = FieldType.Genre
                            }));
                    }

                    if (type == null || type == FieldType.Series)
                    {
                        temporaryList.AddRange(API.Instance.Database.Series.Select(series
                            => new MetadataListObject
                            {
                                Id = series.Id,
                                Name = series.Name,
                                EditName = series.Name,
                                GameCount = showGameNumber ? API.Instance.Database.Games.Count(g => g.SeriesIds?.Any(t => t == series.Id) ?? false) : 0,
                                Type = FieldType.Series
                            }));
                    }

                    if (type == null || type == FieldType.Tag)
                    {
                        temporaryList.AddRange(API.Instance.Database.Tags.Select(tag
                            => new MetadataListObject
                            {
                                Id = tag.Id,
                                Name = tag.Name,
                                EditName = tag.Name,
                                GameCount = showGameNumber ? API.Instance.Database.Games.Count(g => g.TagIds?.Any(t => t == tag.Id) ?? false) : 0,
                                Type = FieldType.Tag
                            }));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, globalProgressOptions);

            Clear();
            this.AddMissing(temporaryList.OrderBy(x => x.TypeLabel).ThenBy(x => x.Name));
        }

        public bool MergeItems(FieldType type, Guid id)
        {
            bool result = false;

            using (API.Instance.Database.BufferedUpdate())
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                    ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergingItems"),
                    false
                )
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = Count - 1;

                        foreach (MetadataListObject item in this)
                        {
                            if (item.Id != id)
                            {
                                DatabaseObjectHelper.ReplaceDbObject(item.Type, item.Id, type, id);

                                activateGlobalProgress.CurrentProgressValue++;
                            }
                        }

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, globalProgressOptions);
            }

            return result;
        }
    }
}