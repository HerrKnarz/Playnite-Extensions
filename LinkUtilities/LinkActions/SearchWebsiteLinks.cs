using LinkUtilities.Linker;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public class SearchWebsiteLinks : BaseWebsiteLinks
{
    // NEXT: Think about adding test cases for search results.

    public override string Id => ActionIds.TypeSearchLinks;

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName, bool onlyMissingLinks, List<BaseLinkSource>? links = null)
    {
        var action = new SearchWebsiteLinks();

        var args = action.GetActionArgs(api, games, pluginName);
        args.OnlyMissingLinks = onlyMissingLinks;
        args.Links = links;
        args.UpdateGamesAfterLoop = true;

        await action.DoForAllAsync(args);
    }

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        if (args is not WebsiteLinksArgs addArgs)
        {
            return false;
        }

        try
        {
            if (addArgs.DebugMode)
            {
                Log.Debug($"Starting {GetType().Name} for game {game.Game.Name}.");
            }

            if (!LinksToProcess.HasItems())
            {
                return false;
            }

            var result = false;

            foreach (var link in LinksToProcess)
            {
                result |= await link.GetSearchedLinkAsync(game.Game, addArgs.OnlyMissingLinks);
            }

            return result;
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

        LinksToProcess = [.. LinksToProcess.Where(x => x.Settings.IsSearchable == true)];

        InitializePipelines(1);

        return true;
    }
}