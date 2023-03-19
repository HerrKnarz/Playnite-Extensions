﻿using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanyCompanion
{
    /// <summary>
    /// Contains all relevant data to merge similar companies to one.
    /// </summary>
    public class MergeGroup : ViewModelBase
    {
        private string companyName = string.Empty;
        private Guid companyId = Guid.Empty;

        /// <summary>
        /// Company Companion plugin. Used to access settings etc.
        /// </summary>
        public CompanyCompanion Plugin { get; set; }
        /// <summary>
        /// Owner of the group. Used to merge one group.
        /// </summary>
        public MergeCompanies Owner { get; set; }
        /// <summary>
        /// String the companies are grouped by
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Name of the masterCompany
        /// </summary>
        public string CompanyName
        {
            get
            {
                return companyName;
            }
            set
            {
                companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }
        /// <summary>
        /// Name to display
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (Plugin.Settings.Settings.ShowGroupKey)
                {
                    return $"{Key} ({Companies.Count})";
                }
                else
                {
                    return $"{CompanyName} ({Companies.Count})";
                }
            }
        }
        /// <summary>
        /// Id of the masterCompany
        /// </summary>
        public Guid CompanyId
        {
            get
            {
                return companyId;
            }
            set
            {
                companyId = value;
                OnPropertyChanged("CompanyId");
            }
        }
        /// <summary>
        /// Collection of all companies that will be merged.
        /// </summary>
        public List<MergeItem> Companies { get; set; }

        /// <summary>
        /// Switches the companies of the merge group in a game to the master company.
        /// </summary>
        /// <param name="game">game to process</param>
        /// <returns></returns>
        public bool UpdateGame(Game game)
        {
            bool updatedDeveloper = false;
            bool updatedPublisher = false;

            foreach (MergeItem company in Companies.Where(c => c.Merge))
            {
                if (company.Id != CompanyId)
                {
                    if (game.Developers != null)
                    {
                        updatedDeveloper = game.DeveloperIds.Remove(company.Id) || updatedDeveloper;
                    }

                    if (game.Publishers != null)
                    {
                        updatedPublisher = game.PublisherIds.Remove(company.Id) || updatedPublisher;
                    }
                }
            }

            if (updatedDeveloper)
            {
                game.DeveloperIds.AddMissing(CompanyId);
            }

            if (updatedPublisher)
            {
                game.PublisherIds.AddMissing(CompanyId);
            }

            return updatedDeveloper || updatedPublisher;
        }

        /// <summary>
        /// Cleans up all companies of the merge group.
        /// </summary>
        public void CleanUpCompanies()
        {
            foreach (Guid id in Companies.Where(c => c.Merge && c.Id != CompanyId).Select(c => c.Id).ToList())
            {
                API.Instance.Database.Companies.Remove(id);
            }

            Company masterCompany = API.Instance.Database.Companies.First(c => c.Id == CompanyId);

            masterCompany.Name = CompanyName;


            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                API.Instance.Database.Companies.Update(masterCompany);
            });
        }

        /// <summary>
        /// Merges this specific group.
        /// </summary>
        public RelayCommand MergeGroupCommand
        {
            get => new RelayCommand(() =>
            {
                Owner.Merge(this);

                Owner.MergeList.Remove(this);
            });
        }

    }
}
