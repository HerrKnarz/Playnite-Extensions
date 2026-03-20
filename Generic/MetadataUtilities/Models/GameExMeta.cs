using KNARZhelper.GamesCommon;

namespace MetadataUtilities.Models
{
    public class GameExMeta : GameEx
    {
        public bool ExecuteConditionalActions { get; set; } = false;
        public bool ExecuteMergeRules { get; set; } = false;
        public bool ExecuteRemoveUnwanted { get; set; } = false;
    }
}
