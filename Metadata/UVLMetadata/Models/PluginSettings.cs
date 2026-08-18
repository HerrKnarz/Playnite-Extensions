using System;
using System.Collections.Generic;

namespace UVLMetadata.Models;

public class PluginSettings : ObservableObject
{
    public BulkImportSettings BulkImportSettings
    {
        get;
        set => SetValue(ref field, value);
    } = new BulkImportSettings();

    public DescriptionToUse DescriptionToUse
    {
        get;
        set => SetValue(ref field, value);
    } = DescriptionToUse.Both;

    public DateTime LastTagRefresh
    {
        get;
        set => SetValue(ref field, value);
    } = DateTime.MinValue;

    public bool OnlyUseFirstDescription
    {
        get;
        set => SetValue(ref field, value);
    } = false;

    public RatingToUse RatingToUse
    {
        get;
        set => SetValue(ref field, value);
    } = RatingToUse.Average;

    public TagCategories TagCategories
    {
        get;
        set => SetValue(ref field, value);
    } = [];
}

public class BulkImportSettings : ObservableObject
{
    public int WindowHeight
    {
        get;
        set => SetValue(ref field, value);
    } = 600;

    public int WindowWidth
    {
        get;
        set => SetValue(ref field, value);
    } = 800;
}
