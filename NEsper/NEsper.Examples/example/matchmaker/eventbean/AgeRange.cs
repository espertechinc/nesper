///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

        public int Low
        {
            get { return low; }
        }

        public int High
        {
            get { return high; }
        }

        public override string ToString()
        {
            return typeof (AgeRange).FullName +
                   "{high=" + high +
                   ",low=" + low +
                   "}";
        }

        public override bool Equals(object obj)
        {
            AgeRange other = obj as AgeRange;
            if ( other == null )
            {
                return false;
            }

            return
                other.low == this.low &&
                other.high == this.high;
        }
    }
}
