using Playnite;

namespace PlayniteExtensionHelpers.GamesCommon;

public class BaseActionGame
{
    private readonly IPlayniteApi? _api;

    public BaseActionGame(Game game)
    {
        Game = game;
        GameId = game.Id;
    }

    public BaseActionGame(string gameId, IPlayniteApi api)
    {
        GameId = gameId;
        _api = api;
    }

    public Game Game
    {
        get
        {
            if (field is null && _api is not null)
            {
                field = _api.Library.Games.Get(GameId);
            }

            field ??= new Game();

            return field;
        }
        set;
    }

    public string GameId { get; set; }

    public bool NeedsToBeUpdated { get; set; } = false;

    public bool Processed { get; set; } = false;
}