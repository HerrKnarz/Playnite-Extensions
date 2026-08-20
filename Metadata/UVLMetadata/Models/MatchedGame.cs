using KNARZhelper.GamesCommon;
using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Models;

public enum MatchingType
{
    Link,
    Name,
    NameAndPlatform
}

public class MatchedGame : ObservableObject
{
    public MatchingType MatchingType
    {
        get;
        set
        {
            SetValue(ref field, value);

            MatchingTypeCaption = field switch
            {
                MatchingType.Link => ResourceProvider.GetString("LOCUVLMetadataMatchingTypeLink"),
                MatchingType.Name => ResourceProvider.GetString("LOCUVLMetadataMatchingTypeName"),
                MatchingType.NameAndPlatform => ResourceProvider.GetString("LOCUVLMetadataMatchingTypeNameAndPlatform"),
                _ => string.Empty
            };
        }
    }

    public string MatchingTypeCaption
    {
        get;
        set => SetValue(ref field, value);
    } = string.Empty;

    public GameEx PlayniteGame
    {
        get;
        set => SetValue(ref field, value);
    }

    public UVLItemOption UVLGame
    {
        get;
        set => SetValue(ref field, value);
    }
}
