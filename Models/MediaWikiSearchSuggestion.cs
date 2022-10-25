namespace LinkUtilities.Models.MediaWiki
{
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://opensearch.org/searchsuggest2", IsNullable = false)]
    public partial class SearchSuggestion
    {

        private SearchSuggestionWarnings warningsField;

        private SearchSuggestionQuery queryField;

        private SearchSuggestionItem[] sectionField;

        private decimal versionField;

        /// <remarks/>
        public SearchSuggestionWarnings Warnings
        {
            get
            {
                return warningsField;
            }
            set
            {
                warningsField = value;
            }
        }

        /// <remarks/>
        public SearchSuggestionQuery Query
        {
            get
            {
                return queryField;
            }
            set
            {
                queryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Item", IsNullable = false)]
        public SearchSuggestionItem[] Section
        {
            get
            {
                return sectionField;
            }
            set
            {
                sectionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public decimal Version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    public partial class SearchSuggestionWarnings
    {

        private SearchSuggestionWarningsMain mainField;

        /// <remarks/>
        public SearchSuggestionWarningsMain Main
        {
            get
            {
                return mainField;
            }
            set
            {
                mainField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    public partial class SearchSuggestionWarningsMain
    {

        private string spaceField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Space
        {
            get
            {
                return spaceField;
            }
            set
            {
                spaceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    public partial class SearchSuggestionQuery
    {

        private string spaceField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Space
        {
            get
            {
                return spaceField;
            }
            set
            {
                spaceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    public partial class SearchSuggestionItem
    {

        private SearchSuggestionItemText textField;

        private SearchSuggestionItemUrl urlField;

        /// <remarks/>
        public SearchSuggestionItemText Text
        {
            get
            {
                return textField;
            }
            set
            {
                textField = value;
            }
        }

        /// <remarks/>
        public SearchSuggestionItemUrl Url
        {
            get
            {
                return urlField;
            }
            set
            {
                urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    public partial class SearchSuggestionItemText
    {

        private string spaceField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Space
        {
            get
            {
                return spaceField;
            }
            set
            {
                spaceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://opensearch.org/searchsuggest2")]
    public partial class SearchSuggestionItemUrl
    {

        private string spaceField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string Space
        {
            get
            {
                return spaceField;
            }
            set
            {
                spaceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText()]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }
}