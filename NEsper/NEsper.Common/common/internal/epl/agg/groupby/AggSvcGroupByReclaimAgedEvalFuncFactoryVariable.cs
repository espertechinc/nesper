///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public class AggSvcGroupByReclaimAgedEvalFuncFactoryVariable : AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        private readonly Variable variable;

        public AggSvcGroupByReclaimAgedEvalFuncFactoryVariable(Variable variable)
        {
            this.variable = variable;
        }

        public AggSvcGroupByReclaimAgedEvalFunc Make(AgentInstanceContext agentInstanceContext)
        {
            VariableReader reader = agentInstanceContext.VariableManagementService.GetReader(
                variable.DeploymentId,
                variable.MetaData.VariableName,
                agentInstanceContext.AgentInstanceId);
            return new AggSvcGroupByReclaimAgedEvalFuncVariable(reader);
        }
    }
} // end of namespace