///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationAgentForgePlugin : AggregationAgentForge
    {
        private readonly AggregationForgeFactoryAccessPlugin parent;
        private readonly AggregationMultiFunctionAgentModeManaged mode;
        private readonly ExprForge optionalFilter;

        public AggregationAgentForgePlugin(
            AggregationForgeFactoryAccessPlugin parent,
            AggregationMultiFunctionAgentModeManaged mode,
            ExprForge optionalFilter)
        {
            this.parent = parent;
            this.mode = mode;
            this.optionalFilter = optionalFilter;
        }

        public ExprForge OptionalFilter => optionalFilter;

        public CodegenExpression Make(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var injectionStrategy =
                (InjectionStrategyClassNewInstance)mode.InjectionStrategyAggregationAgentFactory;
            var factoryField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationMultiFunctionAgentFactory),
                injectionStrategy.GetInitializationExpression(classScope));
            return ExprDotMethod(factoryField, "NewAgent", ConstantNull());
        }
    }
} // end of namespace