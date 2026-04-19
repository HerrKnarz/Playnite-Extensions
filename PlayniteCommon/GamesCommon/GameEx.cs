using Playnite;

namespace PlayniteCommon.GamesCommon;

public class GameEx(Game game)
{
    public Game Game { get; set; } = game;

    public string? Platforms { get; set; }

    public string RealSortingName => Game.SortingName.IsNullOrEmpty() ? Game.Name : Game.SortingName;
}