using KNARZhelper;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;

namespace MetadataUtilities.Models
{
    public class MetadataObject : DatabaseObject
    {
        protected readonly Settings settings;
        private int _gameCount;
        protected string name = string.Empty;
        protected FieldType type;

        public MetadataObject(Settings settings) => this.settings = settings;

        [DontSerialize]
        public new Guid Id { get; set; }

        public FieldType Type
        {
            get => type;
            set => SetValue(ref type, value);
        }

        [DontSerialize]
        public int GameCount
        {
            get => _gameCount;
            set => SetValue(ref _gameCount, value);
        }

        public new string Name
        {
            get => name;
            set => SetValue(ref name, value);
        }

        [DontSerialize]
        public string TypeAndName => $"{type.GetEnumDisplayName()}: {Name}";

        [DontSerialize]
        public string TypeAsString => type.GetEnumDisplayName();

        [DontSerialize]
        public string TypeLabel => type.GetEnumDisplayName();

        public void GetGameCount() => GameCount = DatabaseObjectHelper.GetGameCount((FieldType)type, Id, settings.IgnoreHiddenGamesInGameCount);
    }
}