using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Enums;

public enum AddLink
{
    PerfectAndVeryGood,
    MatchingPlatform,
    AllGames,
    Never
}

public class AddLinkModes : Dictionary<AddLink, string>
{
    public AddLinkModes()
    {
        Add(AddLink.PerfectAndVeryGood, ResourceProvider.GetString("LOCUVLMetadataAddLinkPerfectAndVeryGood"));
        Add(AddLink.MatchingPlatform, ResourceProvider.GetString("LOCUVLMetadataAddLinkMatchingPlatform"));
        Add(AddLink.AllGames, ResourceProvider.GetString("LOCUVLMetadataAddLinkAllGames"));
        Add(AddLink.Never, ResourceProvider.GetString("LOCUVLMetadataAddLinkNever"));
    }
}
