using System.Collections.Generic;

namespace com.espertech.esper.compat.collections.bound
{
    public static class BoundExtensions
    {
        public static bool IsLessThan<TK>(
            Bound<TK> bound,
            TK value,
            IComparer<TK> comparer)
        {
            if (bound == null)
                return true;
            return bound.IsInclusive 
                ? comparer.Compare(value, bound.Value) <= 0
                : comparer.Compare(value, bound.Value) < 0;
        }

        public static bool IsGreaterThan<TK>(
            Bound<TK> bound,
            TK value,
            IComparer<TK> comparer)
        {
            if (bound == null)
                return true;
            return bound.IsInclusive 
                ? comparer.Compare(value, bound.Value) >= 0
                : comparer.Compare(value, bound.Value) > 0;
        }

        public static Bound<TV> MergeLower<TV>(
            this Bound<TV> bound,
            Bound<TV> other,
            IComparer<TV> comparer)
        {
            if (other == null) {
                return bound;
            }
            
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
            if (other == null) {
                return bound;
            }

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