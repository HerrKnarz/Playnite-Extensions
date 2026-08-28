using KNARZhelper;
using KNARZhelper.Controls;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UVLMetadata.Enums;
using UVLMetadata.Models;
using UVLMetadata.Views;

namespace UVLMetadata.ViewModels;

public class BulkImportViewModel : ObservableObject, IRequestFocus
{
    private readonly GameMatcher _gameMatcher = new([.. API.Instance.Database.Games]);
    private readonly UVLMetadata _plugin;
    private RelayCommand<Window> _closeCommand;
    private RelayCommand _importTagsCommand;
    private RelayCommand _refreshTagsCommand;
    private RelayCommand _searchGamesCommand;
    private RelayCommand _selectFieldValueCommand;
    private RelayCommand<IList<object>> _toggleSelectedCommand;

    public BulkImportViewModel(UVLMetadata plugin)
    {
        _plugin = plugin;
        AddLink = _plugin.Settings.Settings.BulkImportSettings.AddLink;

        PrepareTagsViewSource();
    }

    public event EventHandler<FocusRequestedEventArgs> FocusRequested;

    public AddLink AddLink
    {
        get;
        set => SetValue(ref field, value);
    } = AddLink.PerfectAndVeryGood;

    public AddLinkModes AddLinkModes { get; } = [];

    public ICommand CloseCommand => _closeCommand ??= new RelayCommand<Window>(Close);

    public MetadataField FieldType
    {
        get;
        set => SetValue(ref field, value);
    }

    public string FieldValue
    {
        get;
        set => SetValue(ref field, value);
    }

    public string FoundGamesSectionCaption =>
        MatchedTag is not null
            ? $"{string.Format(ResourceProvider.GetString("LOCUVLMetadataBulkImportFoundGamesFor"), MatchedTag?.ShortName)}"
            : ResourceProvider.GetString("LOCUVLMetadataBulkImportFoundGames");

    public bool GroupsExpanded
    {
        get;
        set => SetValue(ref field, value);
    } = false;

    public Dictionary<MetadataField, string> ImportAsModes { get; } = new()
    {
        { MetadataField.AgeRating, ResourceProvider.GetString("LOCAgeRatingLabel") },
        { MetadataField.Features, ResourceProvider.GetString("LOCFeatureLabel") },
        { MetadataField.Genres, ResourceProvider.GetString("LOCGenreLabel") },
        { MetadataField.Series, ResourceProvider.GetString("LOCSeriesLabel") },
        { MetadataField.Tags, ResourceProvider.GetString("LOCTagLabel") },
    };

    public ICommand ImportTagsCommand => _importTagsCommand ??= new RelayCommand(ImportTags);

    public ObservableCollection<MatchedGame> MatchedGames
    {
        get;
        set => SetValue(ref field, value);
    } = [];

    public UVLTag MatchedTag
    {
        get;
        set
        {
            SetValue(ref field, value);

            OnPropertyChanged(nameof(FoundGamesSectionCaption));

            if (MatchedTag is null)
            {
                FieldType = MetadataField.Tags;
                FieldValue = string.Empty;
                return;
            }

            _plugin.Settings.Settings.TagCategories.TryGetValue(MatchedTag?.Category ?? 0, out var tagCategory);

            var fieldName = string.Empty;

            FieldType = tagCategory?.ImportAsByTag(MatchedTag, out fieldName, out _) ?? MetadataField.Tags;
            FieldValue = fieldName ?? MatchedTag?.ShortName ?? string.Empty;
        }
    }

    public ICommand RefreshTagsCommand => _refreshTagsCommand ??= new RelayCommand(RefreshTags);
    public ICommand SearchGamesCommand => _searchGamesCommand ??= new RelayCommand(SearchGames);

    public string SearchTerm
    {
        get;
        set
        {
            SetValue(ref field, value);
            TagsViewSource.View.Filter = Filter;
        }
    } = string.Empty;

    public MatchedGame SelectedGame
    {
        get;
        set => SetValue(ref field, value);
    }

    public UVLTag SelectedTag
    {
        get;
        set => SetValue(ref field, value);
    }

