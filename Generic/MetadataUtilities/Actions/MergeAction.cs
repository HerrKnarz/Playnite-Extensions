using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper;

namespace MetadataUtilities.Actions
{
    public class MergeAction : BaseAction
    {
        private static readonly object _mutex = new object();
        private static MergeAction _instance;

        private readonly List<MergeRule> _rules = new List<MergeRule>();

        private MergeAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public static MergeAction Instance(Settings settings)
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new MergeAction(settings);
                }
            }

            return _instance;
        }

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None,
            object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            bool result = item is MergeRule singleRule
                ? singleRule.Merge(game.Game)
                : _rules.Aggregate(false, (current, rule) => current | rule.Merge(game.Game));

            if (result && actionModifier != ActionModifierType.IsCombi)
            {
                API.Instance.Database.Games.Update(game.Game);
            }

            return result;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            List<MetadataObject> itemsToRemove = new List<MetadataObject>();

            foreach (MergeRule rule in _rules)
            {
                itemsToRemove.AddRange(rule.SourceObjects.Where(x =>
                    x.Id != rule.Id && x.Id != default && !itemsToRemove.Any(i =>
                        i.Type == x.Type && i.Name == x.Name)));
            }

            _rules.Clear();

            if (itemsToRemove.Count <= 0)
            {
                return;
            }

            foreach (MetadataObject itemToRemove in itemsToRemove.Where(itemToRemove => !itemToRemove.IsUsed()))
            {
                itemToRemove.RemoveFromDb();
            }
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            _rules.Clear();

            if (item is MergeRule singleRule)
            {
                _rules.Add(singleRule);
            }
            else
            {
                _rules.AddRange(Settings.MergeRules.DeepClone().ToList());
            }

            foreach (MergeRule rule in _rules)
            {
                foreach (MetadataObject sourceItem in rule.SourceObjects)
                {
                    sourceItem.RefreshId();
                }

                rule.AddToDb();
            }

            return true;
        }
    }
}