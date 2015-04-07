///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.core.start
{
    public class EPPreparedExecuteTableHelper
    {
        public static void AssignTableAccessStrategies(EPServicesContext services, ExprTableAccessNode[] optionalTableNodes, AgentInstanceContext agentInstanceContext)
        {
            if (optionalTableNodes == null)
            {
                return;
            }
            var strategies = EPStatementStartMethodHelperTableAccess.AttachTableAccess(services, agentInstanceContext, optionalTableNodes);
            foreach (var strategyEntry in strategies)
            {
                strategyEntry.Key.Strategy = strategyEntry.Value;
            }
        }
    }
}
