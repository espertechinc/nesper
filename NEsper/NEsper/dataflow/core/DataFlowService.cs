///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.dataflow.core
{
    public interface DataFlowService : EPDataFlowRuntime, IDisposable
    {
        void AddStartGraph(CreateDataFlowDesc desc,
                           StatementContext statementContext,
                           EPServicesContext servicesContext,
                           AgentInstanceContext agentInstanceContext,
                           bool newStatement);

        void RemoveGraph(String graphName);
        void StopGraph(String graphName);
    }
}