///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
    public class StageStatementHelper
    {
        public static void UpdateStatement(
            StatementContext statementContext,
            EPServicesEvaluation svc)
        {
            statementContext.FilterService = svc.FilterService;
            statementContext.SchedulingService = svc.SchedulingService;
            statementContext.InternalEventRouteDest = svc.InternalEventRouteDest;
        }
    }
} // end of namespace