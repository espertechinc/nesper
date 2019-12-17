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
    public class IntSeqKeyFour : IntSeqKey
    {
        public IntSeqKeyFour(
            int one,
            int two,
            int three,
            int four)
        {
            One = one;
            Two = two;
            Three = three;
            Four = four;
        }

        public int One { get; }

        public int Two { get; }

        public int Three { get; }

        public int Four { get; }

        public bool IsParentTo(IntSeqKey other)
        {
            if (other.Length != 5) {
                return false;
            }

            var o = (IntSeqKeyFive) other;
            return One == o.One && Two == o.Two && Three == o.Three && Four == o.Four;
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyFive(One, Two, Three, Four, num);
        }

        public IntSeqKey RemoveFromEnd()
        {
            return new IntSeqKeyThree(One, Two, Three);
        }

        public int Length => 4;

        public int Last => Four;

        public int[] AsIntArray()
        {
            return new[] {One, Two, Three, Four};
        }

        protected bool Equals(IntSeqKeyFour other)
        {
            return One == other.One && Two == other.Two && Three == other.Three && Four == other.Four;
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

            return Equals((IntSeqKeyFour) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = One;
                hashCode = (hashCode * 397) ^ Two;
                hashCode = (hashCode * 397) ^ Three;
                hashCode = (hashCode * 397) ^ Four;
                return hashCode;
            }
        }

        public static void Write(
            DataOutput output,
            IntSeqKeyFour key)
        {
            output.WriteInt(key.One);
            output.WriteInt(key.Two);
            output.WriteInt(key.Three);
            output.WriteInt(key.Four);
        }

        public static IntSeqKeyFour Read(DataInput input)
        {
            return new IntSeqKeyFour(input.ReadInt(), input.ReadInt(), input.ReadInt(), input.ReadInt());
        }
    }
} // end of namespace