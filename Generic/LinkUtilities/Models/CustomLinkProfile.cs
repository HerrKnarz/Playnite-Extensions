using KNARZhelper;

namespace LinkUtilities.Models
{
    public class CustomLinkProfile
    {
        public bool Active { get; set; } = true;
        public bool AllowRedirects { get; set; } = true;
        public string BrowserSearchUrl { get; set; } = string.Empty;
        public bool EscapeDataString { get; set; } = false;
        public string GameUrl { get; set; }
        public string Name { get; set; }
        public bool NeedsToBeChecked { get; set; } = true;
        public bool RemoveDiacritics { get; set; } = false;
        public bool RemoveEditionSuffix { get; set; } = false;
        public bool RemoveHyphens { get; set; } = false;
        public bool RemoveSpecialChars { get; set; } = false;
        public bool RemoveWhitespaces { get; set; } = false;
        public bool ReturnsSameUrl { get; set; } = false;
        public bool ToLower { get; set; } = false;
        public bool ToTitleCase { get; set; } = false;
        public bool UnderscoresToWhitespaces { get; set; } = false;
        public bool UrlEncode { get; set; } = false;
        public bool WhitespacesToHyphens { get; set; } = false;
        public bool WhitespacesToUnderscores { get; set; } = false;

        public string FormatGameName(string gameName)
        {
            if (RemoveEditionSuffix)
            {
                gameName = gameName.RemoveEditionSuffix();
            }

            if (RemoveHyphens)
            {
                gameName = gameName.Replace("-", "");
            }

            if (UnderscoresToWhitespaces)
            {
                gameName = gameName.Replace("_", " ");
            }

            if (RemoveSpecialChars)
            {
                gameName = gameName.RemoveSpecialChars();
            }

            if (RemoveDiacritics)
            {
                gameName = gameName.RemoveDiacritics();
            }

            if (ToTitleCase)
            {
                gameName = gameName.ToTitleCase();
            }

            if (ToLower)
            {
                gameName = gameName.ToLower();
            }

            gameName = RemoveWhitespaces ? gameName.Replace(" ", "") : gameName.CollapseWhitespaces();

            if (WhitespacesToHyphens)
            {
                gameName = gameName.Replace(" ", "-");
            }

            if (WhitespacesToUnderscores)
            {
                gameName = gameName.Replace(" ", "_");
            }

            if (EscapeDataString)
            {
                gameName = gameName.EscapeDataString();
            }

            if (UrlEncode)
            {
                gameName = gameName.UrlEncode();
            }

            return gameName;
        }
    }
}