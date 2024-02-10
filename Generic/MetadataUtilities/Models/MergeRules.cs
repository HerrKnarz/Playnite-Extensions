using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MergeRules : ObservableCollection<MergeRule>
    {
        public void AddRule(MergeRule rule, bool replaceSources = false)
        {
            MergeRule dest = this.FirstOrDefault(x => x.Name == rule.Name && x.Type == rule.Type);

            if (dest == null)
            {
                Add(rule);
                return;
            }

            if (replaceSources)
            {
                dest.SourceObjects.Clear();
                dest.SourceObjects.AddMissing(rule.SourceObjects);

                return;
            }

            foreach (MetadataObject obj in rule.SourceObjects)
            {
                if (!dest.SourceObjects.Any(x => x.Name == obj.Name && x.Type == obj.Type))
                {
                    dest.SourceObjects.Add(obj);
                }
            }
        }
    }
}