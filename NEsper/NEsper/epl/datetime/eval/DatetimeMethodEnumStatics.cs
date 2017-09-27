///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.datetime.reformatop;

namespace com.espertech.esper.epl.datetime.eval
{
    public class DatetimeMethodEnumStatics
    {
        public static readonly OpFactory CALENDAR_OP_FACTORY = new CalendarOpFactory();
        public static readonly OpFactory REFORMAT_OP_FACTORY = new ReformatOpFactory();
        public static readonly OpFactory INTERVAL_OP_FACTORY = new IntervalOpFactory();
    }
} // end of namespace
