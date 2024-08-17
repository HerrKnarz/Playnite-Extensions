using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public enum LogicType
    {
        And,
        Or,
        Nand,
        Nor
    }

    public class ConditionalAction : ObservableObject
    {
        private readonly Settings _settings;
        private ObservableCollection<Action> _actions = new ObservableCollection<Action>();
        private bool _canBeExecutedManually;
        private ObservableCollection<Condition> _conditions = new ObservableCollection<Condition>();
        private bool _enabled = true;
        private bool _ignoreConditionOnManual;
        private string _name = string.Empty;
        private int _sortNo;
        private LogicType _type = LogicType.And;

        public ConditionalAction(Settings settings) => _settings = settings;

        public ObservableCollection<Action> Actions
        {
            get => _actions;
            set => SetValue(ref _actions, value);
        }

        [DontSerialize]
        public string ActionString => string.Join("\n", Actions.Select(x => x.ToString).ToArray());

        public bool CanBeExecutedManually
        {
            get => _canBeExecutedManually;
            set => SetValue(ref _canBeExecutedManually, value);
        }

        public ObservableCollection<Condition> Conditions
        {
            get => _conditions;
            set => SetValue(ref _conditions, value);
        }

        [DontSerialize]
        public string ConditionString => string.Join("\n", Conditions.Select(x => x.ToString).ToArray());

        public bool Enabled
        {
            get => _enabled;
            set => SetValue(ref _enabled, value);
        }

        public bool IgnoreConditionOnManual
        {
            get => _ignoreConditionOnManual;
            set => SetValue(ref _ignoreConditionOnManual, value);
        }

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public int SortNo
        {
            get => _sortNo;
            set => SetValue(ref _sortNo, value);
        }

        public LogicType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }

        [DontSerialize]
        public string TypeString => Type.GetEnumDisplayName();

        public bool CheckAndExecute(Game game, bool isManual = false) =>
            ((isManual && IgnoreConditionOnManual) || CheckConditions(game)) && Execute(game);

        public bool CheckConditions(Game game)
        {
            if (!Conditions.Any())
            {
                return true;
            }

            switch (Type)
            {
                case LogicType.And:
                    return Conditions.All(x => x.IsTrue(game));

                case LogicType.Or:
                    return Conditions.Any(x => x.IsTrue(game));

                case LogicType.Nand:
                    return !Conditions.All(x => x.IsTrue(game));

                case LogicType.Nor:
                    return !Conditions.Any(x => x.IsTrue(game));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool GetIds()
        {
            foreach (Condition item in Conditions)
            {
                item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, item.Type);
            }

            foreach (Action item in Actions)
            {
                item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, (FieldType)item.Type);
            }

            return true;
        }

        private bool Execute(Game game)
        {
            bool mustUpdate = Actions.OrderBy(x => x.ActionType == ActionType.ClearField ? 1 : 2)
                .Aggregate(false, (current, action) => current | action.Execute(game));

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }
    }
}