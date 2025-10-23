﻿using KNARZhelper;
using KNARZhelper.MetadataCommon;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Actions
{
    public class MergeAction : BaseAction
    {
        private static MergeAction _instance;

        private readonly List<MergeRule> _rules = new List<MergeRule>();

        private MergeAction()
        {
        }

        public override string ProgressMessage => ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems");

        public override string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public static MergeAction Instance() => _instance ?? (_instance = new MergeAction());

        public override bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None,
            object item = null, bool isBulkAction = true)
        {
            if (!base.Execute(game, actionModifier, item, isBulkAction))
            {
                return false;
            }

            var types = FieldTypeHelper.GetItemListTypes().ToList();

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"==== Started executing Merge Rules on Game \"{game.Game.Name}\" ======================================");

                foreach (var type in types)
                {
                    Log.Debug($"{type.LabelPlural} before: {string.Join(", ", type.LoadGameMetadata(game.Game).Select(x => x.Name))}");
                }
            }

            var result = item is MergeRule singleRule
                ? singleRule.Merge(game.Game)
                : _rules.Aggregate(false, (current, rule) => current | rule.Merge(game.Game));

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"==== Finished executing Merge Rules on Game \"{game.Game.Name}\" ({result}) ============================");

                foreach (var type in types)
                {
                    Log.Debug($"{type.LabelPlural} after: {string.Join(", ", type.LoadGameMetadata(game.Game).Select(x => x.Name))}");
                }
            }

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
            try
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

                if (_rules.Count == 0)
                {
                    return false;
                }

                foreach (var rule in _rules)
                {
                    foreach (var sourceItem in rule.SourceObjects)
                    {
                        sourceItem.RefreshId();
                    }

                    rule.AddToDb();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }

            return true;
        }
    }
}