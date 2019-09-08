///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class AggregationTableAccessAggReaderForgePlugIn : AggregationTableAccessAggReaderForge
    {
        private readonly Type resultType;
        private readonly AggregationMultiFunctionTableReaderModeManaged mode;

        public AggregationTableAccessAggReaderForgePlugIn(
            Type resultType,
            AggregationMultiFunctionTableReaderModeManaged mode)
        {
            this.resultType = resultType;
            this.mode = mode;
        }

        public Type ResultType {
            get => resultType;
        }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            InjectionStrategyClassNewInstance injectionStrategy =
                (InjectionStrategyClassNewInstance) mode.InjectionStrategyTableReaderFactory;
            CodegenExpressionField factoryField = classScope.AddFieldUnshared(
                true,
                typeof(AggregationMultiFunctionTableReaderFactory),
                injectionStrategy.GetInitializationExpression(classScope));
            return ExprDotMethod(factoryField, "NewReader", ConstantNull());
        }
    }
} // end of namespace