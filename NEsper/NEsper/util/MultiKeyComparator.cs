///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.collection;

namespace com.espertech.esper.util
{
	/// <summary> A comparator on multikeys. The multikeys must contain the same
	/// number of values.
	/// </summary>

    [Serializable]
    public sealed class MultiKeyComparator : IComparer<MultiKeyUntyped>, MetaDefItem
	{
		private readonly bool[] _isDescendingValues;
		
		/// <summary> Ctor.</summary>
		/// <param name="isDescendingValues">each value is true if the corresponding (same index)
		/// entry in the multi-keys is to be sorted in descending order. The multikeys
		/// to be compared must have the same number of values as this array.
		/// </param>
		
        public MultiKeyComparator(bool[] isDescendingValues)
		{
			_isDescendingValues = isDescendingValues;
		}

        /// <summary>
        /// Compares the specified first values.
        /// </summary>
        /// <param name="firstValues">The first values.</param>
        /// <param name="secondValues">The second values.</param>
        /// <returns></returns>
		public int Compare(MultiKeyUntyped firstValues, MultiKeyUntyped secondValues)
		{
			if (firstValues.Count != _isDescendingValues.Length || secondValues.Count != _isDescendingValues.Length)
			{
				throw new ArgumentException("Incompatible size MultiKey sizes for comparison");
			}
			
			for (int i = 0; i < firstValues.Count; i++)
			{
				var valueOne = firstValues.Get(i);
			    var valueTwo = secondValues.Get(i);
				var isDescending = _isDescendingValues[i];
				
				int comparisonResult = CompareValues(valueOne, valueTwo, isDescending);
				if (comparisonResult != 0)
				{
					return comparisonResult;
				}
			}
			
			// Make the comparator compatible with equals
            return !Equals(firstValues, secondValues) ? - 1 : 0;
		}
		
        /// <summary>
        /// Compares two nullable values.
        /// </summary>
        /// <param name="valueOne">first value to compare</param>
        /// <param name="valueTwo">second value to compare</param>
        /// <param name="isDescending">true for descending</param>
        /// <returns>
        /// compare result
        /// </returns>
        public static int CompareValues(Object valueOne, Object valueTwo, bool isDescending)
		{
            return CompareValues(valueOne, valueTwo, isDescending, StringComparison.Ordinal);
		}

        /// <summary>
        /// Compares two nullable values using string options for use with string-typed values.
        /// </summary>
        /// <param name="valueOne">first value to compare</param>
        /// <param name="valueTwo">second value to compare</param>
        /// <param name="isDescending">true for descending</param>
        /// <param name="comparisonOptions">the options string for comparison</param>
        /// <returns>compare result</returns>
        public static int CompareValues(Object valueOne, Object valueTwo, bool isDescending, StringComparison comparisonOptions)
        {
            if (valueOne == null || valueTwo == null)
            {
                // A null value is considered equal to another null
                // value and smaller than any nonnull value
                if (valueOne == null && valueTwo == null)
                {
                    return 0;
                }
                if (valueOne == null)
                {
                    if (isDescending)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (isDescending)
                {
                    return -1;
                }
                return 1;
            }

            int multiplier = isDescending ? -1 : 1;
            if ((valueOne is string) && (valueTwo is string))
            {
                return multiplier * String.Compare((string)valueOne, (string)valueTwo, comparisonOptions);
            }

            IComparable comparable1 = valueOne as IComparable;
            if (comparable1 != null)
            {
                return multiplier * ((IComparable)valueOne).CompareTo(valueTwo);
            }

            throw new InvalidCastException("Cannot sort objects of type " + valueOne.GetType());
        }

        /// <summary>
        /// Compares two nullable values using string options for use with string-typed values.
        /// </summary>
        /// <param name="valueOne">first value to compare</param>
        /// <param name="valueTwo">second value to compare</param>
        /// <param name="isDescending">true for descending</param>
        /// <param name="comparer">the options string for comparison</param>
        /// <returns>compare result</returns>
        public static int CompareValues(Object valueOne, Object valueTwo, bool isDescending, StringComparer comparer)
        {
            if (valueOne == null || valueTwo == null)
            {
                // A null value is considered equal to another null
                // value and smaller than any nonnull value
                if (valueOne == null && valueTwo == null)
                {
                    return 0;
                }
                if (valueOne == null)
                {
                    if (isDescending)
                    {
                        return 1;
                    }
                    return -1;
                }
                if (isDescending)
                {
                    return -1;
                }
                return 1;
            }

            int multiplier = isDescending ? -1 : 1;
            if ((valueOne is string) && (valueTwo is string))
            {
                return multiplier * comparer.Compare((string)valueOne, (string)valueTwo);
            }

            IComparable comparable1 = valueOne as IComparable;
            if (comparable1 != null)
            {
                return multiplier * ((IComparable)valueOne).CompareTo(valueTwo);
            }

            throw new InvalidCastException("Cannot sort objects of type " + valueOne.GetType());
        }

    }
}
