///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Provides a N! (n-faculty) number of permutations for N elements.
    /// Example: for 3 elements provides 6 permutations exactly as follows:
    /// {0, 1, 2}
    /// {0, 2, 1}
    /// {1, 0, 2}
    /// {1, 2, 0}
    /// {2, 0, 1}
    /// {2, 1, 0}
    /// </summary>
    public class PermutationEnumerator
    {
        /// <summary>
        /// Creates the permutation.
        /// </summary>
        /// <param name="numElements">The number of elements.</param>
        /// <returns></returns>
        public static IEnumerable<int[]> Create(int numElements)
        {
            if (numElements < 1) {
                throw new ArgumentException("Invalid element number of 1");
            }

            return CreateInternal(numElements);
        }

        public static IEnumerable<int[]> CreateInternal(int numElements)
        {
            var factors = GetFactors(numElements);
            var maxNumPermutation = Faculty(numElements);

            for (var currentPermutation = 0; currentPermutation < maxNumPermutation; currentPermutation++) {
                yield return GetPermutation(numElements, currentPermutation, factors);
            }
        }

        /// <summary>
        /// Gets the permutation.
        /// </summary>
        /// <param name="numElements">The num elements.</param>
        /// <param name="permutation">The permutation.</param>
        /// <param name="factors">factors for each index</param>
        /// <returns>permutation</returns>
        public static int[] GetPermutation(
            int numElements,
            int permutation,
            int[] factors)
        {
            /*
            Example:
                numElements = 4
                permutation = 21
                factors = {6, 2, 1, 0}

            Init:   out {0, 1, 2, 3}

            21 / 6                      == index 3 -> result {3}, out {0, 1, 2}
            remainder 21 - 3 * 6        == 3
            3 / 2 = second number       == index 1 -> result {3, 1}, out {0, 2}
            remainder 3 - 1 * 2         == 1
                                        == index 1 -> result {3, 1, 2} out {0}
            */

            var result = new int[numElements];
            var outList = new List<Int32>();
            for (int i = 0; i < numElements; i++) {
                outList.Add(i);
            }

            int currentVal = permutation;

            for (int position = 0; position < numElements - 1; position++) {
                int factor = factors[position];
                int index = currentVal / factor;
                result[position] = outList[index];
                outList.RemoveAt(index);
                currentVal -= index * factor;
            }

            result[numElements - 1] = outList[0];

            return result;
        }

        /// <summary>Returns factors for computing the permutation.</summary>
        /// <param name="numElements">number of factors to compute</param>
        /// <returns>factors list</returns>
        public static int[] GetFactors(int numElements)
        {
            int[] facultyFactors = new int[numElements];

            for (int i = 0; i < numElements - 1; i++) {
                facultyFactors[i] = Faculty(numElements - i - 1);
            }

            return facultyFactors;
        }

        /// <summary>Computes faculty of Count.</summary>
        /// <param name="num">to compute faculty for</param>
        /// <returns>Count!</returns>
        public static int Faculty(int num)
        {
            if (num == 0) {
                return 0;
            }

            int fac = 1;
            for (int i = 1; i <= num; i++) {
                fac *= i;
            }

            return fac;
        }
    }
} // end of namespace