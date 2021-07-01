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
    public class IntSeqKeyThree : IntSeqKey
    {
        public IntSeqKeyThree(
            int one,
            int two,
            int three)
        {
            One = one;
            Two = two;
            Three = three;
        }

        public int One { get; }

        public int Two { get; }

        public int Three { get; }

        public bool IsParentTo(IntSeqKey other)
        {
            if (other.Length != 4) {
                return false;
            }

            var o = (IntSeqKeyFour) other;
            return One == o.One && Two == o.Two && Three == o.Three;
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyFour(One, Two, Three, num);
        }

        public IntSeqKey RemoveFromEnd()
        {
            return new IntSeqKeyTwo(One, Two);
        }

        public int Length => 3;

        public int Last => Three;

        public int[] AsIntArray()
        {
            return new[] {One, Two, Three};
        }

        protected bool Equals(IntSeqKeyThree other)
        {
            return One == other.One && Two == other.Two && Three == other.Three;
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

            return Equals((IntSeqKeyThree) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = One;
                hashCode = (hashCode * 397) ^ Two;
                hashCode = (hashCode * 397) ^ Three;
                return hashCode;
            }
        }

        public static void Write(
            DataOutput output,
            IntSeqKeyThree key)
        {
            output.WriteInt(key.One);
            output.WriteInt(key.Two);
            output.WriteInt(key.Three);
        }

        public static IntSeqKeyThree Read(DataInput input)
        {
            return new IntSeqKeyThree(input.ReadInt(), input.ReadInt(), input.ReadInt());
        }
    }
} // end of namespace