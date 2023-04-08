using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Models
{
    internal class FindLinkResult
    {
        public bool Success { get; set; } = false;

        public List<Link> Links { get; set; } = new List<Link>();
    }
}