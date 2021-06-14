using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections.bound;

namespace com.espertech.esper.compat.collections.btree
{
    public static class BTreeDictionaryExtensions
    {
        /// <summary>
        /// Returns the count of items in the underlying btree within a bounded range.
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="range"></param>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <returns></returns>
        internal static int Count<TK, TV>(
            BTree<TK, KeyValuePair<TK, TV>> underlying,
            BoundRange<TK> range)
        {
            if (range.IsUnbounded) {
                return underlying.Count;
            }
                
            // This is a much more complex question when there is a lower bound
            // or upper bound.  In this case, we *currently* need to count the
            // items between the lower and upper found.
            return Enumerate(underlying, range).Count();
        }

        /// <summary>
        /// Returns the first key-value pair in the dictionary.  If the dictionary
        /// is empty, this method throws an InvalidOperationException.
        /// </summary>
        internal static KeyValuePair<TK, TV> FirstKeyValuePair<TK, TV>(
            BTree<TK, KeyValuePair<TK, TV>> underlying,
            BoundRange<TK> range)
        {
            var lower = range.Lower;
            var cursor = lower != null
                ? lower.IsInclusive
                    ? underlying.LessThanOrEqual(lower.Value, underlying.RootCursor)
                    : underlying.LessThan(lower.Value, underlying.RootCursor)
                : underlying.Begin();
            if (cursor.IsEnd) {
                throw new InvalidOperationException();
            }

            return cursor.Value;
        }

        /// <summary>
        /// Returns the last key-value pair in the dictionary.  If the dictionary
        /// is empty, this method throws an InvalidOperationException.
        /// </summary>
        internal static KeyValuePair<TK, TV> LastKeyValuePair<TK, TV>(
            BTree<TK, KeyValuePair<TK, TV>> underlying,
            BoundRange<TK> range)
        {
            var upper = range.Upper;
            var cursor = upper != null
                ? upper.IsInclusive
                    ? underlying.GreaterThanOrEqual(upper.Value, underlying.RootCursor)
                    : underlying.GreaterThan(upper.Value, underlying.RootCursor)
                : underlying.End().MovePrevious();

            if (cursor.IsEnd) {
                throw new InvalidOperationException();
            }

            return cursor.Value;
        }


        /// <summary>
        ///     Returns an enumerator starting at a key-value with the greatest key less than
        ///     or equal to the given key.
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<KeyValuePair<TK, TV>> Enumerate<TK, TV>(
            BTree<TK, KeyValuePair<TK, TV>> underlying,
            BoundRange<TK> range) {
            var isEnd = GetEndPredicate(range.Upper, range.Comparer);
            var cursor = GetStartCursor(underlying, range.Lower);
            while (cursor.IsNotEnd && !isEnd(cursor.Key)) {
                yield return cursor.Value;
                cursor.MoveNext();
            }
        }

        /// <summary>
        /// Returns a cursor that begins at the start bound.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        internal static BTree<TK, KeyValuePair<TK, TV>>.Cursor GetStartCursor<TK, TV>(
            BTree<TK, KeyValuePair<TK, TV>> underlying,
            Bound<TK> start)
        {
            if (start == null)
                return underlying.Begin();
            return start.IsInclusive 
                ? underlying.GreaterThanOrEqual(start.Value, underlying.RootCursor) 
                : underlying.GreaterThan(start.Value, underlying.RootCursor);
        }

        /// <summary>
        /// Returns a predicate that can be used to indicate if a value exceeds the end bound.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        internal static Func<TK, bool> GetEndPredicate<TK>(
            Bound<TK> end,
            IComparer<TK> comparer)
        {
            if (end == null)
                return _ => false;
            if (end.IsInclusive)
                return value => comparer.Compare(value, end.Value) >= 0;

            return value => comparer.Compare(value, end.Value) > 0;
        }
    }
}