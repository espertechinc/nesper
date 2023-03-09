///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace NEsper.Examples.MatchMaker.eventbean
{
    public class AgeRange
    {
        public static readonly AgeRange AGE_1 = new AgeRange(18, 25);
        public static readonly AgeRange AGE_2 = new AgeRange(26, 35);
        public static readonly AgeRange AGE_3 = new AgeRange(36, 45);
        public static readonly AgeRange AGE_4 = new AgeRange(46, 55);
        public static readonly AgeRange AGE_5 = new AgeRange(55, 65);
        public static readonly AgeRange AGE_6 = new AgeRange(65, Int32.MaxValue);

        public static readonly AgeRange[] Values = new AgeRange[]
            {
                AGE_1,
                AGE_2,
                AGE_3,
                AGE_4,
                AGE_5,
                AGE_6
            };

        private int low;
        private int high;

        public AgeRange(int low, int high)
        {
            this.low = low;
            this.high = high;
        }

        public int Low => low;

        public int High => high;

        public override string ToString()
        {
            return typeof (AgeRange).FullName +
                   "{high=" + high +
                   ",low=" + low +
                   "}";
        }

        protected bool Equals(AgeRange other)
        {
            return low == other.low && high == other.high;
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

            return Equals((AgeRange)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(low, high);
        }
    }
}
