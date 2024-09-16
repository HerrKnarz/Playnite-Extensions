using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public class MyGame
    {
        public bool ExecuteConditionalActions { get; set; } = false;
        public bool ExecuteMergeRules { get; set; } = false;
        public bool ExecuteRemoveUnwanted { get; set; } = false;
        public Game Game { get; set; }

        public string Platforms { get; set; }

        public string RealSortingName => string.IsNullOrEmpty(Game.SortingName) ? Game.Name : Game.SortingName;
    }
}