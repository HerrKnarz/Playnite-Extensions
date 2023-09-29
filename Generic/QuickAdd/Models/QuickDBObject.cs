using Playnite.SDK.Models;

namespace QuickAdd.Models
{
    public class QuickDBObject : DatabaseObject
    {
        public bool Add { get; set; } = false;
        public bool Remove { get; set; } = false;
        public bool Toggle { get; set; } = false;
    }
}