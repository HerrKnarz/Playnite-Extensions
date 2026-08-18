using KNARZhelper;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UVLMetadata.Models;
using UVLMetadata.Views;

namespace UVLMetadata.ViewModels;

public class BulkImportViewModel : ObservableObject
{
    private readonly UVLMetadata _plugin;
    private RelayCommand<Window> _closeCommand;

    public BulkImportViewModel(UVLMetadata plugin)
    {
        _plugin = plugin;

        PrepareTagsViewSource();
    }

    public ICommand CloseCommand => _closeCommand ??= new RelayCommand<Window>(Close);

    public bool GroupsExpanded
    {
        get;
        set => SetValue(ref field, value);
    } = false;

    public string SearchTerm
    {
        get;
        set
        {
            SetValue(ref field, value);
            TagsViewSource.View.Filter = Filter;
        }
    } = string.Empty;

    public CollectionViewSource TagsViewSource
    {
        get;
        set => SetValue(ref field, value);
    }

    public static void ShowWindow(UVLMetadata plugin)
    {
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

        try
        {
            var viewModel = new BulkImportViewModel(plugin);

            var view = new BulkImportView();

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
}
