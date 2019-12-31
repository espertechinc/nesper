///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// Helper class that assists with operations on arrays.
    /// </summary>

    public static class Arrays
    {
        public static int GetLength(Array array)
        {
            return array.Length;
        }

        /// <summary>
        /// Compares two arrays for equality
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>

        public static bool AreEqual(
            Array array1,
            Array array2)
        {
            if (array1 == null && array2 == null) {
                return true;
            } else if (array1 == null) {
                return false;
            } else if (array2 == null) {
                return false;
            }

            if (array1.Length != array2.Length) {
                return false;
            }

            for (int ii = array1.Length - 1; ii >= 0; ii--) {
                if (!Object.Equals(array1.GetValue(ii), array2.GetValue(ii))) {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerator<T> GetEnumerator<T>(T[] array)
        {
            return Enumerate(array).GetEnumerator();
        }

        public static IEnumerable<T> Enumerate<T>(T[] array)
        {
            if (array != null) {
                int arrayLength = array.Length;
                for (int ii = 0; ii < arrayLength; ii++) {
                    yield return array[ii];
                }
            }
        }

        public static IEnumerable<T> ReverseEnumerate<T>(T[] array)
        {
            if (array != null) {
                int arrayLength = array.Length;
                for (int ii = arrayLength - 1; ii >= 0; ii--) {
                    yield return array[ii];
                }
            }
        }

        public static T[] CopyOf<T>(
            T[] values,
            int valuesLength)
        {
            T[] targetArray = new T[valuesLength];
            Array.Copy(values, 0, targetArray, 0, valuesLength);
            return targetArray;
        }

        public static IList<T> AsList<T>(params T[] array)
        {
            return array;
        }

        public static bool Contains<T>(
            this T[] values,
            Nullable<T> valueWrapped)
            where T : struct
        {
            if (valueWrapped.HasValue) {
                var unwrapped = valueWrapped.Value;
                for (int ii = 0; ii < values.Length; ii++) {
                    if (Equals(values[ii], unwrapped)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
