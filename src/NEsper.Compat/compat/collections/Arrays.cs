///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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

            for (var ii = array1.Length - 1; ii >= 0; ii--) {
                if (!Equals(array1.GetValue(ii), array2.GetValue(ii))) {
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
                var arrayLength = array.Length;
                for (var ii = 0; ii < arrayLength; ii++) {
                    yield return array[ii];
                }
            }
        }

        public static IEnumerable<T> ReverseEnumerate<T>(T[] array)
        {
            if (array != null) {
                var arrayLength = array.Length;
                for (var ii = arrayLength - 1; ii >= 0; ii--) {
                    yield return array[ii];
                }
            }
        }

        public static T[] CopyOf<T>(
            T[] values,
            int valuesLength)
        {
            var targetArray = new T[valuesLength];
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
                for (var ii = 0; ii < values.Length; ii++) {
                    if (Equals(values[ii], unwrapped)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool DeepEquals(
            Array left,
            Array right)
        {
            return CompatExtensions.DeepEquals(left, right);
        }

        public static Array CreateInstanceChecked(
            Type elementType,
            int dim1Size)
        {
            return Array.CreateInstance(elementType, dim1Size);
        }

        public static Array CreateInstanceChecked(
            Type elementType,
            int[] dimensions)
        {
            // Currently we support one and two dimension arrays created through the checked
            // instance.  We will rewrite with a more general purpose algorithm to support
            // true multi-dimension array initialization.

            switch (dimensions.Length) {
                case 1:
                    return Array.CreateInstance(elementType, dimensions[0]);
                case 2:
                    return CreateJagged(elementType, dimensions[0], dimensions[1]);
                default:
                    throw new NotSupportedException();
            }
        }
        
        public static Array CreateJagged(
            Type elementType,
            int dim1Size)
        {
            return Array.CreateInstance(elementType, dim1Size);
        }
        
        public static Array CreateJagged(
            Type elementType,
            int dim1Size,
            int dim2Size)
        {
            var dim2ArrayType = elementType.MakeArrayType();
            var dim1Array = Array.CreateInstance(dim2ArrayType, dim1Size);
            // for (int dim1 = 0; dim1 < dim1Size; dim1++) {
            //     var dim2Array = Array.CreateInstance(elementType, dim2Size);
            //     dim1Array.SetValue(dim2Array, dim1);
            // }

            return dim1Array;
        }

        public static Array CreateJagged(
            Type elementType,
            int dim1Size,
            int dim2Size,
            int dim3Size)
        {
            var dim3ArrayType = elementType.MakeArrayType();
            var dim2ArrayType = dim3ArrayType.MakeArrayType();
            var dim1Array = Array.CreateInstance(dim2ArrayType, dim1Size);
            for (var dim1 = 0; dim1 < dim1Size; dim1++) {
                var dim2Array = Array.CreateInstance(dim3ArrayType, dim2Size);
                // for (int dim2 = 0; dim2 < dim2Size; dim2++) {
                //     var dim3Array = Array.CreateInstance(elementType, dim3Size);
                //     dim2Array.SetValue(dim3Array, dim2);
                // }
                dim1Array.SetValue(dim2Array, dim1);
            }

            return dim1Array;
        }

        public static Array TryCopy(Array sourceArray)
        {
            var lengths = Enumerable
                .Range(0, sourceArray.Rank)
                .Select(sourceArray.GetLength)
                .ToArray();
            var targetArray = Array.CreateInstance(
                sourceArray.GetType().GetElementType(),
                lengths);
            sourceArray.CopyTo(targetArray, 0);
            return sourceArray;
        }
    }
}