    public ICommand SelectFieldValueCommand => _selectFieldValueCommand ??= new RelayCommand(SelectFieldValue);

    public CollectionViewSource TagsViewSource
    {
        get;
        set => SetValue(ref field, value);
    }

    public ICommand ToggleSelectedCommand => _toggleSelectedCommand ??= new RelayCommand<IList<object>>(games => ToggleSelected(games));

    public static void ShowWindow(UVLMetadata plugin)
    {
        if (plugin.Settings.IsAuthenticated != AuthenticationStatus.Authenticated)
        {
            if (API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCUVLMetadataDialogLoginRequired"), "UVL", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                plugin.OpenSettingsView();
            }
            else
            {
                return;
            }
        }

        if (plugin.Settings.IsAuthenticated != AuthenticationStatus.Authenticated)
        {
            return;
        }

        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

        try
        {
            var viewModel = new BulkImportViewModel(plugin);

            var view = new BulkImportView(viewModel);

            var window = WindowHelper.CreateSizedWindow(
                $"UVL {ResourceProvider.GetString("LOCUVLMetadataMenuBulkImport")}", plugin.Settings.Settings.BulkImportSettings.WindowWidth, plugin.Settings.Settings.BulkImportSettings.WindowHeight);

            window.Content = view;
            window.DataContext = viewModel;

            window.ShowDialog();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error during initializing bulk import view", true);
        }
        finally
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }
    }

    private void Close(Window win)
    {
        var settings = _plugin.Settings.Settings.BulkImportSettings;

        settings.WindowHeight = Convert.ToInt32(win.Height);
        settings.WindowWidth = Convert.ToInt32(win.Width);
        settings.AddLink = AddLink;
        _plugin.SavePluginSettings(_plugin.Settings.Settings);

        win.DialogResult = true;
        win.Close();
    }

    private bool Filter(object item)
    {
        if (!SearchTerm.IsNullOrEmpty() && !GroupsExpanded)
        {
            GroupsExpanded = true;
        }

        return SearchTerm.IsNullOrEmpty() || (item is UVLTag tag && tag.ShortName.Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase));
    }

    private BaseListType GetTypeManager()
    {
        return FieldType switch
        {
            MetadataField.AgeRating => new TypeAgeRating(),
            MetadataField.Features => new TypeFeature(),
            MetadataField.Genres => new TypeGenre(),
            MetadataField.Series => new TypeSeries(),
            MetadataField.Tags => new TypeTag(),
            _ => null,
        };
    }

    private void ImportTags()
    {
        var typeManager = GetTypeManager();

        if (typeManager is null)
        {
            return;
        }

        if (MatchedGames.Count < 1 || !MatchedGames.Any(g => g.Selected))
        {
            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCUVLMetadataBulkImportGamesAffected"), typeManager.LabelSingular, 0), "UVL");

            return;
        }

        var gamesAffected = new List<Game>();

        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

        try
        {
            var fieldId = typeManager.AddDbObject(FieldValue);

            foreach (var game in MatchedGames.Where(x => x.Selected))
            {
                var needsUpdate = false;

                try
                {
                    if (game is null)
                    {
                        continue;
                    }

                    needsUpdate = typeManager.AddValueToGame(game.PlayniteGame.Game, fieldId);

                    var importLink = AddLink switch
                    {
                        AddLink.Never => false,
                        AddLink.PerfectAndVeryGood => game.MatchingScore is MatchingScore.Perfect or MatchingScore.VeryGood,
                        AddLink.MatchingPlatform => game.PlayniteGame.Game.Platforms?.Any(x => x.SpecificationId?.Equals(game.UVLGame.PlatformSpecId) ?? false) ?? false,
                        AddLink.AllGames => true,
                        _ => false
                    };

                    if (importLink && !(game.PlayniteGame.Game.Links?.Any(x => x.Url?.Contains("uvlist.net") ?? false) ?? false))
                    {
                        game.PlayniteGame.Game.Links ??= [];

                        game.PlayniteGame.Game.Links.Add(new Link()
                        {
                            Name = "UVL",
                            Url = game.UVLGame.Url
                        });

                        _gameMatcher.AddLinkMatch(game.PlayniteGame.Game, game.UVLGame.Url);

                        needsUpdate = true;
                    }

                    if (needsUpdate)
                    {
                        game.PlayniteGame.Game.Modified = DateTime.Now;

                        gamesAffected.Add(game.PlayniteGame.Game);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, $"Error during importing {typeManager.LabelSingular} {FieldValue} for game {game.PlayniteGame.Game.Name}");
                }
            }

            API.Instance.MainView.UIDispatcher.Invoke(delegate
            {
                API.Instance.Database.Games.Update(gamesAffected);
            });
        }
        finally
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCUVLMetadataBulkImportGamesAffected"), typeManager.LabelSingular, gamesAffected.Count), "UVL");
    }

    private void OnFocusRequested(string propertyName) => FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));

    private void PrepareTagsViewSource()
    {
        TagsViewSource = new CollectionViewSource
        {
            Source = _plugin.Tags
        };

        using (TagsViewSource.DeferRefresh())
        {
            TagsViewSource.SortDescriptions.Add(new SortDescription("CategoryCaption", ListSortDirection.Ascending));
            TagsViewSource.SortDescriptions.Add(new SortDescription("TypeCaption", ListSortDirection.Ascending));
            TagsViewSource.SortDescriptions.Add(new SortDescription("ShortName", ListSortDirection.Ascending));
        }

        TagsViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("CategoryCaption"));
        TagsViewSource.View.GroupDescriptions.Add(new PropertyGroupDescription("TypeCaption"));

        TagsViewSource.View.Filter = Filter;
    }

    private void RefreshTags()
    {
        using (TagsViewSource.DeferRefresh())
        {
            _plugin.UVLConnect.RefreshTags();
            _plugin.Settings.Settings.LastTagRefresh = _plugin.Tags.LastRefresh;
            _plugin.SavePluginSettings(_plugin.Settings.Settings);
        }

        TagsViewSource.View.Filter = Filter;
    }

    private void SearchGames()
    {
        if (SelectedTag is null)
        {
            return;
        }

        var url = SelectedTag.Slug.Replace("/groups/info/", $"{Resources.WebsiteUrl}/gamesearch/?ftag=");

        var foundGames = _plugin.UVLConnect.GetDetailSearchResults(url, SelectedTag.GameCount);

        if (foundGames is null || !foundGames.Any())
        {
            API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCUVLMetadataDialogNoGamesFound"), "UVL");
            return;
        }

        MatchedGames.Clear();

        MatchedTag = SelectedTag;

        _gameMatcher.MatchGames(foundGames);

        MatchedGames.AddMissing(_gameMatcher.MatchedGames.Values.OrderBy(v => v.PlayniteGame.RealSortingName).ThenBy(v => v.PlayniteGame.Game.ReleaseDate));

        if (MatchedGames.Count == 0)
        {
            return;
        }

        SelectedGame = MatchedGames.FirstOrDefault();

        OnFocusRequested(nameof(SelectedGame));
    }

    private void SelectFieldValue()
    {
        var typeManager = GetTypeManager();

        if (typeManager is null)
        {
            return;
        }

        var label = typeManager.LabelPlural;
        var items = new ObservableCollection<BaseMetadataObject>();

        typeManager.LoadAllMetadata([]).ForEach(item => items.Add(
            new BaseMetadataObject(typeManager, typeManager.Type, item.Name)
            {
                Id = item.Id
            }));

        items.Sort(i => i.Name);

        SelectMetadataViewModel.GetWindow(items, label, false)?.ShowDialog();

        if (items.Count(i => i.Selected) == 0)
        {
            return;
        }

        FieldValue = items.First(i => i.Selected).Name;
    }

    private void ToggleSelected(IList<object> games)
    {
        if (games is null || games.Count < 1)
        {
            return;
        }

        var gamesToSelect = games.Select(x => x as MatchedGame).Where(x => x is not null).ToList();

        if (gamesToSelect.Count < 1)
        {
            return;
        }

        foreach (var game in gamesToSelect)
        {
            game.Selected = !game.Selected;
        }
    }
}
