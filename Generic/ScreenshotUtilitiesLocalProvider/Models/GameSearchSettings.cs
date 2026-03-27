using KNARZhelper.GamesCommon;

namespace ScreenshotUtilitiesLocalProvider.Models
{
    public class GameSearchSettings : IGameSearchSettings
    {
        public bool GameGridShowCompletionStatus { get; set; } = true;
        public bool GameGridShowHidden { get; set; } = true;
        public bool GameGridShowPlatform { get; set; } = true;
        public bool GameGridShowReleaseYear { get; set; } = true;
        public int GameSearchWindowHeight { get; set; } = 700;
        public int GameSearchWindowWidth { get; set; } = 700;
    }
}
