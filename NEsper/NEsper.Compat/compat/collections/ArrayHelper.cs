///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

    public static class ArrayHelper
    {
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
            if (array1 == null) {
                throw new ArgumentNullException("array1");
            }

            if (array2 == null) {
                throw new ArgumentNullException("array2");
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

        public static IEnumerable<T> Iterate<T>(T[] array)
        {
            if (array != null) {
                int arrayLength = array.Length;
                for (int ii = 0; ii < arrayLength; ii++) {
                    yield return array[ii];
                }
            }
        }

        public static IEnumerable<T> ReverseIterate<T>(T[] array)
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
    }
}
