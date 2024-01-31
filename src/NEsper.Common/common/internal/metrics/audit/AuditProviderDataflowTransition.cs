///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public interface AuditProviderDataflowTransition
    {
        void DataflowTransition(
            string dataflowName,
            string dataFlowInstanceId,
            EPDataFlowState state,
            EPDataFlowState newState,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace