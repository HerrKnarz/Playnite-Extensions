using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Enums;

public enum MatchingScore
{
    Perfect,
    VeryGood,
    Good,
    Acceptable,
    Poor
}

public class MatchingScoreModes : Dictionary<MatchingScore, string>
{
    public MatchingScoreModes()
    {
        Add(MatchingScore.Perfect, ResourceProvider.GetString("LOCUVLMetadataMatchingScorePerfect"));
        Add(MatchingScore.VeryGood, ResourceProvider.GetString("LOCUVLMetadataMatchingScoreVeryGood"));
        Add(MatchingScore.Good, ResourceProvider.GetString("LOCUVLMetadataMatchingScoreGood"));
        Add(MatchingScore.Acceptable, ResourceProvider.GetString("LOCUVLMetadataMatchingScoreAcceptable"));
        Add(MatchingScore.Poor, ResourceProvider.GetString("LOCUVLMetadataMatchingScorePoor"));
    }
}
