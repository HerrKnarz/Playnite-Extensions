namespace LinkUtilities.Models
{
    public class CustomLinkProfile
    {
        public string Name { get; set; }
        public string GameUrl { get; set; }
        public string BrowserSearchUrl { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public bool RemoveSpecialChars { get; set; } = false;
        public bool RemoveHyphens { get; set; } = false;
        public bool RemoveWhitespaces { get; set; } = false;
        public bool RemoveDiacritics { get; set; } = false;
        public bool RemoveEditionSuffix { get; set; } = false;
        public bool WhitespacesToHyphens { get; set; } = false;
        public bool UnderscoresToWhitespaces { get; set; } = false;
        public bool WhitespacesToUnderscores { get; set; } = false;
        public bool EscapeDataString { get; set; } = false;
        public bool UrlEncode { get; set; } = false;
        public bool ToLower { get; set; } = false;
        public bool ToTitleCase { get; set; } = false;
        public bool AllowRedirects { get; set; } = true;
        public bool ReturnsSameUrl { get; set; } = false;
        public bool NeedsToBeChecked { get; set; } = true;
    }
}