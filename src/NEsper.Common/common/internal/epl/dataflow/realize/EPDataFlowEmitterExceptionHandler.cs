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
        private static readonly ILog Log = LogManager.GetLogger(typeof(EPDataFlowEmitterExceptionHandler));

        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly string _instanceId;

        public EPDataFlowEmitterExceptionHandler(
            AgentInstanceContext agentInstanceContext,
            string dataFlowName,
            string instanceId,
            string operatorName,
            int operatorNumber,
            string operatorPrettyPrint,
            EPDataFlowExceptionHandler optionalExceptionHandler)
        {
            _agentInstanceContext = agentInstanceContext;
            DataFlowName = dataFlowName;
            _instanceId = instanceId;
            OperatorName = operatorName;
            OperatorNumber = operatorNumber;
            OperatorPrettyPrint = operatorPrettyPrint;
            OptionalExceptionHandler = optionalExceptionHandler;
        }

        public string RuntimeURI => _agentInstanceContext.RuntimeURI;

        public string StatementName => _agentInstanceContext.StatementName;

        public string DataFlowName { get; }

        public string OperatorName { get; }

        public int OperatorNumber { get; }

        public string OperatorPrettyPrint { get; }

        public EPDataFlowExceptionHandler OptionalExceptionHandler { get; }

        public string DeploymentId => _agentInstanceContext.DeploymentId;

        public void HandleException(
            object targetObject,
            MethodInfo fastMethod,
            TargetException ex,
            object[] parameters)
        {
            Log.Error("Exception encountered: " + ex.InnerException?.Message, ex.InnerException);
            HandleExceptionCommon(targetObject, fastMethod, ex.InnerException, parameters);
        }

        public void HandleException(
            object targetObject,
            MethodInfo fastMethod,
            TargetInvocationException ex,
            object[] parameters)
        {
            Log.Error("Exception encountered: " + ex.InnerException?.Message, ex.InnerException);
            HandleExceptionCommon(targetObject, fastMethod, ex.InnerException, parameters);
        }

        public void HandleException(
            object targetObject,
            MethodInfo fastMethod,
            MemberAccessException ex,
            object[] parameters)
        {
            Log.Error("Exception encountered: " + ex.Message, ex);
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
            _agentInstanceContext.AuditProvider.DataflowOp(
                DataFlowName,
                _instanceId,
                OperatorName,
                OperatorNumber,
                parameters,
                _agentInstanceContext);
        }
    }
} // end of namespace