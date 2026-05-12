using CommunityToolkit.Mvvm.ComponentModel;
using Playnite;

namespace LinkUtilities.Models;

public partial class GameLink : ObservableObject
{
    [ObservableProperty]
    public partial Game? Game { get; set; }

    [ObservableProperty]
    public partial WebLink Link { get; set; } = new();

    public async Task Remove()
    {
        if (Game is null || LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return;
        }

        await UIDispatcher.InvokeAsync(async delegate
        {
            if (!Game.Links?.Remove(Link) ?? true)
            {
                return;
            }

            await LinkUtilitiesPlugin.PlayniteApi.Library.Games.UpdateAsync(Game);
        });
    }
}