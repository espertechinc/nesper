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
    public class IntSeqKeyFive : IntSeqKey
    {
        public IntSeqKeyFive(
            int one,
            int two,
            int three,
            int four,
            int five)
        {
            One = one;
            Two = two;
            Three = three;
            Four = four;
            Five = five;
        }

        public int One { get; }

        public int Two { get; }

        public int Three { get; }

        public int Four { get; }

        public int Five { get; }

        public bool IsParentTo(IntSeqKey other)
        {
            if (other.Length != 6) {
                return false;
            }

            var o = (IntSeqKeySix) other;
            return (o.One == One) &&
                   (o.Two == Two) &&
                   (o.Three == Three) &&
                   (o.Four == Four) &&
                   (o.Five == Five);
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeySix(One, Two, Three, Four, Five, num);
        }

        public IntSeqKey RemoveFromEnd()
        {
            return new IntSeqKeyFour(One, Two, Three, Four);
        }

        public int Length => 5;

        public int Last => Five;

        public int[] AsIntArray()
        {
            return new[] {One, Two, Three, Four, Five};
        }

        public static void Write(
            DataOutput output,
            IntSeqKeyFive key)
        {
            output.WriteInt(key.One);
            output.WriteInt(key.Two);
            output.WriteInt(key.Three);
            output.WriteInt(key.Four);
            output.WriteInt(key.Five);
        }

        public static IntSeqKeyFive Read(DataInput input)
        {
            return new IntSeqKeyFive(
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt());
        }

        protected bool Equals(IntSeqKeyFive other)
        {
            return One == other.One &&
                   Two == other.Two &&
                   Three == other.Three &&
                   Four == other.Four &&
                   Five == other.Five;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((IntSeqKeyFive) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = One;
                hashCode = (hashCode * 397) ^ Two;
                hashCode = (hashCode * 397) ^ Three;
                hashCode = (hashCode * 397) ^ Four;
                hashCode = (hashCode * 397) ^ Five;
                return hashCode;
            }
        }
    }
} // end of namespace