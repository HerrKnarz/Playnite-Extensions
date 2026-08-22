using KNARZhelper;
using KNARZhelper.GamesCommon;
using KNARZhelper.WebCommon;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UVLMetadata.Models;

namespace UVLMetadata;

public class GameMatcher(List<Game> playniteGames)
{
    private readonly StringFormatParameters _formatParameters = new()
    {
        RemoveDiacritics = true,
        RemoveEditionSuffix = true,
        RemoveSpecialChars = true,
        ToLower = true,
        UnderscoresToWhitespaces = true,
        WhitespacesToHyphens = true
    };

    private readonly ConcurrentDictionary<string, IList<Guid>> _gamesPerLink = new();
    private readonly ConcurrentDictionary<string, IList<Guid>> _gamesPerName = new();
    private readonly ConcurrentDictionary<string, IList<Guid>> _gamesPerNameAndPlatform = new();
    private readonly int _maxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 1.5));
    private bool _isPrepared = false;
    public Dictionary<Guid, MatchedGame> MatchedGames { get; set; } = [];

    public void MatchGames(List<UVLItemOption> uvlGames)
    {
        var globalProgressOptions = new GlobalProgressOptions($"{ResourceProvider.GetString("LOCUVLMetadataProgressMatchingGames")}", true)
        {
            IsIndeterminate = false
        };

        API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
        {
            try
            {
                if (!_isPrepared)
                {
                    activateGlobalProgress.ProgressMaxValue = uvlGames.Count + 20;
                    activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCUVLMetadataProgressMatchingGames")} - {ResourceProvider.GetString("LOCUVLMetadataProgressMatchingGamesPreparing")}";
                    Prepare();
                    activateGlobalProgress.CurrentProgressValue += 10;
                }
                else
                {
                    activateGlobalProgress.ProgressMaxValue = uvlGames.Count + 10;
                }

                activateGlobalProgress.Text = $"{ResourceProvider.GetString("LOCUVLMetadataProgressMatchingGames")} - {ResourceProvider.GetString("LOCUVLMetadataProgressMatchingGamesMatching")}";

                MatchedGames.Clear();

                var foundByLink = new ConcurrentDictionary<UVLItemOption, IList<Guid>>();
                var foundByNameAndPlatform = new ConcurrentDictionary<UVLItemOption, IList<Guid>>();
                var foundByName = new ConcurrentDictionary<UVLItemOption, IList<Guid>>();

                Parallel.ForEach(uvlGames, new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism, CancellationToken = activateGlobalProgress.CancelToken }, uvlGame =>
                {
                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    uvlGame.DeflatedName = uvlGame.Name.FormatString(_formatParameters) ?? string.Empty;

                    _gamesPerLink.TryGetValue(WebHelper.CleanUpUrl(uvlGame.Url), out var linkMatches);
                    if (linkMatches != null && linkMatches.Count > 0)
                    {
                        foundByLink.TryAdd(uvlGame, linkMatches);
                    }

                    _gamesPerNameAndPlatform.TryGetValue($"{uvlGame.DeflatedName}#{uvlGame.PlatformSpecId?.ToLowerInvariant()}", out var nameAndPlatformMatches);

                    if (nameAndPlatformMatches is null || nameAndPlatformMatches.Count == 0)
                    {
                        _gamesPerNameAndPlatform.TryGetValue($"{uvlGame.DeflatedName}_{uvlGame.PlatformName?.ToLowerInvariant()}", out nameAndPlatformMatches);
                    }

                    if (nameAndPlatformMatches != null && nameAndPlatformMatches.Count > 0)
                    {
                        foundByNameAndPlatform.TryAdd(uvlGame, nameAndPlatformMatches);
                    }

                    _gamesPerName.TryGetValue(uvlGame.DeflatedName, out var nameMatches);
                    if (nameMatches != null && nameMatches.Count > 0)
                    {
                        foundByName.TryAdd(uvlGame, nameMatches);
                    }

                    activateGlobalProgress.CurrentProgressValue++;
                });

                //Add all found matches to the MatchedGames dictionary by priority: Link > NameAndPlatform > Name
                AddToMatchedGames(MatchingType.Link, foundByLink);
                AddToMatchedGames(MatchingType.NameAndPlatform, foundByNameAndPlatform);
                AddToMatchedGames(MatchingType.Name, foundByName);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }, globalProgressOptions);
    }

    public void Prepare()
    {
        try
        {
            Parallel.ForEach(playniteGames, new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, game =>
            {
                if (game.Links is not null && game.Links.Any())
                {
                    foreach (var link in game.Links.Where(l => l.Url.Contains("uvlist.net", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var cleanedUrl = WebHelper.CleanUpUrl(link.Url);
                        AddGameByKey(_gamesPerLink, cleanedUrl, game.Id);
                    }
                }

                var deflatedName = game.Name.FormatString(_formatParameters);

                AddGameByKey(_gamesPerName, deflatedName, game.Id);

                if (game.Platforms is not null && game.Platforms.Any())
                {
                    foreach (var platform in game.Platforms)
                    {
                        var nameAndPlatformKey = $"{deflatedName}{(platform.SpecificationId.IsNullOrEmpty() ? "_" + platform.Name.ToLower() : "#" + platform.SpecificationId.ToLower())}";
                        AddGameByKey(_gamesPerNameAndPlatform, nameAndPlatformKey, game.Id);
                    }
                }
            });
        }
        finally
        {
            _isPrepared = true;
        }
    }

    private void AddGameByKey(ConcurrentDictionary<string, IList<Guid>> dictionary, string key, Guid gameId)
    {
        dictionary.AddOrUpdate(key, [gameId], (_, existing) =>
        {
            lock (existing)
            {
                if (!existing.Contains(gameId))
                {
                    existing.Add(gameId);
                }
            }

            return existing;
        });
    }

    private void AddToMatchedGames(MatchingType matchingType, ConcurrentDictionary<UVLItemOption, IList<Guid>> foundMatches)
    {
        foreach (var match in foundMatches)
        {
            foreach (var gameId in match.Value)
            {
                if (!MatchedGames.ContainsKey(gameId))
                {
                    var game = API.Instance.Database.Games[gameId];

                    MatchedGames.Add(gameId, new MatchedGame
                    {
                        PlayniteGame = new GameEx(game)
                        {
                            Platforms = string.Join(", ", game.Platforms?.Select(x => x.Name).ToList() ?? [])
                        },
                        UVLGame = match.Key,
                        MatchingType = matchingType
                    });
                }
            }
        }
    }
}
