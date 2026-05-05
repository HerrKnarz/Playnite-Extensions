using LinkUtilities.Linker;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public class SearchWebsiteLinks : BaseWebsiteLinks
{
    public override string Id => ActionIds.TypeSearchLinks;

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName, bool onlyMissingLinks)
    {
        var action = new AddWebsiteLinks();

        var args = action.GetActionArgs(api, games, pluginName);
        args.OnlyMissingLinks = onlyMissingLinks;

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
                Log.Debug($"Starting {GetType().Name}{(addArgs.IsBulkAction ? " (Bulk)" : string.Empty)} for game {game.Game.Name}.");
            }

            return await SearchLinksAsync(game.Game, addArgs.OnlyMissingLinks, addArgs.IsBulkAction);
        }
        finally
        {
            if (LinkUtilitiesPlugin.Settings.DebugMode)
            {
                Log.Debug($"Finishing {GetType().Name}{(addArgs.IsBulkAction ? " (Bulk)" : string.Empty)} for game {game.Game.Name}.");
            }
        }
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        if (!await base.PrepareAsync(args) || args is not WebsiteLinksArgs addArgs)
        {
            return false;
        }

        await Links.InitializeAsync();

        if (addArgs.SelectedLinks)
        {
            return SelectLinks(false);
        }
        else
        {
            LinksToProcess = [.. Links.Where(x => x.Settings.IsSearchable == true)];
            InitializePipelines(1);
            return true;
        }
    }

    /// <summary>
    /// Searches links for all configured websites
    /// </summary>
    /// <param name="game">game the links will be searched for.</param>
    /// <param name="onlyMissingLinks">Indicates whether to search for missing links only.</param>
    /// <param name="isBulkAction">
    /// If true, the method already is used in a progress bar and no new one has to be started.
    /// </param>
    /// <returns>True, if new links were added.</returns>
    private async Task<bool> SearchLinksAsync(Game game, bool onlyMissingLinks = true, bool isBulkAction = true)
    {
        if (!LinksToProcess.HasItems())
        {
            return false;
        }

        var result = false;

        if (isBulkAction)
        {
            foreach (var link in LinksToProcess)
            {
                result |= await link.AddSearchedLinkAsync(game, onlyMissingLinks, false);
            }
        }
        else
        {
            if (LinkUtilitiesPlugin.PlayniteApi is null)
            {
                return false;
            }

            var globalProgressOptions = new GlobalProgressOptions($"{Loc.link_utilities_name()}{Environment.NewLine}{Loc.progress_adding_website_links()}", true)
            {
                IsIndeterminate = false
            };

            await LinkUtilitiesPlugin.PlayniteApi.Dialogs.ShowAsyncBlockingProgressAsync(globalProgressOptions,
                async (args) =>
                {
                    try
                    {
                        args.SetProgressMaxValue(LinksToProcess.Count);

                        var counter = 0;

                        foreach (var link in LinksToProcess)
                        {
                            if (args.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            args.SetText($"{Loc.link_utilities_name()}{Environment.NewLine}{Loc.progress_adding_website_links()}{Environment.NewLine}{link.LinkName}");

                            result |= await link.AddSearchedLinkAsync(game, onlyMissingLinks, false);

                            args.SetCurrentProgressValue(++counter);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });
        }

        if (result)
        {
            //NEXT: Implement DoAfterChange
            //await DoAfterChange.Instance().ExecuteAsync(game, actionModifier, isBulkAction);
        }

        return result;
    }
}