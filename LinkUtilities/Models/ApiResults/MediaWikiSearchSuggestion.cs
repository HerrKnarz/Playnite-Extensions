using System.ComponentModel;
using System.Xml.Serialization;

namespace LinkUtilities.Models.ApiResults;

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
[XmlRoot(Namespace = "http://opensearch.org/searchsuggest2", IsNullable = false)]
public class SearchSuggestion
{
    private SearchSuggestionQuery? _queryField;
    private SearchSuggestionItem[]? _sectionField;
    private decimal _versionField;
    private SearchSuggestionWarnings? _warningsField;

    /// <remarks/>
    public SearchSuggestionQuery? Query
    {
        get => _queryField;
        set => _queryField = value;
    }

    /// <remarks/>
    [XmlArrayItem("Item", IsNullable = false)]
    public SearchSuggestionItem[]? Section
    {
        get => _sectionField;
        set => _sectionField = value;
    }

    /// <remarks/>
    [XmlAttribute()]
    public decimal Version
    {
        get => _versionField;
        set => _versionField = value;
    }

    /// <remarks/>
    public SearchSuggestionWarnings? Warnings
    {
        get => _warningsField;
        set => _warningsField = value;
    }
}

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
public class SearchSuggestionItem
{
    private SearchSuggestionItemText? _textField;

    private SearchSuggestionItemUrl? _urlField;

    /// <remarks/>
    public SearchSuggestionItemText? Text
    {
        get => _textField;
        set => _textField = value;
    }

    /// <remarks/>
    public SearchSuggestionItemUrl? Url
    {
        get => _urlField;
        set => _urlField = value;
    }
}

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
public class SearchSuggestionItemText
{
    private string? _spaceField;

    private string? _valueField;

    /// <remarks/>
    [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string? Space
    {
        get => _spaceField;
        set => _spaceField = value;
    }

    /// <remarks/>
    [XmlText()]
    public string? Value
    {
        get => _valueField;
        set => _valueField = value;
    }
}

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
public class SearchSuggestionQuery
{
    private string? _spaceField;

    private string? _valueField;

    /// <remarks/>
    [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string? Space
    {
        get => _spaceField;
        set => _spaceField = value;
    }

    /// <remarks/>
    [XmlText()]
    public string? Value
    {
        get => _valueField;
        set => _valueField = value;
    }
}

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
public class SearchSuggestionWarnings
{
    private SearchSuggestionWarningsMain? _mainField;

    /// <remarks/>
    public SearchSuggestionWarningsMain? Main
    {
        get => _mainField;
        set => _mainField = value;
    }
}

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
public class SearchSuggestionWarningsMain
{
    private string? _spaceField;

    private string? _valueField;

    /// <remarks/>
    [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string? Space
    {
        get => _spaceField;
        set => _spaceField = value;
    }

    /// <remarks/>
    [XmlText()]
    public string? Value
    {
        get => _valueField;
        set => _valueField = value;
    }
}

/// <remarks/>
[Serializable()]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
public class SearchSuggestionItemUrl
{
    private string? _spaceField;

    private string? _valueField;

    /// <remarks/>
    [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
    public string? Space
    {
        get => _spaceField;
        set => _spaceField = value;
    }

    /// <remarks/>
    [XmlText()]
    public string? Value
    {
        get => _valueField;
        set => _valueField = value;
    }
}