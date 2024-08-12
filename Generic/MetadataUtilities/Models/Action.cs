using System.Collections.Generic;

namespace MetadataUtilities.Models
{
    public enum ActionType
    {
        AddObject,
        RemoveObject,
        ClearField
    }

    public class Action : ObservableObject
    {
        private readonly Settings _settings;
        private ActionType _type = ActionType.AddObject;

        public Action(Settings settings) => _settings = settings;

        public ActionType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }
    }
}