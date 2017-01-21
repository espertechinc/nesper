///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.dataflow.core
{
    public class DataFlowStmtDesc
    {
        public DataFlowStmtDesc(CreateDataFlowDesc graphDesc,
                                StatementContext statementContext,
                                EPServicesContext servicesContext,
                                AgentInstanceContext agentInstanceContext,
                                IDictionary<GraphOperatorSpec, Attribute[]> operatorAnnotations)
        {
            GraphDesc = graphDesc;
            StatementContext = statementContext;
            ServicesContext = servicesContext;
            AgentInstanceContext = agentInstanceContext;
            OperatorAnnotations = operatorAnnotations;
        }

        public CreateDataFlowDesc GraphDesc { get; private set; }

        public StatementContext StatementContext { get; private set; }

        public EPServicesContext ServicesContext { get; private set; }

        public AgentInstanceContext AgentInstanceContext { get; private set; }

        public IDictionary<GraphOperatorSpec, Attribute[]> OperatorAnnotations { get; private set; }
    }
}