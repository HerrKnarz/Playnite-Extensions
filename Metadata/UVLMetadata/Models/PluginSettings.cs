using System;
using System.Collections.Generic;
using UVLMetadata.Enums;

namespace UVLMetadata.Models;

public class BulkImportSettings : ObservableObject
{
    public AddLink AddLink
    {
        get;
        set => SetValue(ref field, value);
    } = AddLink.PerfectAndVeryGood;

    public int WindowHeight
    {
        get;
        set => SetValue(ref field, value);
    } = 700;

    public int WindowWidth
    {
        get;
        set => SetValue(ref field, value);
    } = 1000;
}

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

    public bool DisplayTopPanelButton
    {
        get;
        set => SetValue(ref field, value);
    } = true;

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
