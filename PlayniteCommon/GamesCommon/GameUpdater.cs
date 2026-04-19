using Playnite;

namespace PlayniteCommon.GamesCommon;

public class GameUpdater
{
    public static async Task UpdateGamesAsync<T>(List<T> games, IPlayniteApi api, bool debugMode = false)
    {
        if (!games.HasItems())
        {
            return;
        }

        var gamesToUpdate = new List<Game>();

        switch (games)
        {
            case List<Game> listOfGames:
                gamesToUpdate = listOfGames;
                break;

            case List<string> gameIds:
                {
                    foreach (var gameId in gameIds)
                    {
                        var game = api.Library.Games.Get(gameId);

                        if (game is not null)
                        {
                            gamesToUpdate.AddMissing(game);
                        }
                    }

                    break;
                }
        }

        if (debugMode)
        {
            Log.Debug($"Updating {gamesToUpdate.Count} games:\n{string.Join("\n", gamesToUpdate.Select(g => g.Name))}");
        }

        if (gamesToUpdate.Count == 0)
        {
            return;
        }

        await UIDispatcher.Invoke(async delegate
        {
            await api.Library.Games.UpdateAsync(gamesToUpdate);
        });
    }
}