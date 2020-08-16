using System.Collections.Generic;

namespace com.espertech.esper.compat.collections.bound
{
    public class BoundRange<TK>
    {
        private readonly Bound<TK> _lower;
        private readonly Bound<TK> _upper;
        private readonly IComparer<TK> _comparer;

        public Bound<TK> Lower => _lower;

        public Bound<TK> Upper => _upper;

        public IComparer<TK> Comparer => _comparer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <param name="comparer"></param>
        public BoundRange(
            Bound<TK> lower,
            Bound<TK> upper,
            IComparer<TK> comparer)
        {
            _lower = lower;
            _upper = upper;
            _comparer = comparer;
        }

        /// <summary>
        /// Returns true if the range is unbounded.
        /// </summary>
        public bool IsUnbounded => (
            (_lower == null) &&
            (_upper == null));

        /// <summary>
        /// Merges this range with another range.  Ranges may only become more restrictive, never less restrictive.
        /// </summary>
        /// <param name="otherRange"></param>
        /// <returns></returns>

        public BoundRange<TK> Merge(BoundRange<TK> otherRange)
        {
            return new BoundRange<TK>(
                _lower.MergeLower(otherRange.Lower, _comparer),
                _upper.MergeUpper(otherRange.Upper, _comparer),
                _comparer);
        }
        
        /// <summary>
        /// Returns true if the value is within the lower and upper bound.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsWithin(TK value)
        {
            return IsGreaterThan(_lower, value) 
                   && IsLessThan(_upper, value);
        }
        
        private bool IsLessThan(
            Bound<TK> bound,
            TK value)
        {
            if (bound == null)
                return true;
            return bound.IsInclusive 
                ? _comparer.Compare(value, bound.Value) <= 0
                : _comparer.Compare(value, bound.Value) < 0;
        }

        private bool IsGreaterThan(
            Bound<TK> bound,
            TK value)
        {
            if (bound == null)
                return true;
            return bound.IsInclusive 
                ? _comparer.Compare(value, bound.Value) >= 0
                : _comparer.Compare(value, bound.Value) > 0;
        }

    }
}