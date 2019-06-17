///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.runtime.@internal.dataflow.op.epstatementsource
{
    public class EPStatementSourceFactory : DataFlowOperatorFactory
    {
        public ExprEvaluator StatementName { set; get; }

        public IDictionary<string, object> StatementFilter { set; get; }

        public IDictionary<string, object> Collector { set; get; }

        public bool SubmitEventBean {
            set => IsSubmitEventBean = value;
        }

        public bool IsSubmitEventBean { get; private set; }

        public ExprEvaluator StatementDeploymentId { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var statementDeploymentIdParam =
                DataFlowParameterResolution.ResolveStringOptional("statementDeploymentId", StatementDeploymentId, context);
            var statementNameParam = DataFlowParameterResolution.ResolveStringOptional("statementName", StatementName, context);
            var statementFilterInstance = DataFlowParameterResolution
                .ResolveOptionalInstance<EPDataFlowEPStatementFilter>("statementFilter", StatementFilter, context);
            var collectorInstance = DataFlowParameterResolution
                .ResolveOptionalInstance<EPDataFlowIRStreamCollector>("collector", Collector, context);

            if (statementNameParam == null && statementFilterInstance == null) {
                throw new EPException("Failed to find required 'statementName' or 'statementFilter' parameter");
            }

            return new EPStatementSourceOp(
                this, context.AgentInstanceContext, statementDeploymentIdParam, statementNameParam, statementFilterInstance, collectorInstance);
        }
    }
} // end of namespace