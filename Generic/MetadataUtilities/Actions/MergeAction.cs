using KNARZhelper;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class MergeAction : BaseAction
    {
        private static MergeAction _instance;

        private readonly List<MergeRule> _rules = new List<MergeRule>();

        private MergeAction(Settings settings) : base(settings)
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public static MergeAction Instance(Settings settings) => _instance ?? (_instance = new MergeAction(settings));

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None,
            object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var result = item is MergeRule singleRule
                ? singleRule.Merge(game.Game)
                : _rules.Aggregate(false, (current, rule) => current | rule.Merge(game.Game));

            if (result && actionModifier != ActionModifierType.IsCombi)
            {
                _gamesAffected.Add(game.Game);
            }

            return result;
        }

        public override void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            base.FollowUp(actionModifier, item, isBulkAction);

            var itemsToRemove = new List<MetadataObject>();

            foreach (var rule in _rules)
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

            foreach (var itemToRemove in itemsToRemove.Where(itemToRemove => !itemToRemove.IsUsed()))
            {
                itemToRemove.RemoveFromDb();
            }
        }

        public override bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null, bool isBulkAction = true)
        {
            if (!base.Prepare(actionModifier, item, isBulkAction))
            {
                return false;
            }

            _rules.Clear();

            if (item is MergeRule singleRule)
            {
                _rules.Add(singleRule);
            }
            else
            {
                _rules.AddRange(Settings.MergeRules.DeepClone().ToList());
            }

            foreach (var rule in _rules)
            {
                foreach (var sourceItem in rule.SourceObjects)
                {
                    sourceItem.RefreshId();
                }

                rule.AddToDb();
            }

            return true;
        }
    }
}