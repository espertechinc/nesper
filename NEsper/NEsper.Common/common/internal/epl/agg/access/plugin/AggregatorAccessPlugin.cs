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
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregatorAccessPlugin : AggregatorAccessWFilterBase
    {
        private readonly AggregationMultiFunctionStateModeManaged mode;

        private readonly CodegenExpressionRef state;

        public AggregatorAccessPlugin(
            int col,
            bool join,
            CodegenCtor ctor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            ExprNode optionalFilter,
            AggregationMultiFunctionStateModeManaged mode)
            : base(optionalFilter)

        {
            state = membersColumnized.AddMember(col, typeof(AggregationMultiFunctionState), "state");
            this.mode = mode;

            var injectionStrategy = (InjectionStrategyClassNewInstance) mode.InjectionStrategyAggregationStateFactory;
            var factoryField = classScope.AddFieldUnshared(
                true, typeof(AggregationMultiFunctionStateFactory),
                injectionStrategy.GetInitializationExpression(classScope));
            ctor.Block.AssignRef(state, ExprDotMethod(factoryField, "newState", ConstantNull()));
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.ExprDotMethod(
                state, "applyEnter", symbols.GetAddEPS(method), symbols.GetAddExprEvalCtx(method));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.ExprDotMethod(
                state, "applyLeave", symbols.GetAddEPS(method), symbols.GetAddExprEvalCtx(method));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(state, "clear");
        }

        public override void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef @ref,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef output,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (mode.HasHA) {
                method.Block.Expression(StaticMethod(mode.Serde, "write", output, RowDotRef(row, state)));
            }
        }

        public override void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenExpressionRef unitKey,
            CodegenClassScope classScope)
        {
            if (mode.HasHA) {
                method.Block.AssignRef(RowDotRef(row, state), StaticMethod(mode.Serde, "read", input));
            }
        }

        public static CodegenExpression CodegenGetAccessTableState(int column)
        {
            return RefCol("state", column);
        }
    }
} // end of namespace