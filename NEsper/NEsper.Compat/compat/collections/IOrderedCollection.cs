using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public interface IOrderedCollection<TV> : ICollection<TV>
    {
        /// <summary>
        /// Returns the first value in the collection.  If the collection is empty, this method throws
        /// an IllegalOperationException.
        /// </summary>
        TV FirstEntry { get; }

        /// <summary>
        /// Returns the last value in the collection.  If the collection is empty, this method throws
        /// an IllegalOperationException.
        /// </summary>
        TV LastEntry { get; }

        /// <summary>
        /// Returns a readonly ordered collection that includes everything before the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        IOrderedCollection<TV> Head(
            TV value,
            bool isInclusive = false);

        /// <summary>
        /// Returns a readonly ordered collection that includes everything after the value.
        /// Whether the value is included in the range depends on whether the isInclusive
        /// flag is set.
        /// </summary>
        /// <param name="value">The end value.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        IOrderedCollection<TV> Tail(
            TV value,
            bool isInclusive = true);

        /// <summary>
        /// Returns a readonly ordered collection that includes everything between the
        /// Returns a readonly ordered dictionary that includes everything between the
        /// two provided values.  Whether each value is included in the range depends
        /// on whether the isInclusive flag is set.
        /// </summary>
        /// <returns></returns>
        IOrderedCollection<TV> Between(
            TV startValue,
            bool isStartInclusive,
            TV endValue,
            bool isEndInclusive);

        /// <summary>
        /// Finds the first value greater than or equal to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TV GreaterThanOrEqualTo(TV value);

        /// <summary>
        /// Finds the first value greater than or equal to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGreaterThanOrEqualTo(TV value, out TV result);

        /// <summary>
        /// Finds the first value less than or equal to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TV LessThanOrEqualTo(TV value);

        /// <summary>
        /// Finds the first value less than or equal to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryLessThanOrEqualTo(TV value, out TV result);

        /// <summary>
        /// Finds the first value greater than to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TV GreaterThan(TV value);

        /// <summary>
        /// Finds the first value greater than to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGreaterThan(TV value, out TV result);

        /// <summary>
        /// Finds the first value less than to the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TV LessThan(TV value);

        /// <summary>
        /// Finds the first value less than the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryLessThan(TV value, out TV result);
    }
}