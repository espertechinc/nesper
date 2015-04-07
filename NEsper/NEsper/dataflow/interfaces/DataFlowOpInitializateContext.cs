///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.dataflow.interfaces
{
    public class DataFlowOpInitializateContext
    {
        public DataFlowOpInitializateContext(String dataflowName, String dataflowInstanceId, Object dataflowInstanceUserObject, IDictionary<int, DataFlowOpInputPort> inputPorts, IDictionary<int, DataFlowOpOutputPort> outputPorts, StatementContext statementContext, EPServicesContext servicesContext, AgentInstanceContext agentInstanceContext, EPRuntimeEventSender runtimeEventSender, EPServiceProvider engine, Attribute[] operatorAnnotations)
        {
            DataflowName = dataflowName;
            DataflowInstanceId = dataflowInstanceId;
            DataflowInstanceUserObject = dataflowInstanceUserObject;
            InputPorts = inputPorts;
            OutputPorts = outputPorts;
            StatementContext = statementContext;
            ServicesContext = servicesContext;
            AgentInstanceContext = agentInstanceContext;
            RuntimeEventSender = runtimeEventSender;
            Engine = engine;
            OperatorAnnotations = operatorAnnotations;
        }

        public string DataflowName { get; private set; }

        public string DataflowInstanceId { get; private set; }

        public object DataflowInstanceUserObject { get; private set; }

        public StatementContext StatementContext { get; private set; }

        public EPServicesContext ServicesContext { get; private set; }

        public AgentInstanceContext AgentInstanceContext { get; private set; }

        public IDictionary<int, DataFlowOpInputPort> InputPorts { get; private set; }

        public IDictionary<int, DataFlowOpOutputPort> OutputPorts { get; private set; }

        public EPRuntimeEventSender RuntimeEventSender { get; private set; }

        public EPServiceProvider Engine { get; private set; }

        public Attribute[] OperatorAnnotations { get; private set; }
    }
}
