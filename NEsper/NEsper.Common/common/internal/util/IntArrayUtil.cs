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
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.util
{
    public class IntArrayUtil
    {
        public static readonly int[] EMPTY_ARRAY = new int[0];

        public static int[] GetParentPath(int[] path)
        {
            var parent = new int[path.Length - 1];
            for (var i = 0; i < path.Length - 1; i++) {
                parent[i] = path[i];
            }

            return parent;
        }

        public static void WriteOptionalArray(
            int[] ints,
            DataOutput output)
        {
            if (ints == null) {
                output.WriteBoolean(false);
                return;
            }

            output.WriteBoolean(true);
            WriteArray(ints, output);
        }

        public static void WriteArray(
            int[] ints,
            DataOutput output)
        {
            output.WriteInt(ints.Length);
            foreach (var value in ints) {
                output.WriteInt(value);
            }
        }

        public static int[] ReadOptionalArray(DataInput input)
        {
            var hasValue = input.ReadBoolean();
            if (!hasValue) {
                return null;
            }

            return ReadArray(input);
        }

        public static int[] ReadArray(DataInput input)
        {
            var size = input.ReadInt();
            var stamps = new int[size];
            for (var i = 0; i < size; i++) {
                stamps[i] = input.ReadInt();
            }

            return stamps;
        }

        public static IEnumerator<int> ToEnumerator(int[] array)
        {
            for (int ii = 0; ii < array.Length; ii++) {
                yield return array[ii];
            }
        }

        public static int[] Append(
            int[] array,
            int value)
        {
            var newArray = new int[array.Length + 1];
            Array.Copy(array, 0, newArray, 0, array.Length);
            newArray[array.Length] = value;
            return newArray;
        }

        public static int[] Copy(int[] src)
        {
            var copy = new int[src.Length];
            Array.Copy(src, 0, copy, 0, src.Length);
            return copy;
        }

        public static int[] ToArray(ICollection<int> collection)
        {
            var values = new int[collection.Count];
            var index = 0;
            foreach (var value in collection) {
                values[index++] = value;
            }

            return values;
        }

        public static int?[] ToBoxedArray(ICollection<int> collection)
        {
            var values = new int?[collection.Count];
            var index = 0;
            foreach (var value in collection) {
                values[index++] = value;
            }

            return values;
        }

        public static bool CompareParentKey(
            int[] key,
            int[] parentKey)
        {
            if (key.Length - 1 != parentKey.Length) {
                return false;
            }

            for (var i = 0; i < parentKey.Length; i++) {
                if (key[i] != parentKey[i]) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace