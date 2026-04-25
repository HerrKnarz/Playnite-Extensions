using LinkUtilities.Helper;
using LinkUtilities.Interfaces;
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
internal class AddWebsiteLinks : BaseAction
{
    private static AddWebsiteLinks? _instance;
    private readonly Pipelines Pipelines = [];
    private List<BaseClasses.Linker>? _linkers;

    private AddWebsiteLinks()
    {
        Links = [];
    }

    /*
     * NEXT: Implement custom link profiles
     * public List<CustomLinkProfile> CustomLinkProfiles
    {
        get;
        set
        {
            field.Clear();
            field.AddRange(value);

            Links.RefreshCustomLinkProfiles(field);
        }
    } = new List<CustomLinkProfile>();*/

    public override string Id => "linkutilities.website.links";

    public Links Links { get; }

    //NEXT: Probably add name string to localization file for all linkers.
    public override string Name => "Website links";

    public string SteamId { get; set; } = string.Empty;

    public static AddWebsiteLinks Instance() => _instance ??= new AddWebsiteLinks();

    //NEXT: Check if the SteamId can be removed, since it's now set directly when adding a steam link.
    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        if (args is not AddWebsiteLinksArgs addArgs)
        {
            return false;
        }

        try
        {
            if (addArgs.DebugMode)
            {
                Log.Debug($"Starting {GetType().Name}{(addArgs.IsBulkAction ? " (Bulk)" : string.Empty)} of type {addArgs.AddType} for game {game.Game.Name}.");
            }

            SteamId = string.Empty;

            return addArgs.AddType switch
            {
                AddWebsiteLinkTypes.Add
                or AddWebsiteLinkTypes.AddSelected
                    => await AddLinksAsync(game.Game, addArgs.IsBulkAction),
                AddWebsiteLinkTypes.Search
                or AddWebsiteLinkTypes.SearchMissing
                or AddWebsiteLinkTypes.SearchSelected
                    => await SearchLinksAsync(game.Game, addArgs.AddType, addArgs.IsBulkAction),
                AddWebsiteLinkTypes.SearchInBrowser
                    => await SearchLinksInBrowserAsync(game.Game, addArgs.IsBulkAction),
                _ => throw new ArgumentOutOfRangeException(nameof(args), addArgs.AddType, null),
            };
        }
        finally
        {
            SteamId = string.Empty;

            if (LinkUtilitiesPlugin.Settings.DebugMode)
            {
                Log.Debug($"Finishing {GetType().Name}{(addArgs.IsBulkAction ? " (Bulk)" : string.Empty)} of type {addArgs.AddType} for game {game.Game.Name}.");
            }
        }
    }

    public override async Task FollowUpAsync(BaseActionArgs args)
    {
        await base.FollowUpAsync(args);

        Pipelines?.CleanUp();
    }

    public override AddWebsiteLinksArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        return new AddWebsiteLinksArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_adding_website_links(),
            ResultMessageId = LocId.dialog_added_links_message
        };
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        if (args is not AddWebsiteLinksArgs addArgs)
        {
            return false;
        }

        switch (addArgs.AddType)
        {
            case AddWebsiteLinkTypes.Add:
                _linkers = [.. Links.Where(x => x.Settings.IsAddable == true || (x.Settings.IsCustomSource && x.AddType != LinkAddTypes.None))];
                InitializePipelines();
                return true;

            case AddWebsiteLinkTypes.AddSelected:
                return SelectLinks();

            case AddWebsiteLinkTypes.Search:
            case AddWebsiteLinkTypes.SearchMissing:
                _linkers = [.. Links.Where(x => x.Settings.IsSearchable == true)];
                InitializePipelines(1);
                return true;

            case AddWebsiteLinkTypes.SearchInBrowser:
                _linkers = [.. Links.Where(x => x.CanBeBrowserSearched)];
                return true;

            case AddWebsiteLinkTypes.SearchSelected:
                return SelectLinks(false);

            default:
                throw new ArgumentOutOfRangeException(nameof(args), addArgs.AddType, null);
        }
    }

    public override bool ProcessUpdateData(Game gameToUpdate, BaseActionGame processedGame)
                                            => LinkHelper.UpdateGameInLibrary(gameToUpdate, processedGame);

    private async Task<bool> AddAsync(Game game)
    {
        var result = await FindLinksAsync(game);

        return result.result && result.links.HasItems() && await LinkHelper.AddLinksAsync(game, result.links);
    }

    /// <summary>
    /// Adds links to all configured websites
    /// </summary>
    /// <param name="game">game the links will be added to.</param>
    /// <param name="isBulkAction">
    /// If true, the method already is used in a progress bar and no new one has to be started.
    /// </param>
    /// <returns>True, if new links were added.</returns>
    private async Task<bool> AddLinksAsync(Game game, bool isBulkAction = true)
    {
        if (isBulkAction)
        {
            return await AddAsync(game);
        }

        //NEXT: Check if the blocking one is really needed anymore and don't use when running in background.

        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return false;
        }

        var globalProgressOptions = new GlobalProgressOptions(
            $"{Loc.link_utilities_name()}{Environment.NewLine}{Loc.progress_adding_website_links()}",
            true
        )
        {
            IsIndeterminate = true
        };

        var result = false;

        await LinkUtilitiesPlugin.PlayniteApi.Dialogs.ShowAsyncBlockingProgressAsync(globalProgressOptions,
            async (args) =>
            {
                try
                {
                    result = await AddAsync(game);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

        return result;
    }

    private void DisposePipelines()
    {
        if (Pipelines.Count == 0)
        {
            return;
        }

        _linkers?.ForEach(l => l.Pipeline = null);

        Pipelines.CleanUp();
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

        if (!_linkers.HasItems())
        {
            return (links, false);
        }

        var linksQueue = new ConcurrentQueue<WebLink>();

        foreach (var priority in _linkers.Select(l => l.Priority).Distinct())
        {
            await Parallel.ForEachAsync(Pipelines, Pipelines.ParallelOptions, async (pipeline, cancellationToken) =>
            {
                foreach (var linker in _linkers.Where(x => x.Priority == priority && x.Pipeline == pipeline).OrderBy(l => l.LinkName))
                {
                    var result = await linker.FindLinksAsync(game);

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

    private void InitializePipelines(int count = 0)
    {
        DisposePipelines();

        if (!_linkers.HasItems())
        {
            return;
        }

        Pipelines.Initialize(count == 0 ? _linkers.Count : count);

        var pipelineId = 0;

        foreach (var linker in _linkers)
        {
            linker.Pipeline = Pipelines[pipelineId];

            pipelineId++;

            if (pipelineId >= Pipelines.Count)
            {
                pipelineId = 0;
            }
        }
    }

    /// <summary>
    /// Searches links for all configured websites
    /// </summary>
    /// <param name="game">game the links will be searched for.</param>
    /// <param name="actionModifier">Kind of search (e.g. Search or SearchMissing)</param>
    /// <param name="isBulkAction">
    /// If true, the method already is used in a progress bar and no new one has to be started.
    /// </param>
    /// <returns>True, if new links were added.</returns>
    private async Task<bool> SearchLinksAsync(Game game, AddWebsiteLinkTypes addType = AddWebsiteLinkTypes.None, bool isBulkAction = true)
    {
        if (!_linkers.HasItems())
        {
            return false;
        }

        var result = false;

        if (isBulkAction)
        {
            foreach (var link in _linkers)
            {
                result |= await link.AddSearchedLinkAsync(game, addType == AddWebsiteLinkTypes.SearchMissing, false);
            }
        }
        //NEXT: Check if the blocking one is really needed anymore and don't use when running in background.
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
                        args.SetProgressMaxValue(_linkers.Count);

                        var counter = 0;

                        foreach (ILinker link in _linkers)
                        {
                            if (args.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            args.SetText($"{Loc.link_utilities_name()}{Environment.NewLine}{Loc.progress_adding_website_links()}{Environment.NewLine}{link.LinkName}");

                            result |= await link.AddSearchedLinkAsync(game, addType == AddWebsiteLinkTypes.SearchMissing, false);

                            args.SetCrrentProgressValue(++counter);
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

    /// <summary>
    /// Opens the search page for all configured websites in the standard web browser
    /// </summary>
    /// <param name="game">game the links will be searched for.</param>
    /// <param name="isBulkAction">
    /// If true, the method already is used in a progress bar and no new one has to be started.
    /// </param>
    /// <returns>True, if pages were opened.</returns>
    private async Task<bool> SearchLinksInBrowserAsync(Game game, bool isBulkAction = true)
    {
        if (!_linkers.HasItems())
        {
            return false;
        }

        var result = false;

        var linksToSearch = _linkers.Where(link => !LinkHelper.LinkExists(game, link.LinkName)).ToList();

        if (isBulkAction)
        {
            foreach (var link in linksToSearch)
            {
                link.StartBrowserSearch(game);
                result = true;
            }
        }
        //NEXT: Check if the blocking one is really needed anymore and don't use when running in background.
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
                        args.SetProgressMaxValue(linksToSearch.Count);

                        var counter = 0;

                        foreach (var link in linksToSearch)
                        {
                            args.SetText($"{Loc.link_utilities_name()}{Environment.NewLine}{Loc.progress_adding_website_links()}{Environment.NewLine}{link.LinkName}");
                            if (args.CancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            link.StartBrowserSearch(game);
                            result = true;

                            args.SetCrrentProgressValue(++counter);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });
        }

        return result;
    }

    private bool SelectLinks(bool add = true) => false;/* NEXT: Implement SelectLinks

            try
            {
                var viewModel = new SelectedLinksViewModel(Links, add);
                var window = WindowHelper.CreateSizeToContentWindow(ResourceProvider.GetString("LOCLinkUtilitiesSelectLinksWindowName"));
                var view = new SelectedLinksView(window) { DataContext = viewModel };

                window.Content = view;
                if (window.ShowDialog() != true)
                {
                    return false;
                }

                _linkers = viewModel.Links.Where(x => x.Selected).Select(x => x.Linker).ToList();

                InitializePipelines(add ? 0 : 1);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing SelectedLinksView", true);

                return false;
            }*/
}