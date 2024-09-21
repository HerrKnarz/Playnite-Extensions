using KNARZhelper.DatabaseObjectTypes;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;

namespace MetadataUtilities.Actions
{
    public class AddDefaultsAction : BaseAction
    {
        private static AddDefaultsAction _instance;
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly TypeCategory _categoryType = new TypeCategory();
        private readonly List<Guid> _tagIds = new List<Guid>();
        private readonly TypeTag _tagType = new TypeTag();

        private AddDefaultsAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressAddingDefaults");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogAddedDefaultsMessage";

        public static AddDefaultsAction Instance(Settings settings) => _instance ?? (_instance = new AddDefaultsAction(settings));

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var mustUpdate = _categoryType.AddValueToGame(game.Game, _categoryIds);

            if (!Settings.SetDefaultTagsOnlyIfEmpty || (game.Game.TagIds?.Count != 0))
            {
                mustUpdate |= _tagType.AddValueToGame(game.Game, _tagIds);
            }

            if (mustUpdate)
            {
                _gamesAffected.Add(game.Game);
            }

            return mustUpdate;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
            base.FollowUp(actionModifier, item, isBulkAction);

            _categoryIds.Clear();
            _tagIds.Clear();
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Prepare(actionModifier, item, isBulkAction))
            {
                return false;
            }

            _categoryIds.Clear();
            foreach (var category in Settings.DefaultCategories)
            {
                _categoryIds.Add(category.AddToDb());
            }

            _tagIds.Clear();
            foreach (var tag in Settings.DefaultTags)
            {
                _tagIds.Add(tag.AddToDb());
            }

            return _categoryIds.Count != 0 || _tagIds.Count != 0;
        }
    }
}