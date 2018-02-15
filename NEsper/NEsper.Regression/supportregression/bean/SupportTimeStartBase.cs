///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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

	            LongdateStart = start;
	            UtildateStart = SupportDateTime.ToDate(start);
	            CaldateStart = SupportDateTime.ToDateTimeEx(start);
	            LongdateEnd = end;
	            UtildateEnd = SupportDateTime.ToDate(end);
	            CaldateEnd = SupportDateTime.ToDateTimeEx(end);
	        }
	    }

	    public long? LongdateStart { get; private set; }

	    public DateTime UtildateStart { get; private set; }

	    public DateTimeEx CaldateStart { get; private set; }

	    public long? LongdateEnd { get; private set; }

	    public DateTime UtildateEnd { get; private set; }

	    public DateTimeEx CaldateEnd { get; private set; }

	    public string Key { get; set; }
    }
} // end of namespace
