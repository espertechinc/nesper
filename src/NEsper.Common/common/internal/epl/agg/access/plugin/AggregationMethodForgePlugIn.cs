///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationMethodForgePlugIn : AggregationMethodForge
    {
        private readonly Type _resultType;
        private readonly AggregationMultiFunctionAggregationMethodModeManaged _mode;

        public AggregationMethodForgePlugIn(
            Type resultType,
            AggregationMultiFunctionAggregationMethodModeManaged mode)
        {
            _resultType = resultType;
            _mode = mode;
        }

        public Type ResultType => _resultType;

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var injectionStrategy = (InjectionStrategyClassNewInstance)_mode.InjectionStrategyAggregationMethodFactory;
            var factoryField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationMultiFunctionAggregationMethodFactory),
                injectionStrategy.GetInitializationExpression(classScope));
            return ExprDotMethod(factoryField, "NewMethod", ConstantNull());
        }
    }
} // end of namespace