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
using com.espertech.esper.common.@internal.fabric;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregatorAccessPlugin : AggregatorAccessWFilterBase
    {
        private AggregationMultiFunctionStateModeManaged mode;
        private CodegenExpressionMember state;

        public AggregatorAccessPlugin(
            ExprNode optionalFilter,
            AggregationMultiFunctionStateModeManaged mode)
            : base(optionalFilter)
        {
            this.mode = mode;
        }

        public override void InitAccessForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            state = membersColumnized.AddMember(col, typeof(AggregationMultiFunctionState), "state");

            var injectionStrategy = (InjectionStrategyClassNewInstance)mode.InjectionStrategyAggregationStateFactory;
            var factoryField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationMultiFunctionStateFactory),
                injectionStrategy.GetInitializationExpression(classScope));
            rowCtor.Block.AssignRef(state, ExprDotMethod(factoryField, "NewState", ConstantNull()));
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.ExprDotMethod(
                state,
                "ApplyEnter",
                symbols.GetAddEps(method),
                symbols.GetAddExprEvalCtx(method));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.ExprDotMethod(
                state,
                "ApplyLeave",
                symbols.GetAddEps(method),
                symbols.GetAddExprEvalCtx(method));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(state, "Clear");
        }

        public override void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (mode.HasHA) {
                method.Block.Expression(StaticMethod(mode.Serde, "Write", output, writer, RowDotMember(row, state)));
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
                method.Block.AssignRef(RowDotMember(row, state), StaticMethod(mode.Serde, "Read", input));
            }
        }

        public override void CollectFabricType(FabricTypeCollector collector)
        {
            if (mode.HasHA) {
                collector.PlugInAggregation(mode.Serde);
            }
        }

        public static CodegenExpression CodegenGetAccessTableState(int column)
        {
            return MemberCol("state", column);
        }
    }
} // end of namespace