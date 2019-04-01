///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeMethodEnumStatics
    {
        public static readonly ForgeFactory CALENDAR_FORGE_FACTORY = new CalendarForgeFactory();
        public static readonly ForgeFactory REFORMAT_FORGE_FACTORY = new ReformatForgeFactory();
        public static readonly ForgeFactory INTERVAL_FORGE_FACTORY = new IntervalForgeFactory();
    }
} // end of namespace