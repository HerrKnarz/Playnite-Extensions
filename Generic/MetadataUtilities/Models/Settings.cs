using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KNARZhelper.Enum;

namespace MetadataUtilities.Models
{
    public class Settings : ObservableObject
    {
        private bool _addRemovedToUnwanted;
        private bool _alwaysSaveManualMergeRules;
        private ObservableCollection<ConditionalAction> _conditionalActions = new ObservableCollection<ConditionalAction>();
        private MetadataObjects _defaultCategories;
        private MetadataObjects _defaultTags;
        private int _editorWindowHeight = 600;
        private int _editorWindowWidth = 1200;
        private bool _filterAgeRatings = true;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private ObservableCollection<FilterType> _filterTypes = new ObservableCollection<FilterType>();
        private bool _gameGridShowCompletionStatus = true;
        private bool _gameGridShowPlatform = true;
        private bool _gameGridShowReleaseYear = true;
        private int _gameSearchWindowHeight = 700;
        private int _gameSearchWindowWidth = 700;
        private bool _ignoreHiddenGamesInGameCount;
        private bool _ignoreHiddenGamesInRemoveUnused;
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private bool _mergeMetadataOnMetadataUpdate;
        private MergeRules _mergeRules = new MergeRules();
        private ObservableCollection<string> _prefixes = new ObservableCollection<string>();
        private string _quickAddCustomPath = string.Empty;
        private ObservableCollection<QuickAddObject> _quickAddObjects = new ObservableCollection<QuickAddObject>();
        private bool _quickAddShowDialog = true;
        private bool _quickAddSingleMenuEntry;
        private bool _removeUnusedOnStartup;
        private bool _removeUnwantedOnMetadataUpdate = true;
        private bool _renameDefaults = true;
        private bool _renameMergeRules = true;
        private bool _setDefaultTagsOnlyIfEmpty = true;
        private bool _showTopPanelButton = true;
        private bool _showTopPanelSettingsButton = false;
        private MetadataObjects _unusedItemsWhiteList;
        private MetadataObjects _unwantedItems;
        private bool _writeDebugLog;

        public Settings()
        {
            _defaultCategories = new MetadataObjects(this, FieldType.Category);
            _defaultTags = new MetadataObjects(this, FieldType.Tag);
            _unusedItemsWhiteList = new MetadataObjects(this);
            _unwantedItems = new MetadataObjects(this);
        }

        public bool AddRemovedToUnwanted
        {
            get => _addRemovedToUnwanted;
            set => SetValue(ref _addRemovedToUnwanted, value);
        }

        public bool AlwaysSaveManualMergeRules
        {
            get => _alwaysSaveManualMergeRules;
            set => SetValue(ref _alwaysSaveManualMergeRules, value);
        }

        public ObservableCollection<ConditionalAction> ConditionalActions
        {
            get => _conditionalActions;
            set => SetValue(ref _conditionalActions, value);
        }

        public MetadataObjects DefaultCategories
        {
            get => _defaultCategories;
            set => SetValue(ref _defaultCategories, value);
        }

        public MetadataObjects DefaultTags
        {
            get => _defaultTags;
            set => SetValue(ref _defaultTags, value);
        }

        public int EditorWindowHeight
        {
            get => _editorWindowHeight;
            set => SetValue(ref _editorWindowHeight, value);
        }

        public int EditorWindowWidth
        {
            get => _editorWindowWidth;
            set => SetValue(ref _editorWindowWidth, value);
        }

        public bool FilterAgeRatings
        {
            get => _filterAgeRatings;
            set => SetValue(ref _filterAgeRatings, value);
        }

        public bool FilterCategories
        {
            get => _filterCategories;
            set => SetValue(ref _filterCategories, value);
        }

        public bool FilterFeatures
        {
            get => _filterFeatures;
            set => SetValue(ref _filterFeatures, value);
        }

        public bool FilterGenres
        {
            get => _filterGenres;
            set => SetValue(ref _filterGenres, value);
        }

        public bool FilterSeries
        {
            get => _filterSeries;
            set => SetValue(ref _filterSeries, value);
        }

        public bool FilterTags
        {
            get => _filterTags;
            set => SetValue(ref _filterTags, value);
        }

        public ObservableCollection<FilterType> FilterTypes
        {
            get => _filterTypes;
            set => SetValue(ref _filterTypes, value);
        }

        public bool GameGridShowCompletionStatus
        {
            get => _gameGridShowCompletionStatus;
            set => SetValue(ref _gameGridShowCompletionStatus, value);
        }

        public bool GameGridShowPlatform
        {
            get => _gameGridShowPlatform;
            set => SetValue(ref _gameGridShowPlatform, value);
        }

