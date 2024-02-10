using MetadataUtilities.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace MetadataUtilities
{
    public class Settings : ObservableObject
    {
        private bool _alwaysSaveManualMergeRules;
        private ObservableCollection<MetadataListObject> _defaultCategories = new ObservableCollection<MetadataListObject>();
        private ObservableCollection<MetadataListObject> _defaultTags = new ObservableCollection<MetadataListObject>();
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
        private CollectionViewSource _mergeRuleViewSource;
        private HashSet<string> _prefixes = new HashSet<string>();
        private bool _removeUnusedOnStartup;
        private MergeRule _selectedMergeRule;
        private bool _setDefaultTagsOnlyIfEmpty = true;
        private bool _showTopPanelButton = true;
        private CollectionViewSource _sourceObjectsViewSource;
        private bool _writeDebugLog;

        public Settings()
        {
            MergeRuleViewSource = new CollectionViewSource
            {
                Source = MergeRules
            };

            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            MergeRuleViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            MergeRuleViewSource.IsLiveSortingRequested = true;

            SourceObjectsViewSource = new CollectionViewSource();

            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            SourceObjectsViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            SourceObjectsViewSource.IsLiveSortingRequested = true;
        }

        public bool AlwaysSaveManualMergeRules
        {
            get => _alwaysSaveManualMergeRules;
            set => SetValue(ref _alwaysSaveManualMergeRules, value);
        }

        public ObservableCollection<MetadataListObject> DefaultCategories
        {
            get => _defaultCategories;
            set => SetValue(ref _defaultCategories, value);
        }

        public ObservableCollection<MetadataListObject> DefaultTags
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

        [DontSerialize]
        public CollectionViewSource MergeRuleViewSource
        {
            get => _mergeRuleViewSource;
            set
            {
                _mergeRuleViewSource = value;
                OnPropertyChanged();
            }
        }

        public HashSet<string> Prefixes
        {
            get => _prefixes;
            set => SetValue(ref _prefixes, value);
        }

        public bool RemoveUnusedOnStartup
        {
            get => _removeUnusedOnStartup;
            set => SetValue(ref _removeUnusedOnStartup, value);
        }

        [DontSerialize]
        public MergeRule SelectedMergeRule
        {
            get => _selectedMergeRule;
            set
            {
                SetValue(ref _selectedMergeRule, value);

                SourceObjectsViewSource.Source = _selectedMergeRule == null
                    ? new ObservableCollection<MetadataListObject>()
                    : _selectedMergeRule.SourceObjects;

                SourceObjectsViewSource.View.Refresh();
            }
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

        [DontSerialize]
        public CollectionViewSource SourceObjectsViewSource
        {
            get => _sourceObjectsViewSource;
            set
            {
                _sourceObjectsViewSource = value;
                OnPropertyChanged();
            }
        }

        public bool WriteDebugLog
        {
            get => _writeDebugLog;
            set => SetValue(ref _writeDebugLog, value);
        }
    }
}