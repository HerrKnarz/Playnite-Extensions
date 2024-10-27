using KNARZhelper;
using MetadataUtilities.Enums;
using MetadataUtilities.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MetadataUtilities.Actions
{
    public abstract class BaseAction : IBaseAction
    {
        internal readonly List<Game> _gamesAffected = new List<Game>();

        public Settings Settings => ControlCenter.Instance.Settings;

        public void DoForAll(List<MyGame> games, bool showDialog = false,
            ActionModifierType actionModifier = ActionModifierType.None, object item = null)
        {
            ControlCenter.Instance.IsUpdating = true;
            var gamesAffected = 0;

            if (Settings.WriteDebugLog)
            {
                Log.Debug($"===> Started {GetType()} for {games.Count} games. =======================");
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (games.Count == 1)
                {
                    if (Execute(games.First(), actionModifier, item, false))
                    {
                        gamesAffected++;
                    }

                    FollowUp(actionModifier, item, false);
                }
                // if we have more than one game in the list, we want to start buffered mode and
                // show a progress bar.
                else if (games.Count > 1)
                {
                    using (API.Instance.Database.BufferedUpdate())
                    {
                        if (!Prepare(actionModifier, item))
                        {
                            return;
                        }

                        var globalProgressOptions = new GlobalProgressOptions(
                            $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")} - {ProgressMessage}",
                            true
                        )
                        {
                            IsIndeterminate = false
                        };

                        API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                        {
                            try
                            {
                                activateGlobalProgress.ProgressMaxValue = games.Count;

                                foreach (var game in games)
                                {
                                    activateGlobalProgress.Text =
                                        $"{ResourceProvider.GetString("LOCMetadataUtilitiesName")}{Environment.NewLine}{ProgressMessage}{Environment.NewLine}{game.Game.Name}";

                                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    if (Execute(game, actionModifier, item))
                                    {
                                        gamesAffected++;
                                    }

                                    activateGlobalProgress.CurrentProgressValue++;
                                }

                                FollowUp(actionModifier, item);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                            }
                        }, globalProgressOptions);
                    }

                    // Shows a dialog with the number of games actually affected.
                    if (!showDialog)
                    {
                        return;
                    }

                    Cursor.Current = Cursors.Default;
                    API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString(ResultMessage), gamesAffected));
                }
            }
            finally
            {
                if (Settings.WriteDebugLog)
                {
                    Log.Debug($"===> Finished {GetType()} with {gamesAffected} games affected. =======================");
                }

                ControlCenter.Instance.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }
        }

        public virtual bool Execute(MyGame game, ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true) => isBulkAction || Prepare(actionModifier, item, false);

        public virtual void FollowUp(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
            if (actionModifier != ActionModifierType.IsCombi)
            {
                ControlCenter.UpdateGames(_gamesAffected);
            }

            _gamesAffected.Clear();
        }

        public virtual bool Prepare(ActionModifierType actionModifier = ActionModifierType.None, object item = null,
            bool isBulkAction = true)
        {
            _gamesAffected.Clear();

            return true;
        }

        public abstract string ProgressMessage { get; }

        public abstract string ResultMessage { get; }
    }
}