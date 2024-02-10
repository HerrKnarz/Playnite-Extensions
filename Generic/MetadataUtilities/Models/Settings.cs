using MetadataUtilities.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MetadataUtilities
{
    public class Settings : ObservableObject
    {
        private bool _alwaysSaveManualMergeRules;
        private ObservableCollection<MetadataObject> _defaultCategories = new ObservableCollection<MetadataObject>();
        private ObservableCollection<MetadataObject> _defaultTags = new ObservableCollection<MetadataObject>();
        private int _editorWindowHeight = 600;
        private int _editorWindowWidth = 1200;
        private bool _filterCategories = true;
        private bool _filterFeatures = true;
        private bool _filterGenres = true;
        private bool _filterSeries = true;
        private bool _filterTags = true;
        private bool _ignoreHiddenGamesInGameCount;
        private bool _ignoreHiddenGamesInRemoveUnused;
        private DateTime _lastAutoLibUpdate = DateTime.Now;
        private bool _mergeMetadataOnMetadataUpdate;
        private MergeRules _mergeRules = new MergeRules();
        private ObservableCollection<string> _prefixes = new ObservableCollection<string>();
        private bool _removeUnusedOnStartup;
        private bool _setDefaultTagsOnlyIfEmpty = true;
        private bool _showTopPanelButton = true;
        private bool _writeDebugLog;

        public bool AlwaysSaveManualMergeRules
        {
            get => _alwaysSaveManualMergeRules;
            set => SetValue(ref _alwaysSaveManualMergeRules, value);
        }

        public ObservableCollection<MetadataObject> DefaultCategories
        {
            get => _defaultCategories;
            set => SetValue(ref _defaultCategories, value);
        }

        public ObservableCollection<MetadataObject> DefaultTags
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

        public bool RemoveUnusedOnStartup
        {
            get => _removeUnusedOnStartup;
            set => SetValue(ref _removeUnusedOnStartup, value);
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

        public bool WriteDebugLog
        {
            get => _writeDebugLog;
            set => SetValue(ref _writeDebugLog, value);
        }
    }
}