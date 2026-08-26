using KNARZhelper.GamesCommon;
using Playnite.SDK;
using System;
using System.Collections.Generic;

namespace UVLMetadata.Models;

public enum MatchingScore
{
    Perfect,
    VeryGood,
    Good,
    Acceptable,
    Poor
}

public enum MatchingType
{
    Link,
    Name,
    NameAndPlatform
}

public class MatchedGame : ObservableObject
{
    public MatchingScore MatchingScore
    {
        get
        {
            if (MatchingType == MatchingType.Link)
            {
                return MatchingScore.Perfect;
            }

            if (int.TryParse(UVLGame.ReleaseDate, out var uvlReleaseYearAsInt))
            {
                if (PlayniteGame.Game.ReleaseYear == uvlReleaseYearAsInt)
                {
                    return uvlReleaseYearAsInt < 2000 && UVLGame.Name.Length < 15
                        ? MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Acceptable : MatchingScore.Perfect
                        : MatchingType != MatchingType.NameAndPlatform ? MatchingScore.VeryGood : MatchingScore.Perfect;
                }

                if (PlayniteGame.Game.ReleaseYear is null)
                {
                    return uvlReleaseYearAsInt < 2000 && UVLGame.Name.Length < 15
                        ? MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Poor : MatchingScore.Good
                        : MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Good : MatchingScore.VeryGood;
                }

                if (Math.Abs(PlayniteGame.Game.ReleaseYear.Value - uvlReleaseYearAsInt) <= 5)
                {
                    return uvlReleaseYearAsInt < 2000 && UVLGame.Name.Length < 15
                        ? MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Acceptable : MatchingScore.VeryGood
                        : MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Good : MatchingScore.VeryGood;
                }
            }

            return UVLGame.Name.Length < 15
                ? MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Poor : MatchingScore.Acceptable
                : MatchingType != MatchingType.NameAndPlatform ? MatchingScore.Good : MatchingScore.VeryGood;
        }
    }

    public string MatchingScoreCaption => MatchingScore switch
    {
        MatchingScore.Perfect => ResourceProvider.GetString("LOCUVLMetadataMatchingScorePerfect"),
        MatchingScore.VeryGood => ResourceProvider.GetString("LOCUVLMetadataMatchingScoreVeryGood"),
        MatchingScore.Good => ResourceProvider.GetString("LOCUVLMetadataMatchingScoreGood"),
        MatchingScore.Acceptable => ResourceProvider.GetString("LOCUVLMetadataMatchingScoreAcceptable"),
        MatchingScore.Poor => ResourceProvider.GetString("LOCUVLMetadataMatchingScorePoor"),
        _ => string.Empty
    };

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

            OnPropertyChanged(nameof(MatchingScore));
            OnPropertyChanged(nameof(MatchingScoreCaption));
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
