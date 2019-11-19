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
    public class IntSeqKeySix : IntSeqKey
    {
        public IntSeqKeySix(
            int one,
            int two,
            int three,
            int four,
            int five,
            int six)
        {
            One = one;
            Two = two;
            Three = three;
            Four = four;
            Five = five;
            Six = six;
        }

        public int One { get; }

        public int Two { get; }

        public int Three { get; }

        public int Four { get; }

        public int Five { get; }

        public int Six { get; }

        public bool IsParentTo(IntSeqKey other)
        {
            if (other.Length != 7) {
                return false;
            }

            var o = (IntSeqKeyMany) other;
            var array = o.Array;
            return One == array[0] &&
                   Two == array[1] &&
                   Three == array[2] &&
                   Four == array[3] &&
                   Five == array[4] &&
                   Six == array[5];
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyMany(new[] {One, Two, Three, Four, Five, Six, num});
        }

        public IntSeqKey RemoveFromEnd()
        {
            return new IntSeqKeyFive(One, Two, Three, Four, Five);
        }

        public int Length => 6;

        public int Last => Six;

        public int[] AsIntArray()
        {
            return new[] {One, Two, Three, Four, Five, Six};
        }

        public static void Write(
            DataOutput output,
            IntSeqKeySix key)
        {
            output.WriteInt(key.One);
            output.WriteInt(key.Two);
            output.WriteInt(key.Three);
            output.WriteInt(key.Four);
            output.WriteInt(key.Five);
            output.WriteInt(key.Six);
        }

        public static IntSeqKeySix Read(DataInput input)
        {
            return new IntSeqKeySix(
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt(),
                input.ReadInt());
        }

        protected bool Equals(IntSeqKeySix other)
        {
            return One == other.One &&
                   Two == other.Two &&
                   Three == other.Three &&
                   Four == other.Four &&
                   Five == other.Five &&
                   Six == other.Six;
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

            return Equals((IntSeqKeySix) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = One;
                hashCode = (hashCode * 397) ^ Two;
                hashCode = (hashCode * 397) ^ Three;
                hashCode = (hashCode * 397) ^ Four;
                hashCode = (hashCode * 397) ^ Five;
                hashCode = (hashCode * 397) ^ Six;
                return hashCode;
            }
        }
    }
} // end of namespace