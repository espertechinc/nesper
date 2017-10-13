///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.datetime.calop
{
	public class CalendarOpPlusFastAddResult
    {
        public CalendarOpPlusFastAddResult(long factor, DateTimeEx scheduled)
        {
	        Factor = factor;
	        Scheduled = scheduled;
	    }

	    public long Factor { get; private set; }

        public DateTimeEx Scheduled { get; private set; }
    }
} // end of namespace
