using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    public delegate void RenameObjectEventHandler(object sender, string oldName, string newName);

    public interface IEditableObjectType : IObjectType, IValueType
    {
        event RenameObjectEventHandler RenameObject;

        Guid AddDbObject(string name);

        bool RemoveDbObject(Guid id, bool checkIfUsed = true);

        bool RemoveObjectFromGame(Game game, List<Guid> ids);

        bool RemoveObjectFromGame(Game game, Guid id);

        IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id,
            FieldType? newType = null, Guid? newId = null, bool removeAfter = true);

        void UpdateDbObject(Guid id, string name);

        DbInteractionResult UpdateName(Guid id, string oldName, string newName);
    }
}