using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using KNARZhelper.DatabaseObjectTypes;

namespace MetadataUtilities.Actions
{
    public class AddDefaultsAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static AddDefaultsAction _instance;
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly TypeCategory _categoryType = new TypeCategory();
        private readonly List<Guid> _tagIds = new List<Guid>();
        private readonly TypeTag _tagType = new TypeTag();

        private AddDefaultsAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressAddingDefaults");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogAddedDefaultsMessage";

        public Settings Settings { get; set; }

        public static AddDefaultsAction Instance(MetadataUtilities plugin)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new AddDefaultsAction(plugin);
                }
            }

            return _instance;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            bool mustUpdate = _categoryType.AddDbObjectToGame(game, _categoryIds);

            if (!Settings.SetDefaultTagsOnlyIfEmpty || (game.TagIds?.Count != 0))
            {
                mustUpdate |= _tagType.AddDbObjectToGame(game, _tagIds);
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true)
        {
            _categoryIds.Clear();
            foreach (MetadataObject category in Settings.DefaultCategories)
            {
                _categoryIds.Add(category.AddToDb());
            }

            _tagIds.Clear();
            foreach (MetadataObject tag in Settings.DefaultTags)
            {
                _tagIds.Add(tag.AddToDb());
            }

            return _categoryIds.Count != 0 || _tagIds.Count != 0;
        }
    }
}