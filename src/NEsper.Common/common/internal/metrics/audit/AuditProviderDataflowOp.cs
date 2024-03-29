///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public interface AuditProviderDataflowOp
    {
        void DataflowOp(
            string dataFlowName,
            string instanceId,
            string operatorName,
            int operatorNumber,
            object[] parameters,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace