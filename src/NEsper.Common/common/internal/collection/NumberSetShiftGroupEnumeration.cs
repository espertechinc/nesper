///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Enumeration that first returns a round-shift-left of all numbers and
    /// when that is exhausted it returns number sets using grouped algo until
    /// exhausted.
    /// </summary>
    public class NumberSetShiftGroupEnumeration
    {
        public static IEnumerable<int[]> New(int[] numberSet)
        {
            if (numberSet.Length < 6) {
                throw new ArgumentException("Only supported for at least 6-number sets");
            }

            return CreateInternal(numberSet);
        }

        protected internal static IEnumerable<int[]> CreateInternal(int[] numberSet)
        {
            for (var shiftCount = 0; shiftCount < numberSet.Length; shiftCount++) {
                var result = new int[numberSet.Length];
                var count = shiftCount;
                for (var i = 0; i < numberSet.Length; i++) {
                    var index = count + i;
                    if (index >= numberSet.Length) {
                        index -= numberSet.Length;
                    }

                    result[i] = numberSet[index];
                }

                yield return result;
            }

            // Initialize the permutation
            // simply always make 4 buckets
            var buckets = new Dictionary<int, List<int>>();
            for (var i = 0; i < numberSet.Length; i++) {
                var bucketNum = i % 4;
                var bucket = buckets.Get(bucketNum);
                if (bucket == null) {
                    bucket = new List<int>();
                    buckets[bucketNum] = bucket;
                }

                bucket.Add(numberSet[i]);
            }

            var permutationEnumerator = PermutationEnumerator.Create(4).GetEnumerator();
            // we throw the first one away, it is the same as a shift result
            permutationEnumerator.MoveNext();

            while (permutationEnumerator.MoveNext()) {
                yield return Translate(numberSet, buckets, permutationEnumerator.Current);
            }
        }

        private static int[] Translate(
            int[] numberSet,
            IDictionary<int, List<int>> buckets,
            int[] bucketsPermuted)
        {
            var result = new int[numberSet.Length];
            var count = 0;
            for (var i = 0; i < bucketsPermuted.Length; i++) {
                var bucket = buckets.Get(bucketsPermuted[i]);
                foreach (var j in bucket) {
                    result[count++] = j;
                }
            }

            return result;
        }
    }
}