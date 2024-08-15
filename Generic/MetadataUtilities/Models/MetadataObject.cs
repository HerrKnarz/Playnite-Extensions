using KNARZhelper;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MetadataObject : DatabaseObject
    {
        internal readonly Settings _settings;
        internal string _name = string.Empty;
        internal FieldType _type;
        private int _gameCount;
        private bool _selected;

        public MetadataObject(Settings settings) => this._settings = settings;

        [DontSerialize]
        public int GameCount
        {
            get => _gameCount;
            set => SetValue(ref _gameCount, value);
        }

        [DontSerialize]
        public new Guid Id { get; set; }

        public new string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [DontSerialize]
        public bool Selected
        {
            get => _selected;
            set => SetValue(ref _selected, value);
        }

        public FieldType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }

        [DontSerialize]
        public string TypeAndName => Name.Any() ? $"{_type.GetEnumDisplayName()}: {Name}" : _type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeAsString => _type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeLabel => _type.GetEnumDisplayName();

        public void GetGameCount() => GameCount = DatabaseObjectHelper.GetGameCount(_type, Id, _settings.IgnoreHiddenGamesInGameCount);
    }
}