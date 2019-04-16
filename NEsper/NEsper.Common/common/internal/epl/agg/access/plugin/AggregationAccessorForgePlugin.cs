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
using com.espertech.esper.common.@internal.epl.agg.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationAccessorForgePlugin : AggregationAccessorForge
    {
        private readonly AggregationForgeFactoryAccessPlugin parent;
        private readonly AggregationMultiFunctionAccessorModeManaged mode;
        private CodegenExpressionField accessorField;

        public AggregationAccessorForgePlugin(
            AggregationForgeFactoryAccessPlugin parent,
            AggregationMultiFunctionAccessorModeManaged mode)
        {
            this.parent = parent;
            this.mode = mode;
        }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            MakeBlock("getValue", context.Column, context.Method, context.ClassScope);
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            MakeBlock("getEnumerableEvents", context.Column, context.Method, context.ClassScope);
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            MakeBlock("getEnumerableEvent", context.Column, context.Method, context.ClassScope);
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            MakeBlock("getEnumerableScalar", context.Column, context.Method, context.ClassScope);
        }

        private void MakeBlock(
            string getterMethod,
            int column,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (accessorField == null) {
                InjectionStrategyClassNewInstance injectionStrategy =
                    (InjectionStrategyClassNewInstance) mode.InjectionStrategyAggregationAccessorFactory;
                accessorField = classScope.AddFieldUnshared(
                    true, typeof(AggregationMultiFunctionAccessor),
                    ExprDotMethod(injectionStrategy.GetInitializationExpression(classScope), "newAccessor", ConstantNull()));
            }

            method.Block.MethodReturn(
                ExprDotMethod(accessorField, getterMethod, RefCol("state", column), REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
        }
    }
} // end of namespace