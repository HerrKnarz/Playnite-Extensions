using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Enums;

public enum RatingToUse
{
    Median,
    Average,
}

public class RatingToUseModes : Dictionary<RatingToUse, string>
{
    public RatingToUseModes()
    {
        Add(RatingToUse.Median, ResourceProvider.GetString("LOCUVLMetadataSettingsRatingMedian"));
        Add(RatingToUse.Average, ResourceProvider.GetString("LOCUVLMetadataSettingsRatingAverage"));
    }
}
