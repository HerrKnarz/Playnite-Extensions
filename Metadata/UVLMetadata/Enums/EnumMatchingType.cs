using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Enums;

public enum MatchingType
{
    Link,
    NameAndPlatform,
    Name
}

public class MatchingTypeModes : Dictionary<MatchingType, string>
{
    public MatchingTypeModes()
    {
        Add(MatchingType.Link, ResourceProvider.GetString("LOCUVLMetadataMatchingTypeLink"));
        Add(MatchingType.NameAndPlatform, ResourceProvider.GetString("LOCUVLMetadataMatchingTypeNameAndPlatform"));
        Add(MatchingType.Name, ResourceProvider.GetString("LOCUVLMetadataMatchingTypeName"));
    }
}
