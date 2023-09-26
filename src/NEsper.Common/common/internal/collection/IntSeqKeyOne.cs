///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.collection
{
    public class IntSeqKeyOne : IntSeqKey
    {
        public IntSeqKeyOne(int one)
        {
            One = one;
        }

        public int One { get; }

        public bool IsParentTo(IntSeqKey other)
        {
            if (other.Length != 2) {
                return false;
            }

            var o = (IntSeqKeyTwo)other;
            return One == o.One;
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyTwo(One, num);
        }

        public IntSeqKey RemoveFromEnd()
        {
            return IntSeqKeyRoot.INSTANCE;
        }

        public int Length => 1;

        public int Last => One;

        public int[] AsIntArray()
        {
            return new[] { One };
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (IntSeqKeyOne)o;

            return One == that.One;
        }

        public override int GetHashCode()
        {
            return One;
        }

        public static void Write(
            DataOutput output,
            IntSeqKeyOne key)
        {
            output.WriteInt(key.One);
        }

        public static IntSeqKeyOne Read(DataInput input)
        {
            return new IntSeqKeyOne(input.ReadInt());
        }
    }
} // end of namespace