///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.dataflow.interfaces
{
    public class DataFlowOpInitializeContext
    {
        public DataFlowOpInitializeContext(
            IContainer container,
            string dataFlowName,
            string operatorName,
            int operatorNumber,
            AgentInstanceContext agentInstanceContext,
            IDictionary<string, object> additionalParameters,
            string dataFlowInstanceId,
            EPDataFlowOperatorParameterProvider parameterProvider,
            DataFlowOperatorFactory dataFlowOperatorFactory,
            object dataflowInstanceUserObject)
        {
            Container = container;
            DataFlowName = dataFlowName;
            OperatorName = operatorName;
            OperatorNumber = operatorNumber;
            AgentInstanceContext = agentInstanceContext;
            AdditionalParameters = additionalParameters;
            DataFlowInstanceId = dataFlowInstanceId;
            ParameterProvider = parameterProvider;
            DataFlowOperatorFactory = dataFlowOperatorFactory;
            DataflowInstanceUserObject = dataflowInstanceUserObject;
        }

        public IContainer Container { get; }
        public AgentInstanceContext AgentInstanceContext { get; }

        public IDictionary<string, object> AdditionalParameters { get; }

        public string DataFlowInstanceId { get; }

        public EPDataFlowOperatorParameterProvider ParameterProvider { get; }

        public string OperatorName { get; }

        public int OperatorNumber { get; }

        public string DataFlowName { get; }

        public DataFlowOperatorFactory DataFlowOperatorFactory { get; }

        public object DataflowInstanceUserObject { get; }
    }
} // end of namespace