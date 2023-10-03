using Playnite.SDK.Models;

namespace QuickAdd.Models
{
    public class QuickDbObject : DatabaseObject
    {
        public bool Add { get; set; } = false;
        public bool Remove { get; set; } = false;
        public bool Toggle { get; set; } = false;
        public string CustomPath { get; set; } = string.Empty;
    }
}