using System.Collections.Generic;

namespace com.espertech.esper.compat.collections.bound
{
    public static class BoundExtensions
    {
        public static Bound<TV> MergeLower<TV>(
            this Bound<TV> bound,
            Bound<TV> other,
            IComparer<TV> comparer)
        {
            return MergeLower(bound, other.Value, other.IsInclusive, comparer);
        }

        public static Bound<TV> MergeLower<TV>(
            this Bound<TV> bound,
            TV value,
            bool isInclusive,
            IComparer<TV> comparer)
        {
            if (bound == null)
                return new Bound<TV>(value, isInclusive);
            var comp = comparer.Compare(value, bound.Value);
            if (comp == 0) {
                return new Bound<TV>(value, isInclusive & bound.IsInclusive);
            } else if (comp < 0) {
                return bound;
            }

            return new Bound<TV>(value, isInclusive);
        }

        public static Bound<TV> MergeUpper<TV>(
            this Bound<TV> bound,
            Bound<TV> other,
            IComparer<TV> comparer)
        {
            return MergeUpper(bound, other.Value, other.IsInclusive, comparer);
        }
        
        public static Bound<TV> MergeUpper<TV>(
            this Bound<TV> bound,
            TV value,
            bool isInclusive,
            IComparer<TV> comparer)
        {
            if (bound == null)
                return new Bound<TV>(value, isInclusive);
            var comp = comparer.Compare(value, bound.Value);
            if (comp == 0) {
                return new Bound<TV>(value, isInclusive & bound.IsInclusive);
            } else if (comp > 0) {
                return bound;
            }

            return new Bound<TV>(value, isInclusive);
        }
    }
}