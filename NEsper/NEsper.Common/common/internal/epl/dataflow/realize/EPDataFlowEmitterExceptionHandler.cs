///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class EPDataFlowEmitterExceptionHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EPDataFlowEmitterExceptionHandler));

        private readonly AgentInstanceContext agentInstanceContext;
        private readonly string instanceId;

        public EPDataFlowEmitterExceptionHandler(
            AgentInstanceContext agentInstanceContext,
            string dataFlowName,
            string instanceId,
            string operatorName,
            int operatorNumber,
            string operatorPrettyPrint,
            EPDataFlowExceptionHandler optionalExceptionHandler)
        {
            this.agentInstanceContext = agentInstanceContext;
            DataFlowName = dataFlowName;
            this.instanceId = instanceId;
            OperatorName = operatorName;
            OperatorNumber = operatorNumber;
            OperatorPrettyPrint = operatorPrettyPrint;
            OptionalExceptionHandler = optionalExceptionHandler;
        }

        public string RuntimeURI => agentInstanceContext.RuntimeURI;

        public string StatementName => agentInstanceContext.StatementName;

        public string DataFlowName { get; }

        public string OperatorName { get; }

        public int OperatorNumber { get; }

        public string OperatorPrettyPrint { get; }

        public EPDataFlowExceptionHandler OptionalExceptionHandler { get; }

        public string DeploymentId => agentInstanceContext.DeploymentId;

        public void HandleException(
            object targetObject,
            MethodInfo fastMethod,
            TargetException ex,
            object[] parameters)
        {
            log.Error("Exception encountered: " + ex.InnerException?.Message, ex.InnerException);
            HandleExceptionCommon(targetObject, fastMethod, ex.InnerException, parameters);
        }

        public void HandleException(
            object targetObject,
            MethodInfo fastMethod,
            TargetInvocationException ex,
            object[] parameters)
        {
            log.Error("Exception encountered: " + ex.InnerException?.Message, ex.InnerException);
            HandleExceptionCommon(targetObject, fastMethod, ex.InnerException, parameters);
        }

        public void HandleException(
            object targetObject,
            MethodInfo fastMethod,
            MemberAccessException ex,
            object[] parameters)
        {
            log.Error("Exception encountered: " + ex.Message, ex);
            HandleExceptionCommon(targetObject, fastMethod, ex, parameters);
        }

        internal void HandleExceptionCommon(
            object targetObject,
            MethodInfo fastMethod,
            Exception ex,
            object[] parameters)
        {
            OptionalExceptionHandler?.Handle(
                new EPDataFlowExceptionContext(
                    DataFlowName,
                    OperatorName,
                    OperatorNumber,
                    OperatorPrettyPrint,
                    ex));
        }

        public void HandleAudit(
            object targetObject,
            object[] parameters)
        {
            agentInstanceContext.AuditProvider.DataflowOp(
                DataFlowName,
                instanceId,
                OperatorName,
                OperatorNumber,
                parameters,
                agentInstanceContext);
        }
    }
} // end of namespace