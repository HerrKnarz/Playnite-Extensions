namespace MetadataUtilities.Models
{
    public enum ComparatorType
    {
        IsEqual,
        IsDifferent,
        IsEmpty
    }

    public class Condition : MetadataObject
    {
        private ComparatorType _comparator = ComparatorType.IsEqual;

        public Condition(Settings settings) : base(settings)
        {
        }

        public ComparatorType Comparator
        {
            get => _comparator;
            set => SetValue(ref _comparator, value);
        }
    }
}