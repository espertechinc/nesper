///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.supportunit.bean
{
	public abstract class SupportTimeStartBase
    {
	    private string _key;
	    private readonly long? _longdateStart;
        private readonly DateTimeOffset _utildateStart;
	    private readonly DateTimeEx _dateTimeExStart;
	    private readonly long? _longdateEnd;
	    private readonly DateTimeOffset _utildateEnd;
        private readonly DateTimeEx _dateTimeExEnd;

	    public SupportTimeStartBase(string key, string datestr, long duration)
        {
	        _key = key;

	        if (datestr != null) {
	            // expected : 2002-05-30T09:00:00.000
	            long start = DateTimeParser.ParseDefaultMSec(datestr);
	            long end = start + duration;

	            _longdateStart = start;
	            _utildateStart = SupportDateTime.ToDate(start);
	            _dateTimeExStart = SupportDateTime.ToDateTimeEx(start);
	            _longdateEnd = end;
	            _utildateEnd = SupportDateTime.ToDate(end);
                _dateTimeExEnd = SupportDateTime.ToDateTimeEx(end);
	        }
	    }

	    public long? LongdateStart
	    {
	        get { return _longdateStart; }
	    }

	    public DateTimeOffset UtildateStart
	    {
	        get { return _utildateStart; }
	    }

	    public DateTimeEx DateTimeExStart
	    {
	        get { return _dateTimeExStart; }
	    }

	    public long? LongdateEnd
	    {
	        get { return _longdateEnd; }
	    }

	    public DateTimeOffset UtildateEnd
	    {
	        get { return _utildateEnd; }
	    }

	    public DateTimeEx DateTimeExEnd
	    {
	        get { return _dateTimeExEnd; }
	    }

	    public string Key
	    {
	        get { return _key; }
	        set { _key = value; }
	    }
	}
} // end of namespace
