///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.@internal.schedulesvcimpl
{
    public interface ScheduleVisitor
    {
        void Visit(ScheduleVisit visit);
    }

    public class ProxyScheduleVisitor : ScheduleVisitor
    {
        public Action<ScheduleVisit> ProcVisit;
        public void Visit(ScheduleVisit visit) => ProcVisit?.Invoke(visit);
    }
}