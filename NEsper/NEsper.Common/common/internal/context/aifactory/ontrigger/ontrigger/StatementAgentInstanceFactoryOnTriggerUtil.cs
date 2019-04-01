///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class StatementAgentInstanceFactoryOnTriggerUtil
    {
        public static StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementAgentInstanceFactoryOnTriggerInfraBase @base,
            StatementContext statementContext, int agentInstanceId)
        {
            var namedWindow = @base.NamedWindow;
            if (namedWindow != null) {
                NamedWindowInstance namedWindowInstance;
                if (agentInstanceId == -1) {
                    namedWindowInstance = namedWindow.NamedWindowInstanceNoContext;
                }
                else {
                    namedWindowInstance = namedWindow.GetNamedWindowInstance(agentInstanceId);
                }

                return namedWindowInstance.RootViewInstance.AgentInstanceContext.AgentInstanceLock;
            }

            var table = @base.Table;
            TableInstance tableInstance;
            if (agentInstanceId == -1) {
                tableInstance = table.TableInstanceNoContext;
            }
            else {
                tableInstance = table.GetTableInstance(agentInstanceId);
            }

            return tableInstance.AgentInstanceContext.AgentInstanceLock;
        }
    }
} // end of namespace