using System;
using System.Collections.Generic;

namespace UVLMetadata.Models;

public class PluginSettings : ObservableObject
{
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
