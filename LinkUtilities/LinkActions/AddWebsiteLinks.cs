using LinkUtilities.Helper;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;
using System.Collections.Concurrent;

namespace LinkUtilities.LinkActions;

/// <summary>
/// Class to add a link to all available websites in the Links list, if a definitive link was found.
/// </summary>
public class AddWebsiteLinks : BaseWebsiteLinks
{
    public override string Id => ActionIds.TypeAddLinks;

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName, bool onlySelectedLinks = false, List<BaseLinkSource>? links = null)
    {
        var action = new AddWebsiteLinks();
        var args = action.GetActionArgs(api, games, pluginName);
        args.SelectedLinks = onlySelectedLinks;
        args.DoForAllType = DoForAllTypes.BackgroundOperation;
        args.Links = links;

        await action.DoForAllAsync(args);
    }

    public static async Task CreateAndTestAsync(IPlayniteApi api, string pluginName)
    {
        var action = new AddWebsiteLinks();

        var games = new List<BaseActionGame>()
        {
            new(new ("TestGame"))
        };

        var args = action.GetActionArgs(api, games, pluginName);
        args.DoForAllType = DoForAllTypes.BackgroundOperation;
        args.TestMode = true;

        await action.DoForAllAsync(args);
    }

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        try
        {
            if (args.DebugMode)
            {
                Log.Debug($"Starting {GetType().Name} for game {game.Game.Name}.");
            }

            var result = await FindLinksAsync(game.Game);

            return result.result && result.links.HasItems() && await LinkHelper.AddLinksAsync(game.Game, result.links);
        }
        finally
        {
            if (LinkUtilitiesPlugin.Settings.DebugMode)
            {
                Log.Debug($"Finishing {GetType().Name} for game {game.Game.Name}.");
            }
        }
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        if (!await base.PrepareAsync(args) || args is not WebsiteLinksArgs websiteArgs)
        {
            return false;
        }

        if (websiteArgs.SelectedLinks)
        {
            return SelectLinks();
        }

        if (!LinksToProcess.HasItems())
        {
            return false;
        }

        LinksToProcess = [.. LinksToProcess.Where(x => x.Settings.IsAddable == true || (x.Settings.IsCustomSource && x.AddType != LinkAddTypes.None))];

        InitializePipelines();

        return true;
    }

    /// <summary>
    /// Finds links to all configured websites. The links are added asynchronously to a
    /// ConcurrentBag and then returned as a distinct list.
    /// </summary>
    /// <param name="game">game the links will be found for.</param>
    /// <returns>List of found links and True, if links were found.</returns>
    private async Task<(List<WebLink> links, bool result)> FindLinksAsync(Game game)
    {
        var links = new List<WebLink>();

        if (!LinksToProcess.HasItems())
        {
            return (links, false);
        }

        var linksQueue = new ConcurrentQueue<WebLink>();

        foreach (var priority in LinksToProcess.Select(l => l.Priority).Distinct())
        {
            await Parallel.ForEachAsync(Pipelines, Pipelines.ParallelOptions, async (pipeline, cancellationToken) =>
            {
                foreach (var linker in LinksToProcess.Where(x => x.Priority == priority && x.Pipeline == pipeline).OrderBy(l => l.LinkName))
                {
                    // If we fetch links for only one site and a delay is configured, we wait before
                    // fetching the links. This is to avoid potential issues with rate limits or
                    // temporary blocks from the website when fetching links for multiple games in a row.
                    if ((links.Count == 1) && (linker.Delay > 0))
                    {
                        await Task.Delay(linker.Delay, cancellationToken);
                    }

                    var gameToProcess = game;
                    if (TestMode)
                    {
                        gameToProcess = new Game(game.Name);
                    }

                    var result = await linker.FindLinksAsync(gameToProcess);

                    if (!result.result || !result.links.HasItems())
                    {
                        continue;
                    }

                    result.links.ForEach(link => linksQueue.Enqueue(link));
                }
            });
        }

        var linksAdded = links.AddMissing(linksQueue.Distinct());

        return (links, linksAdded);
    }
}