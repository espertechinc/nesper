///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;

namespace com.espertech.esper.supportregression.bean
{
	public abstract class SupportTimeStartBase
    {
	    public SupportTimeStartBase(string key, string datestr, long duration)
        {
	        Key = key;

	        if (datestr != null)
	        {
	            // expected : 2002-05-30T09:00:00.000
	            long start = DateTimeParser.ParseDefaultMSec(datestr);
	            long end = start + duration;

	            longdateStart = start;
	            utildateStart = SupportDateTime.ToDate(start);
	            caldateStart = SupportDateTime.ToDateTimeEx(start);
	            longdateEnd = end;
	            utildateEnd = SupportDateTime.ToDate(end);
	            caldateEnd = SupportDateTime.ToDateTimeEx(end);
	        }
	    }

        [PropertyName("longdateStart")]
	    public long? longdateStart { get; private set; }

        [PropertyName("utildateStart")]
	    public DateTime utildateStart { get; private set; }

        [PropertyName("caldateStart")]
	    public DateTimeEx caldateStart { get; private set; }

        [PropertyName("longdateEnd")]
	    public long? longdateEnd { get; private set; }

        [PropertyName("utildateEnd")]
	    public DateTime utildateEnd { get; private set; }

        [PropertyName("caldateEnd")]
	    public DateTimeEx caldateEnd { get; private set; }

	    public string Key { get; set; }
    }
} // end of namespace
