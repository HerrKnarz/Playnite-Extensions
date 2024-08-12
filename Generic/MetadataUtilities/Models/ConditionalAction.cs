using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private bool _canBeExecutedManually = false;
        private ObservableCollection<Condition> _conditions = new ObservableCollection<Condition>();
        private bool _ignoreConditionOnManual = false;
        private string _name = string.Empty;
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
    }
}