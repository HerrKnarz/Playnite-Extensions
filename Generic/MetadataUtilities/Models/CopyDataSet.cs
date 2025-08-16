using KNARZhelper;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class CopyDataSet : ObservableObject
    {
        private ObservableCollection<CopyField> _fields = new ObservableCollection<CopyField>();

        public CopyDataSet(Game sourceGame, params CopyField[] metadataFields)
        {
            SourceGame = sourceGame;
            _fields.AddMissing(metadataFields);
        }

        public CopyDataSet(Game sourceGame)
        {
            SourceGame = sourceGame;

            foreach (var fieldType in FieldTypeHelper.GetAllTypes().Where(x => x.CanBeSetInGame))
            {
                _fields.Add(new CopyField(fieldType.Type));
            }
        }

        public bool CopyToGame(Game targetGame)
        {
            var mustUpdate = false;

            foreach (var field in Fields)
            {
                mustUpdate |= field.CopyToGame(SourceGame, targetGame, field.ReplaceData, field.OnlyIfEmpty);
            }

            return mustUpdate;
        }

        [DontSerialize]
        public Game SourceGame { get; set; }

        public ObservableCollection<CopyField> Fields
        {
            get => _fields;
            set => SetValue(ref _fields, value);
        }
    }
}
