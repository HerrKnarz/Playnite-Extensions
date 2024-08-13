using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public enum LogicType
    {
        And,
        Or,
        Nand,
        Nor,
        Manual
    }

    public class ConditionalAction : ObservableObject
    {
        private readonly Settings _settings;
        private ObservableCollection<Action> _actions = new ObservableCollection<Action>();
        private bool _canBeExecutedManually;
        private ObservableCollection<Condition> _conditions = new ObservableCollection<Condition>();
        private bool _ignoreConditionOnManual;
        private string _name = string.Empty;
        private int _sortNo = 0;
        private LogicType _type = LogicType.And;

        public ConditionalAction(Settings settings) => _settings = settings;

        public ObservableCollection<Action> Actions
        {
            get => _actions;
            set => SetValue(ref _actions, value);
        }

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
            set
            {
                SetValue(ref _type, value);

                if (value == LogicType.Manual)
                {
                    _canBeExecutedManually = true;
                }
            }
        }

        public bool CheckAndExecute(Game game, bool isManual = false) =>
            ((isManual && IgnoreConditionOnManual) || CheckConditions(game)) && Execute(game);

        public bool CheckConditions(Game game)
        {
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

                case LogicType.Manual:
                    return CanBeExecutedManually;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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