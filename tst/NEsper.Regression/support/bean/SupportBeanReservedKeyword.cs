///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanReservedKeyword
    {
        public SupportBeanReservedKeyword innerbean;

        public SupportBeanReservedKeyword(
            int seconds,
            int order)
        {
            Seconds = seconds;
            Order = order;
        }

        public int Seconds { get; set; }

        public int[] Group { get; set; }

        public int Order { get; set; }

        public SupportBeanReservedKeyword Innerbean {
            get => innerbean;
            set => innerbean = value;
        }

        public Inner Timestamp { get; set; }

        public class Inner
        {
            public int Hour { get; set; }
        }
    }
} // end of namespace