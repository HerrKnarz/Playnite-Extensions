using KNARZhelper.Enum;
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
            }
            else if (replaceSources)
            {
                dest.SourceObjects.Clear();
                dest.SourceObjects.AddMissing(rule.SourceObjects);
            }
            else
            {
                foreach (MetadataObject obj in rule.SourceObjects)
                {
                    if (!dest.SourceObjects.Any(x => x.Name == obj.Name && x.Type == obj.Type))
                    {
                        dest.SourceObjects.Add(obj);
                    }
                }
            }

            AssimilateRules(rule);
        }

        /// <summary>
        /// Checks for other rules that have one of the current source items as the destination and
        /// integrates them into the current rule
        /// </summary>
        /// <param name="rule"></param>
        public void AssimilateRules(MergeRule rule)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            foreach (MergeRule child in this.Where(dest =>
                         dest != rule && rule.SourceObjects.Any(source =>
                             dest.Name == source.Name && dest.Type == source.Type)).ToList())
            {
                foreach (MetadataObject obj in child.SourceObjects)
                {
                    if (!rule.SourceObjects.Any(x => x.Name == obj.Name && x.Type == obj.Type))
                    {
                        rule.SourceObjects.Add(obj);
                    }
                }

                if (!rule.SourceObjects.Any(x => x.Name == child.Name && x.Type == child.Type))
                {
                    rule.SourceObjects.Add(child);
                }

                Remove(child);
            }
        }

        public bool FindAndRenameRule(FieldType fieldType, string name, string newName)
        {
            MergeRule rule = this.FirstOrDefault(x => x.Name == name && x.Type == fieldType);

            if (rule == null)
            {
                return false;
            }

            rule.Name = newName;

            return true;
        }
    }
}