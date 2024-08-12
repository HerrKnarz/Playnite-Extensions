using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace MetadataUtilities.Actions
{
    public class AddDefaultsAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static AddDefaultsAction _instance;
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly List<Guid> _tagIds = new List<Guid>();

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

            bool mustUpdate = DatabaseObjectHelper.AddDbObjectToGame(game, SettableFieldType.Category, _categoryIds);

            if (!Settings.SetDefaultTagsOnlyIfEmpty || (game.TagIds?.Count != 0))
            {
                mustUpdate |= DatabaseObjectHelper.AddDbObjectToGame(game, SettableFieldType.Tag, _tagIds);
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

            foreach (SettableMetadataObject category in Settings.DefaultCategories)
            {
                _categoryIds.Add(DatabaseObjectHelper.AddDbObject(SettableFieldType.Category, category.Name));
            }

            _tagIds.Clear();
            foreach (SettableMetadataObject tag in Settings.DefaultTags)
            {
                _tagIds.Add(DatabaseObjectHelper.AddDbObject(SettableFieldType.Tag, tag.Name));
            }

            return _categoryIds.Count != 0 || _tagIds.Count != 0;
        }
    }
}