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
    public class IntSeqKeyTwo : IntSeqKey
    {
        public IntSeqKeyTwo(
            int one,
            int two)
        {
            One = one;
            Two = two;
        }

        public int One { get; }

        public int Two { get; }

        public int Length => 2;
        public int Last => Two;

        public bool IsParentTo(IntSeqKey other)
        {
            if (other.Length != 3) {
                return false;
            }

            var o = (IntSeqKeyThree)other;
            return One == o.One && Last == o.Two;
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyThree(One, Two, num);
        }

        public IntSeqKey RemoveFromEnd()
        {
            return new IntSeqKeyOne(One);
        }

        public int[] AsIntArray()
        {
            return new[] { One, Two };
        }

        public static void Write(
            DataOutput output,
            IntSeqKeyTwo key)
        {
            output.WriteInt(key.One);
            output.WriteInt(key.Last);
        }

        public static IntSeqKeyTwo Read(DataInput input)
        {
            return new IntSeqKeyTwo(input.ReadInt(), input.ReadInt());
        }

        protected bool Equals(IntSeqKeyTwo other)
        {
            return One == other.One && Two == other.Two;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((IntSeqKeyTwo)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (One * 397) ^ Two;
            }
        }
    }
} // end of namespace