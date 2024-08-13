using KNARZhelper;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class MetadataObject : DatabaseObject
    {
        private protected readonly Settings _settings;
        private int _gameCount;
        private protected string _name = string.Empty;
        private protected FieldType _type;

        public MetadataObject(Settings settings) => this._settings = settings;

        [DontSerialize]
        public new Guid Id { get; set; }

        public FieldType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }

        [DontSerialize]
        public int GameCount
        {
            get => _gameCount;
            set => SetValue(ref _gameCount, value);
        }

        public new string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        [DontSerialize]
        public string TypeAndName => $"{_type.GetEnumDisplayName()}: {Name}";

        [DontSerialize]
        public string TypeAsString => _type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeLabel => _type.GetEnumDisplayName();

        public void GetGameCount() => GameCount = DatabaseObjectHelper.GetGameCount(_type, Id, _settings.IgnoreHiddenGamesInGameCount);
    }
}