///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.collection
{
    public class IntSeqKeyMany : IntSeqKey
    {
        public IntSeqKeyMany(int[] array)
        {
            if (array.Length < 7) {
                throw new ArgumentException("Array size less than 7");
            }

            Array = array;
        }

        public int[] Array { get; }

        public bool IsParentTo(IntSeqKey other)
        {
            if (!(other is IntSeqKeyMany many)) {
                return false;
            }

            return IntArrayUtil.CompareParentKey(many.Array, Array);
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyMany(IntArrayUtil.Append(Array, num));
        }

        public IntSeqKey RemoveFromEnd()
        {
            if (Array.Length > 7) {
                return new IntSeqKeyMany(IntArrayUtil.GetParentPath(Array));
            }

            return new IntSeqKeySix(Array[0], Array[1], Array[2], Array[3], Array[4], Array[5]);
        }

        public int Length => Array.Length;

        public int Last => Array[^1];

        public int[] AsIntArray()
        {
            return Array;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (IntSeqKeyMany)o;
            return Array.AreEqual(that.Array);
        }

        public override int GetHashCode()
        {
            return CompatExtensions.Hash(Array);
        }

        public static void Write(
            DataOutput output,
            IntSeqKeyMany key)
        {
            var array = key.Array;
            output.WriteInt(array.Length);
            foreach (var i in array) {
                output.WriteInt(i);
            }
        }

        public static IntSeqKeyMany Read(DataInput input)
        {
            var size = input.ReadInt();
            var array = new int[size];
            for (var i = 0; i < size; i++) {
                array[i] = input.ReadInt();
            }

            return new IntSeqKeyMany(array);
        }
    }
} // end of namespace