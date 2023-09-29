using Playnite.SDK.Models;

namespace QuickAdd.Models
{
    public class QuickTag : Tag
    {
        public bool Add { get; set; } = false;
        public bool Remove { get; set; } = false;
        public bool Toggle { get; set; } = false;
    }
}