        public bool GameGridShowReleaseYear
        {
            get => _gameGridShowReleaseYear;
            set => SetValue(ref _gameGridShowReleaseYear, value);
        }

        public int GameSearchWindowHeight
        {
            get => _gameSearchWindowHeight;
            set => SetValue(ref _gameSearchWindowHeight, value);
        }

        public int GameSearchWindowWidth
        {
            get => _gameSearchWindowWidth;
            set => SetValue(ref _gameSearchWindowWidth, value);
        }

        public bool IgnoreHiddenGamesInGameCount
        {
            get => _ignoreHiddenGamesInGameCount;
            set => SetValue(ref _ignoreHiddenGamesInGameCount, value);
        }

        public bool IgnoreHiddenGamesInRemoveUnused
        {
            get => _ignoreHiddenGamesInRemoveUnused;
            set => SetValue(ref _ignoreHiddenGamesInRemoveUnused, value);
        }

        public DateTime LastAutoLibUpdate
        {
            get => _lastAutoLibUpdate;
            set => SetValue(ref _lastAutoLibUpdate, value);
        }

        public bool MergeMetadataOnMetadataUpdate
        {
            get => _mergeMetadataOnMetadataUpdate;
            set => SetValue(ref _mergeMetadataOnMetadataUpdate, value);
        }

        public MergeRules MergeRules
        {
            get => _mergeRules;
            set => SetValue(ref _mergeRules, value);
        }

        public ObservableCollection<string> Prefixes
        {
            get => _prefixes;
            set => SetValue(ref _prefixes, value);
        }

        public string QuickAddCustomPath
        {
            get => _quickAddCustomPath;
            set => SetValue(ref _quickAddCustomPath, value);
        }

        public ObservableCollection<QuickAddObject> QuickAddObjects
        {
            get => _quickAddObjects;
            set => SetValue(ref _quickAddObjects, value);
        }

        public bool QuickAddShowDialog
        {
            get => _quickAddShowDialog;
            set => SetValue(ref _quickAddShowDialog, value);
        }

        public bool QuickAddSingleMenuEntry
        {
            get => _quickAddSingleMenuEntry;
            set => SetValue(ref _quickAddSingleMenuEntry, value);
        }

        public bool RemoveUnusedOnStartup
        {
            get => _removeUnusedOnStartup;
            set => SetValue(ref _removeUnusedOnStartup, value);
        }

        public bool RemoveUnwantedOnMetadataUpdate
        {
            get => _removeUnwantedOnMetadataUpdate;
            set => SetValue(ref _removeUnwantedOnMetadataUpdate, value);
        }

        public bool RenameDefaults
        {
            get => _renameDefaults;
            set => SetValue(ref _renameDefaults, value);
        }

        public bool RenameMergeRules
        {
            get => _renameMergeRules;
            set => SetValue(ref _renameMergeRules, value);
        }

        public bool SetDefaultTagsOnlyIfEmpty
        {
            get => _setDefaultTagsOnlyIfEmpty;
            set => SetValue(ref _setDefaultTagsOnlyIfEmpty, value);
        }

        public bool ShowTopPanelButton
        {
            get => _showTopPanelButton;
            set => SetValue(ref _showTopPanelButton, value);
        }

        public bool ShowTopPanelSettingsButton
        {
            get => _showTopPanelSettingsButton;
            set => SetValue(ref _showTopPanelSettingsButton, value);
        }

        public MetadataObjects UnusedItemsWhiteList
        {
            get => _unusedItemsWhiteList;
            set => SetValue(ref _unusedItemsWhiteList, value);
        }

        public MetadataObjects UnwantedItems
        {
            get => _unwantedItems;
            set => SetValue(ref _unwantedItems, value);
        }

        public bool WriteDebugLog
        {
            get => _writeDebugLog;
            set => SetValue(ref _writeDebugLog, value);
        }

        /// <summary>
        /// Resets the settings in the MetadataObjects classes, if these were created using
        /// deserializing from JSON.
        /// </summary>
        public void ResetReferences()
        {
            UnusedItemsWhiteList.Settings = this;
            UnwantedItems.Settings = this;
            DefaultCategories.Settings = this;
            DefaultTags.Settings = this;

            foreach (MergeRule rule in MergeRules)
            {
                rule.SourceObjects.Settings = this;
            }

            foreach (FieldType item in FieldTypeHelper.ItemListFieldValues().Keys
                         .Where(item => _filterTypes.All(x => x.Type != item)))
            {
                _filterTypes.Add(new FilterType()
                {
                    Type = item,
                    Selected = true,
                });
            }
        }
    }
}