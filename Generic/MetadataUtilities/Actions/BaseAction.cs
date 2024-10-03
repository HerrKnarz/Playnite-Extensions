using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace MetadataUtilities.Actions
{
    public abstract class BaseAction : IBaseAction
    {
        internal readonly List<Game> _gamesAffected = new List<Game>();

        protected BaseAction(Settings settings) => Settings = settings;

        public Settings Settings { get; set; }

        public virtual bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true) => isBulkAction || Prepare(actionModifier, item, false);

        public virtual void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
            if (actionModifier != ActionModifierType.IsCombi)
            {
                MetadataFunctions.UpdateGames(_gamesAffected, Settings);
            }

            _gamesAffected.Clear();
        }

        public virtual bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
            _gamesAffected.Clear();

            return true;
        }

        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }
    }
}