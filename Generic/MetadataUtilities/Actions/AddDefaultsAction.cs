using KNARZhelper;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class AddDefaultsAction : BaseAction
    {
        private static AddDefaultsAction _instance;
        private static readonly object _mutex = new object();
        private readonly List<Guid> _categoryIds = new List<Guid>();
        private readonly List<Guid> _tagIds = new List<Guid>();

        private AddDefaultsAction(MetadataUtilities plugin) => Settings = plugin.Settings.Settings;

        public override string ProgressMessage { get; } = "LOCMetadataUtilitiesProgressAddingDefaults";

        public override string ResultMessage { get; } = "LOCMetadataUtilitiesDialogAddedDefaultsMessage";

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

        public override bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            _categoryIds.Clear();

            foreach (MetadataObject category in Settings.DefaultCategories)
            {
                _categoryIds.Add(DatabaseObjectHelper.AddDbObject(FieldType.Category, category.Name));
            }

            _tagIds.Clear();
            foreach (MetadataObject tag in Settings.DefaultTags)
            {
                _tagIds.Add(DatabaseObjectHelper.AddDbObject(FieldType.Tag, tag.Name));
            }

            return _categoryIds.Count != 0 || _tagIds.Count != 0;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            bool mustUpdate = DatabaseObjectHelper.AddDbObjectToGame(game, FieldType.Category, _categoryIds);

            if (!Settings.SetDefaultTagsOnlyIfEmpty || (game.TagIds?.Count != 0))
            {
                mustUpdate |= DatabaseObjectHelper.AddDbObjectToGame(game, FieldType.Tag, _tagIds);
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }
    }
}