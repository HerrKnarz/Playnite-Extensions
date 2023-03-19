﻿using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanyCompanion.ViewModels
{
    public class GameList : ViewModelBase
    {
        /// <summary>
        /// List of all games associated with the company
        /// </summary>
        public List<Game> Games { get; }
        /// <summary>
        /// Info about the games from that company (number of games and names of the first 10) to show in the window.
        /// </summary>
        public string ShortInfo { get; }
        /// <summary>
        /// List of all games for the tooltip.
        /// </summary>
        public string Tooltip { get; }

        public GameList(Guid companyId, bool isDeveloper = true)
        {
            Games = new List<Game>();

            if (isDeveloper)
            {
                Games.AddRange(API.Instance.Database.Games
                    .Where(g => g.DeveloperIds != null && g.DeveloperIds.Contains(companyId)));
            }
            else
            {
                Games.AddRange(API.Instance.Database.Games
                        .Where(g => g.PublisherIds != null && g.PublisherIds.Contains(companyId)));
            }

            string gamesShort = string.Join(", ", Games
                .OrderByDescending(g => g.Favorite)
                .ThenByDescending(g => g.Playtime)
                .ThenBy(g => (g.SortingName != "") ? g.SortingName : g.Name)
                .Select(g => g.Name)
                .Distinct()
                .Take(10)
                .ToList());

            if (Games.Count == 0)
            {
                ShortInfo = $"0 {ResourceProvider.GetString("LOCCompanyCompanionMergeWindowGames")}";
            }
            else if (Games.Count == 1)
            {
                ShortInfo = $"1 {ResourceProvider.GetString("LOCCompanyCompanionMergeWindowGame")}: {gamesShort}";
            }
            else
            {
                ShortInfo = $"{Games.Count} {ResourceProvider.GetString("LOCCompanyCompanionMergeWindowGames")}: {gamesShort}";
            }

            Tooltip = string.Join(Environment.NewLine, Games
                .OrderBy(g => (g.SortingName != null && g.SortingName != "") ? g.SortingName : g.Name)
                .Select(g => g.Name)
                .Distinct()
                .ToList());
        }
    }
}
