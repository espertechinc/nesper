///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    /// View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class InfraOnDeleteViewFactory : InfraOnExprBaseViewFactory
    {
        public InfraOnDeleteViewFactory(EventType infaEventType)
            : base(infaEventType)

        {
        }

        public override InfraOnExprBaseViewResult MakeNamedWindow(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance namedWindowRootViewInstance,
            AgentInstanceContext agentInstanceContext)
        {
            return new InfraOnExprBaseViewResult(
                new OnExprViewNamedWindowDelete(lookupStrategy, namedWindowRootViewInstance, agentInstanceContext),
                null);
        }

        public override InfraOnExprBaseViewResult MakeTable(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext)
        {
            return new InfraOnExprBaseViewResult(
                new OnExprViewTableDelete(lookupStrategy, tableInstance, agentInstanceContext),
                null);
        }
    }
} // end of namespace