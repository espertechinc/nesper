///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodTargetStrategyVariableFactory : MethodTargetStrategyFactory,
        StatementReadyCallback
    {
        protected internal MethodTargetStrategyStaticMethodInvokeType invokeType;
        protected internal MethodInfo method;

        public Variable Variable { get; set; }

        public string MethodName { get; set; }

        public Type[] MethodParameters { get; set; }

        public MethodTargetStrategy Make(AgentInstanceContext agentInstanceContext)
        {
            var reader = agentInstanceContext.VariableManagementService.GetReader(
                Variable.DeploymentId,
                Variable.MetaData.VariableName,
                agentInstanceContext.AgentInstanceId);
            return new MethodTargetStrategyVariable(this, reader);
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            method = MethodTargetStrategyStaticMethod.ResolveMethod(
                Variable.MetaData.Type,
                MethodName,
                MethodParameters);
            invokeType = MethodTargetStrategyStaticMethodInvokeTypeExtensions.GetInvokeType(method);
        }
    }
} // end of namespace