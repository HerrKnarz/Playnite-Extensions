using System.Collections.Generic;

namespace LinkUtilities.Models
{
    public class SelectedLink : ObservableObject
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}
