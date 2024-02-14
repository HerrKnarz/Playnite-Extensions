using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public class MyGame
    {
        public Game Game { get; set; }

        public string Platforms { get; set; }

        public string RealSortingName => string.IsNullOrEmpty(Game.SortingName) ? Game.Name : Game.SortingName;
    }
}