using LinkUtilities.Helper;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public enum LinkActionType
{
    Default,
    Add,
    Search,
    BrowserSearch
}

public class WebsiteLinksArgs(string id, string name, IPlayniteApi api, List<BaseActionGame> games, string pluginName) : BaseActionArgs(id, name, api, games, pluginName)
{
    public LinkActionType ActionType { get; set; } = LinkActionType.Default;
    public bool OnlyMissingLinks { get; set; } = false;
    public bool SelectedLinks { get; set; } = false;
    public bool TestMode { get; set; } = false;
}

public abstract class BaseWebsiteLinks : BaseAction
{
    public BaseWebsiteLinks()
    {
        Links = [];
    }

    public override string Id => "linkutilities.website.links";

    public Links Links { get; }

    public List<BaseLinkSource>? LinksToProcess { get; set; }

    public override string Name => Loc.action_name_website_links();

    public Pipelines Pipelines { get; } = [];

    /// <summary>
    /// Determines if the action is in test mode. In that case the test cases will be executed
    /// instead of the actual game.
    /// </summary>
    public virtual bool TestMode { get; set; } = false;

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

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args) => args is WebsiteLinksArgs;

    public override async Task FollowUpAsync(BaseActionArgs args)
    {
        await base.FollowUpAsync(args);

        Pipelines?.CleanUp();

        if (TestMode && LinksToProcess.HasItems())
        {
            LinksToProcess.ForEach(l => l.TestResultQueue.ForEach(r => Log.Debug(r)));
        }
    }

    public override WebsiteLinksArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        return new WebsiteLinksArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_adding_website_links(),
            ResultMessageId = LocId.dialog_added_links_message
        };
    }

    public void InitializePipelines(int count = 0)
    {
        DisposePipelines();

        if (!LinksToProcess.HasItems())
        {
            return;
        }

        Pipelines.Initialize(count == 0 ? LinksToProcess.Count : count);

        var pipelineId = 0;

        foreach (var linker in LinksToProcess)
        {
            linker.Pipeline = Pipelines[pipelineId];

            pipelineId++;

            if (pipelineId >= Pipelines.Count)
            {
                pipelineId = 0;
            }
        }
    }

    public override async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        if (args is not WebsiteLinksArgs addArgs)
        {
            return false;
        }

        await Links.InitializeAsync();

        if (addArgs.TestMode)
        {
            TestMode = true;

            Links.ForEach(l => l.TestMode = true);
        }

        return true;
    }

    public override async Task<bool> ProcessUpdateDataAsync(Game gameToUpdate, BaseActionGame processedGame)
        => TestMode || await LinkHelper.UpdateGameInLibraryAsync(gameToUpdate, processedGame);

    public bool SelectLinks(bool add = true) => false;

    /* NEXT: Implement SelectLinks

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

                Linkers = viewModel.Links.Where(x => x.Selected).Select(x => x.Linker).ToList();

                InitializePipelines(add ? 0 : 1);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing SelectedLinksView", true);

                return false;
            }*/

    private void DisposePipelines()
    {
        if (Pipelines.Count == 0)
        {
            return;
        }

        LinksToProcess?.ForEach(l => l.Pipeline = null);

        Pipelines.CleanUp();
    }
}