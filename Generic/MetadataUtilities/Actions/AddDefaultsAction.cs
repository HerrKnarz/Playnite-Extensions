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

        public override string ProgressMessage { get; } = "LOCMetadataUtilitiesDialogAddedDefaultsMessage";

        public override string ResultMessage { get; } = "LOCMetadataUtilitiesDialogAddedMessage";

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

            foreach (MetadataListObject category in Settings.DefaultCategories)
            {
                _categoryIds.Add(DatabaseObjectHelper.AddDbObject(FieldType.Category, category.Name));
            }

            _tagIds.Clear();
            foreach (MetadataListObject tag in Settings.DefaultTags)
            {
                _tagIds.Add(DatabaseObjectHelper.AddDbObject(FieldType.Tag, tag.Name));
            }

            return _categoryIds.Any() || _tagIds.Any();
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, isBulkAction))
            {
                return false;
            }

            bool mustUpdate = DatabaseObjectHelper.AddDbObjectToGame(game, FieldType.Category, _categoryIds);

            if (!Settings.SetDefaultTagsOnlyIfEmpty || (game.TagIds?.Any() ?? false))
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