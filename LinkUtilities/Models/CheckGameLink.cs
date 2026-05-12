using CommunityToolkit.Mvvm.ComponentModel;
using Playnite;
using PlayniteExtensionHelpers.WebCommon;

namespace LinkUtilities.Models;

public partial class CheckGameLink : GameLink
{
    [ObservableProperty]
    public partial UrlLoadResult LinkCheckResult { get; set; }

    [ObservableProperty]
    public partial bool UrlIsEqual { get; set; }

    public async Task Replace()
    {
        if (Game is null || LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return;
        }

        await UIDispatcher.InvokeAsync(async delegate
        {
            Game.Links?.Single(x => x.Matches(Link)).Url = LinkCheckResult.ResponseUrl;

            await LinkUtilitiesPlugin.PlayniteApi.Library.Games.UpdateAsync(Game);
        });

        UrlIsEqual = true;
    }
}