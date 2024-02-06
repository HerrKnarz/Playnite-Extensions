﻿using Playnite.SDK.Models;

namespace MetadataUtilities.Models
{
    public class MyGame
    {
        public string Name { get; set; }

        public string SortingName { get; set; }

        public CompletionStatus CompletionStatus { get; set; }

        public int? ReleaseYear { get; set; }

        public string RealSortingName => string.IsNullOrWhiteSpace(SortingName) ? Name : SortingName;
    }
}