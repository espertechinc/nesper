///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// Provided a 2-dimensional array of values, provide all possible combinations:
    /// <pre>
    ///     For example, an array { {1}, {"A", "B"}, {"X", "Y"} } provides these combinations:
    ///        1 A X
    ///        1 A Y
    ///        1 B X
    ///        1 B Y
    /// </pre>.
    /// Usage Note: Do not hold on to the returned object array as {@link #nextElement()} returns the same array
    /// with changed values for each enumeration.
    /// <p>
    ///   Each array element must be non-null and length 1 or more.
    /// </p>
    /// <p>
    ///   Does not detect duplicates in values.
    /// </p>
    /// <p>
    ///   Allows any number for the first dimension.
    /// </p>
    /// <p>
    ///   The algorithm adds 1 to the right and overflows until done.
    /// </p>
    /// </summary>
    public class CombinationEnumeration : IEnumerator<Object[]>
    {
        private readonly IEnumerator<object[]> _subEnumerator;

        public CombinationEnumeration(Object[][] combinations)
        {
            _subEnumerator = New(combinations).GetEnumerator();
        }

        object IEnumerator.Current => Current;

        public object[] Current => _subEnumerator.Current;

        public bool MoveNext()
        {
            return _subEnumerator.MoveNext();
        }

        /// <summary>
        /// Semi-linear syntactic view of the enumeration process
        /// </summary>
        /// <param name="combinations"></param>
        /// <returns></returns>

        public static IEnumerable<Object[]> New(Object[][] combinations)
        {
            if (combinations.Any(element => element == null || element.Length < 1))
            {
                throw new ArgumentException("Expecting non-null element of minimum length 1");
            }

            return NewInternal(combinations);
        }

        public static IEnumerable<Object[]> FromZeroBasedRanges(int[] zeroBasedRanges)
        {
            var combinations = new Object[zeroBasedRanges.Length][];
            for (int i = 0; i < zeroBasedRanges.Length; i++)
            {
                combinations[i] = new Object[zeroBasedRanges[i]];
                for (int j = 0; j < zeroBasedRanges[i]; j++)
                {
                    combinations[i][j] = j;
                }
            }
            return NewInternal(combinations);
        }

        /// <summary>
        /// Guts of the enumeration process go here.  This allows the method above to
        /// validate the inputs while this portion does the rest on demand.
        /// </summary>
        /// <param name="combinations"></param>
        /// <returns></returns>

        private static IEnumerable<object[]> NewInternal(object[][] combinations)
        {
            var current = new int[combinations.Length];
            var prototype = new Object[combinations.Length];
            var hasMore = true;

            while (hasMore)
            {
                Populate(combinations, prototype, current);
                hasMore = DetermineNext(combinations, prototype, current);
                yield return prototype;
            }
        }

        private static void Populate(object[][] combinations, object[] prototype, int[] current)
        {
            for (int i = 0; i < prototype.Length; i++)
            {
                prototype[i] = combinations[i][current[i]];
            }
        }

        private static bool DetermineNext(object[][] combinations, object[] prototype, int[] current)
        {
            for (int i = combinations.Length - 1; i >= 0; i--)
            {
                int max = combinations[i].Length;
                if (current[i] < max - 1)
                {
                    current[i]++;
                    return true;
                }
                // overflowing
                current[i] = 0;
            }

            return false;
        }


        public void Reset()
        {
            throw new UnsupportedOperationException();
        }

        public void Dispose()
        {
        }
    }
}
