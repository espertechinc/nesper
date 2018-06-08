///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.expression.time
{
    public interface TimeAbacus 
    {
        long DeltaForSecondsNumber(object timeInSeconds);
    
        long DeltaForSecondsDouble(double seconds);
    
        long CalendarSet(long fromTime, DateTimeEx dt);

        long CalendarGet(DateTimeEx dt, long remainder);

        long OneSecond { get; }

        DateTimeEx ToDate(long ts);
    }
} // end of namespace
