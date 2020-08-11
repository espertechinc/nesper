using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public interface IOrderedDictionary<TK, TV> : IDictionary<TK, TV>
    {
        /// <summary>
        /// Returns a comparer for the key.
        /// </summary>
        IComparer<TK> KeyComparer { get; }
        
        /// <summary>
        /// Returns the first key-value pair in the dictionary.  If the dictionary
        /// is empty, this method throws an exception.
        /// </summary>
        KeyValuePair<TK, TV> FirstEntry { get; }

        /// <summary>
        /// Returns the last key-value pair in the dictionary.  If the dictionary
        /// is empty, this method throws an exception.
        /// </summary>
        KeyValuePair<TK, TV> LastEntry { get; }

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything before the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        IOrderedDictionary<TK, TV> Head(
            TK value,
            bool isInclusive = false);

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything after the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        IOrderedDictionary<TK, TV> Tail(
            TK value,
            bool isInclusive = true);

        /// <summary>
        /// Returns a readonly ordered dictionary that includes everything between the
        /// two provided values.  Whether each value is included in the range depends
        /// on whether the isInclusive flag is set.
        /// </summary>
        /// <returns></returns>
        IOrderedDictionary<TK, TV> Between(
            TK startValue,
            bool isStartInclusive,
            TK endValue,
            bool isEndInclusive);

        /// <summary>
        /// Finds a key-value mapping associated with the least key greater than or equal to the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KeyValuePair<TK, TV>? GreaterThanOrEqualTo(TK key);

        /// <summary>
        /// Finds a key-value mapping associated with the least key greater than or equal to the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        bool TryGreaterThanOrEqualTo(TK key, out KeyValuePair<TK, TV> valuePair);

        /// <summary>
        /// Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KeyValuePair<TK, TV>? LessThanOrEqualTo(TK key);

        /// <summary>
        /// Finds a key-value mapping associated with the greatest key less than or equal to the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        bool TryLessThanOrEqualTo(TK key, out KeyValuePair<TK, TV> valuePair);

        /// <summary>
        /// Finds a key-value mapping associated with the least key strictly greater than the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KeyValuePair<TK, TV>? GreaterThan(TK key);

        /// <summary>
        /// Finds a key-value mapping associated with the least key strictly greater than the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        bool TryGreaterThan(TK key, out KeyValuePair<TK, TV> valuePair);

        /// <summary>
        /// Finds a key-value mapping associated with the greatest key strictly less than the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        KeyValuePair<TK, TV>? LessThan(TK key);

        /// <summary>
        /// Finds a key-value mapping associated with the greatest key strictly less than the given key.
        /// Outputs the valuePair if found and returns true.  Otherwise, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valuePair"></param>
        /// <returns></returns>
        bool TryLessThan(TK key, out KeyValuePair<TK, TV> valuePair);
    }
}