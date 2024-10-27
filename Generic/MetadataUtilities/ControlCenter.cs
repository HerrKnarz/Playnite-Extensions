using MetadataUtilities.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities
{
    public class ControlCenter
    {
        private readonly MetadataUtilities _plugin;
        private readonly Settings _settings;

        public ControlCenter(MetadataUtilities plugin)
        {
            _plugin = plugin;
            Instance = this;
        }

        public ControlCenter(Settings settings)
        {
            _settings = settings;
            Instance = this;
        }

        public static ControlCenter Instance { get; set; }

        public bool IsUpdating { get; set; } = false;

        public HashSet<Guid> KnownGames { get; private set; }

        public HashSet<Guid> NewGames { get; } = new HashSet<Guid>();

        public Settings Settings => _plugin?.Settings?.Settings ?? _settings;

        public bool AddNewGame(Guid id) => NewGames.Add(id);

        public void GetKnownGames()
        {
            if (KnownGames != null)
            {
                return;
            }

            KnownGames = API.Instance.Database.Games.Select(x => x.Id).Distinct().ToHashSet();
        }

        public void ResetNewGames()
        {
            KnownGames.UnionWith(NewGames);
            NewGames.Clear();
        }

        public void SavePluginSettings() => _plugin?.SavePluginSettings(Settings);
    }
}
