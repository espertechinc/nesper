///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportReferenceCountedMapRCLFunctionHandler : AggregationMultiFunctionHandler
    {
        private readonly ExprNode eval;

        public SupportReferenceCountedMapRCLFunctionHandler(ExprNode eval)
        {
            this.eval = eval;
        }

        public EPType ReturnType => EPTypeHelper.SingleValue(typeof(int?));

        public AggregationMultiFunctionStateKey AggregationStateUniqueKey =>
            throw new UnsupportedOperationException("The lookup function is only for table-column-reads");

        public AggregationMultiFunctionStateMode StateMode =>
            throw new UnsupportedOperationException("The lookup function is only for table-column-reads");

        public AggregationMultiFunctionAccessorMode AccessorMode =>
            throw new UnsupportedOperationException("The lookup function is only for table-column-reads");

        public AggregationMultiFunctionAgentMode AgentMode =>
            throw new UnsupportedOperationException("The lookup function is only for table-column-reads");

        public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx)
        {
            return new AggregationMultiFunctionAggregationMethodModeManaged()
                .SetInjectionStrategyAggregationMethodFactory(
                    new InjectionStrategyClassNewInstance(typeof(SupportReferenceCountedMapAggregationMethodFactory))
                .AddExpression("eval", eval));        
        }
    }
} // end of namespace