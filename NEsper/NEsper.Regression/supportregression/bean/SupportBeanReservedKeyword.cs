///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.annotation;

namespace com.espertech.esper.supportregression.bean
{
	public class SupportBeanReservedKeyword
	{
        [PropertyName("timestamp")]
        public Inner Timestamp { get; set; }

        [PropertyName("innerbean")]
        public SupportBeanReservedKeyword Innerbean { get; set; }

        [PropertyName("seconds")]
        public int Seconds { get; set; }

        [PropertyName("order")]
        public int Order { get; set; }

        [PropertyName("group")]
        public int[] Group { get; set; }

	    public SupportBeanReservedKeyword(int seconds, int order)
	    {
	        Seconds = seconds;
	        Order = order;
	    }

        public class Inner
        {
            [PropertyName("hour")]
            public int Hour { get; set; }
        }
	}

    public class SupportBeanReservedKeywordNested
    {
        public SupportBeanReservedKeyword Nested { get; set; }

        public SupportBeanReservedKeywordNested(SupportBeanReservedKeyword nested)
        {
            Nested = nested;
        }
    }
} // End of namespace
