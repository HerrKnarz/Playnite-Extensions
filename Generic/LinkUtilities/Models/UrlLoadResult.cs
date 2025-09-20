﻿using HtmlAgilityPack;
using Playnite.SDK;
using System.Net;
using System.Windows.Media;

namespace LinkUtilities.Models
{
    public class UrlLoadResult
    {
        public string ErrorDetails { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public string ResponseUrl { get; set; } = string.Empty;
        public HtmlDocument Document { get; set; } = null;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Unused;

        public SolidColorBrush StatusColor => StatusCode >= HttpStatusCode.OK && StatusCode < HttpStatusCode.Ambiguous
            ? (SolidColorBrush)ResourceProvider.GetResource("PositiveRatingBrush")
            : StatusCode >= HttpStatusCode.Ambiguous && StatusCode < HttpStatusCode.BadRequest
                ? (SolidColorBrush)ResourceProvider.GetResource("MixedRatingBrush")
                : (SolidColorBrush)ResourceProvider.GetResource("NegativeRatingBrush");
    }
}