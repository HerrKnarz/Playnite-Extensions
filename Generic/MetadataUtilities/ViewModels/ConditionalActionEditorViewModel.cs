using KNARZhelper;
using KNARZhelper.Enum;
using MetadataUtilities.Models;
using MetadataUtilities.Views;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Action = MetadataUtilities.Models.Action;
using Condition = MetadataUtilities.Models.Condition;

namespace MetadataUtilities.ViewModels
{
    public class ConditionalActionEditorViewModel : ObservableObject
    {
        private readonly Settings _settings;
        private ConditionalAction _conditionalAction;

        public ConditionalActionEditorViewModel(Settings settings, ConditionalAction conditionalAction)
        {
            _settings = settings;
            _conditionalAction = conditionalAction;
        }

        public RelayCommand<string> AddActionAgeRatingsCommand => new RelayCommand<string>(type =>
            AddActions(FieldType.AgeRating, type.ToActionType()));

        public RelayCommand<string> AddActionCategoriesCommand => new RelayCommand<string>(type =>
            AddActions(FieldType.Category, type.ToActionType()));

        public RelayCommand<string> AddActionFeaturesCommand => new RelayCommand<string>(type =>
            AddActions(FieldType.Feature, type.ToActionType()));

        public RelayCommand<string> AddActionGenresCommand => new RelayCommand<string>(type =>
            AddActions(FieldType.Genre, type.ToActionType()));

        public RelayCommand<string> AddActionSeriesCommand => new RelayCommand<string>(type =>
            AddActions(FieldType.Series, type.ToActionType()));

        public RelayCommand<string> AddActionTagsCommand => new RelayCommand<string>(type =>
            AddActions(FieldType.Tag, type.ToActionType()));

        public RelayCommand<string> AddConditionAgeRatingsCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.AgeRating, type.ToComparatorType()));

        public RelayCommand<string> AddConditionCategoriesCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Category, type.ToComparatorType()));

        public RelayCommand<string> AddConditionDevelopersCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Developer, type.ToComparatorType()));

        public RelayCommand<string> AddConditionFeaturesCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Feature, type.ToComparatorType()));

        public RelayCommand<string> AddConditionGenresCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Genre, type.ToComparatorType()));

        public RelayCommand<string> AddConditionPlatformsCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Platform, type.ToComparatorType()));

        public RelayCommand<string> AddConditionPublishersCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Publisher, type.ToComparatorType()));

        public RelayCommand<string> AddConditionSeriesCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Series, type.ToComparatorType()));

        public RelayCommand<string> AddConditionSourcesCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Source, type.ToComparatorType()));

        public RelayCommand<string> AddConditionTagsCommand => new RelayCommand<string>(type =>
            AddConditions(FieldType.Tag, type.ToComparatorType()));

        public ConditionalAction ConditionalAction
        {
            get => _conditionalAction;
            set => SetValue(ref _conditionalAction, value);
        }

        public RelayCommand<IList<object>> RemoveActionCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (Action item in items.ToList().Cast<Action>())
            {
                ConditionalAction.Actions.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<IList<object>> RemoveConditionCommand => new RelayCommand<IList<object>>(items =>
        {
            foreach (Condition item in items.ToList().Cast<Condition>())
            {
                ConditionalAction.Conditions.Remove(item);
            }
        }, items => items?.Count != 0);

        public RelayCommand<Window> SaveCommand => new RelayCommand<Window>(win =>
        {
            if (ConditionalAction.Name == null || ConditionalAction.Name?.Length == 0)
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoNameSet"), string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!ConditionalAction.Actions.Any())
            {
                API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoActionsSet"), string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ConditionalAction.Enabled && !ConditionalAction.Conditions.Any())
            {
                if (API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCMetadataUtilitiesDialogNoConditionsSet"), string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }

            win.DialogResult = true;
            win.Close();
        });

        public static Window GetWindow(Settings settings, ConditionalAction conditionalAction)
        {
            try
            {
                ConditionalActionEditorViewModel viewModel = new ConditionalActionEditorViewModel(settings, conditionalAction);

                ConditionalActionEditorView conditionalActionEditorView = new ConditionalActionEditorView();

                Window window = WindowHelper.CreateSizedWindow(ResourceProvider.GetString("LOCMetadataUtilitiesDialogConditionalActionEditor"), 800, 500);
                window.Content = conditionalActionEditorView;
                window.DataContext = viewModel;

                return window;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during initializing edit conditional action dialog", true);

                return null;
            }
        }

        public void AddActions(FieldType fieldType, ActionType actionType)
        {
            if (actionType == ActionType.ClearField)
            {
                if (!ConditionalAction.Actions.Any(x => x.ActionType == actionType && x.Type == fieldType))
                {
                    ConditionalAction.Actions.Add(new Action(_settings)
                    {
                        Name = string.Empty,
                        Type = fieldType,
                        ActionType = actionType
                    });
                }

                return;
            }

            List<MetadataObject> items = MetadataFunctions.GetItemsFromAddDialog(fieldType, _settings);

            if (items.Count == 0)
            {
                return;
            }

            foreach (MetadataObject item in items.Where(item => ConditionalAction.Actions.All(x => x.TypeAndName != item.TypeAndName || x.ActionType != actionType)))
            {
                ConditionalAction.Actions.Add(new Action(_settings)
                {
                    Name = item.Name,
                    Type = item.Type,
                    ActionType = actionType
                });
            }

            ConditionalAction.Conditions = ConditionalAction.Conditions.OrderBy(x => x.ToString).ToObservable();
        }

        public void AddConditions(FieldType fieldType, ComparatorType comparatorType)
        {
            if (comparatorType == ComparatorType.IsEmpty)
            {
                if (!ConditionalAction.Conditions.Any(x => x.Comparator == comparatorType && x.Type == fieldType))
                {
                    ConditionalAction.Conditions.Add(new Condition(_settings)
                    {
                        Name = string.Empty,
                        Type = fieldType,
                        Comparator = comparatorType
                    });
                }

                return;
            }

            List<MetadataObject> items = MetadataFunctions.GetItemsFromAddDialog(fieldType, _settings);

            if (items.Count == 0)
            {
                return;
            }

            foreach (MetadataObject item in items.Where(item => ConditionalAction.Conditions.All(x => x.TypeAndName != item.TypeAndName || x.Comparator != comparatorType)))
            {
                ConditionalAction.Conditions.Add(new Condition(_settings)
                {
                    Name = item.Name,
                    Type = item.Type,
                    Comparator = comparatorType
                });
            }

            ConditionalAction.Conditions = ConditionalAction.Conditions.OrderBy(x => x.ToString).ToObservable();
        }
    }
}