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
    public class SupportReferenceCountedMapRCMFunctionHandler : AggregationMultiFunctionHandler
    {
        private readonly ExprNode[] parameterExpressions;

        public SupportReferenceCountedMapRCMFunctionHandler(
            AggregationMultiFunctionStateKey sharedStateKey,
            ExprNode[] parameterExpressions)
        {
            AggregationStateUniqueKey = sharedStateKey;
            this.parameterExpressions = parameterExpressions;
        }

        public EPChainableType ReturnType => EPChainableTypeHelper.NullValue();

        public AggregationMultiFunctionStateKey AggregationStateUniqueKey { get; }

        public AggregationMultiFunctionStateMode StateMode =>
            new AggregationMultiFunctionStateModeManaged().WithInjectionStrategyAggregationStateFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportReferenceCountedMapStateFactory)));

        public AggregationMultiFunctionAccessorMode AccessorMode =>
            new AggregationMultiFunctionAccessorModeManaged().WithInjectionStrategyAggregationAccessorFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportReferenceCountedMapAccessorFactory)));

        public AggregationMultiFunctionAgentMode AgentMode =>
            new AggregationMultiFunctionAgentModeManaged().SetInjectionStrategyAggregationAgentFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportReferenceCountedMapAgentFactory)).AddExpression(
                    "eval",
                    parameterExpressions[0]));

        public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx)
        {
            throw new UnsupportedOperationException(
                "This aggregation function is not designed for use with table columns");
        }
    }
} // end of namespace