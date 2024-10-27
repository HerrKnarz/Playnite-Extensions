using KNARZhelper;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace MetadataUtilities.Models
{
    public class Settings : ObservableObject
    {
        [DontSerialize]
        public readonly Regex CompanyFormRegex =
            new Regex(@",?\s+((co[,.\s]+)?ltd|(l\.)?inc|s\.?l|a[./]?s|limited|l\.?l\.?(c|p)|s\.?a(\.?r\.?l)?|s\.?r\.?o|gmbh|ab|corp|pte|ace|co|pty|pty\sltd|srl)\.?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private bool _addRemovedToUnwanted;
        private bool _alwaysSaveManualMergeRules;
        private int _conditionActionWindowHeight = 600;
        private int _conditionActionWindowWidth = 800;
        private ObservableCollection<ConditionalAction> _conditionalActions = new ObservableCollection<ConditionalAction>();
        private int _editorWindowHeight = 600;
        private int _editorWindowWidth = 1200;
        private ObservableCollection<FilterType> _filterTypes = new ObservableCollection<FilterType>();
        private bool _gameGridShowCompletionStatus = true;
        private bool _gameGridShowHidden;
        private bool _gameGridShowPlatform = true;
        private bool _gameGridShowReleaseYear = true;
        private int _gameSearchWindowHeight = 700;
        private int _gameSearchWindowWidth = 700;
        private bool _ignoreHiddenGamesInGameCount;
        private bool _mergeMetadataOnMetadataUpdate;
        private MergeRules _mergeRules = new MergeRules();
        private bool _prefixControlConfirmDeletion = true;
        private bool _prefixControlShowAddButton = true;
        private bool _prefixControlShowDeleteButton = true;
        private ObservableCollection<string> _prefixes = new ObservableCollection<string>();
        private ObservableCollection<PrefixItemList> _prefixItemTypes = new ObservableCollection<PrefixItemList>();
        private string _quickAddCustomPath = string.Empty;
        private ObservableCollection<QuickAddObject> _quickAddObjects = new ObservableCollection<QuickAddObject>();
        private bool _quickAddShowDialog = true;
        private bool _quickAddSingleMenuEntry;
        private bool _removeUnwantedOnMetadataUpdate = true;
        private bool _renameConditionalActions = true;
        private bool _renameMergeRules = true;
        private bool _renameQuickAdd = true;
        private bool _renameWhitelist = true;
        private bool _showTopPanelButton = true;
        private bool _showTopPanelSettingsButton;
        private ObservableCollection<TypeConfig> _typeConfigs = new ObservableCollection<TypeConfig>();
        private ObservableCollection<WhiteListItem> _unusedItemsWhiteList = new ObservableCollection<WhiteListItem>();
        private MetadataObjects _unwantedItems = new MetadataObjects();
        private bool _writeDebugLog;

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

        public int ConditionActionWindowHeight
        {
            get => _conditionActionWindowHeight;
            set => SetValue(ref _conditionActionWindowHeight, value);
        }

        public int ConditionActionWindowWidth
        {
            get => _conditionActionWindowWidth;
            set => SetValue(ref _conditionActionWindowWidth, value);
        }

        public ObservableCollection<ConditionalAction> ConditionalActions
        {
            get => _conditionalActions;
            set => SetValue(ref _conditionalActions, value);
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

        public bool GameGridShowHidden
        {
            get => _gameGridShowHidden;
            set => SetValue(ref _gameGridShowHidden, value);
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

        public bool PrefixControlConfirmDeletion
        {
            get => _prefixControlConfirmDeletion;
            set => SetValue(ref _prefixControlConfirmDeletion, value);
        }

        public bool PrefixControlDisplayAddButton
        {
            get => _prefixControlShowAddButton;
            set => SetValue(ref _prefixControlShowAddButton, value);
        }

        public bool PrefixControlDisplayDeleteButton
        {
            get => _prefixControlShowDeleteButton;
            set => SetValue(ref _prefixControlShowDeleteButton, value);
        }

        public ObservableCollection<string> Prefixes
        {
            get => _prefixes;
            set => SetValue(ref _prefixes, value);
        }

        public ObservableCollection<PrefixItemList> PrefixItemTypes
        {
            get => _prefixItemTypes;
            set => SetValue(ref _prefixItemTypes, value);
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

        public bool RemoveUnwantedOnMetadataUpdate
        {
            get => _removeUnwantedOnMetadataUpdate;
            set => SetValue(ref _removeUnwantedOnMetadataUpdate, value);
        }

        public bool RenameConditionalActions
        {
            get => _renameConditionalActions;
            set => SetValue(ref _renameConditionalActions, value);
        }

        public bool RenameMergeRules
        {
            get => _renameMergeRules;
            set => SetValue(ref _renameMergeRules, value);
        }

        public bool RenameQuickAdd
        {
            get => _renameQuickAdd;
            set => SetValue(ref _renameQuickAdd, value);
        }

        public bool RenameWhiteList
        {
            get => _renameWhitelist;
            set => SetValue(ref _renameWhitelist, value);
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

        public ObservableCollection<TypeConfig> TypeConfigs
        {
            get => _typeConfigs;
            set => SetValue(ref _typeConfigs, value);
        }

        public ObservableCollection<WhiteListItem> UnusedItemsWhiteList
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

        public void SetMissingTypes()
        {
            foreach (var item in FieldTypeHelper.ItemListFieldValues().Keys
                         .Where(item => _filterTypes.All(x => x.Type != item)))
            {
                _filterTypes.Add(new FilterType()
                {
                    Type = item,
                    Selected = true,
                });
            }

            foreach (var item in FieldTypeHelper.ItemListFieldValues().Keys
                         .Where(item => _typeConfigs.All(x => x.Type != item)))
            {
                _typeConfigs.Add(new TypeConfig()
                {
                    Type = item,
                    Selected = true,
                });
            }
        }
    }
}