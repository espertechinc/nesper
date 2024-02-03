///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.datetime.calop;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public abstract class DTLocalForgeCalOpsCalBase
    {
        internal readonly IList<CalendarForge> calendarForges;

        public DTLocalForgeCalOpsCalBase(IList<CalendarForge> calendarForges)
        {
            this.calendarForges = calendarForges;
        }
    }
} // end of namespace