///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.runtime.@internal.dataflow.op.filter
{
    public class FilterFactory : DataFlowOperatorFactory
    {
        public ExprEvaluator Filter { get; set; }

        public bool IsSingleOutputPort { get; set; }

        public EventType EventType { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            return new FilterOp(this, context.AgentInstanceContext);
        }
    }
} // end of namespace