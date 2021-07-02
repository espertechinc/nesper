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
        private readonly AggregationMultiFunctionStateModeManaged _mode;

        private readonly CodegenExpressionMember _state;

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
            _state = membersColumnized.AddMember(col, typeof(AggregationMultiFunctionState), "state");
            this._mode = mode;

            var injectionStrategy = (InjectionStrategyClassNewInstance) mode.InjectionStrategyAggregationStateFactory;
            var factoryField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationMultiFunctionStateFactory),
                injectionStrategy.GetInitializationExpression(classScope));
            ctor.Block.AssignRef(_state, ExprDotMethod(factoryField, "NewState", ConstantNull()));
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.ExprDotMethod(
                _state,
                "ApplyEnter",
                symbols.GetAddEPS(method),
                symbols.GetAddExprEvalCtx(method));
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.ExprDotMethod(
                _state,
                "ApplyLeave",
                symbols.GetAddEPS(method),
                symbols.GetAddExprEvalCtx(method));
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_state, "Clear");
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
            if (_mode.HasHA) {
                method.Block.Expression(StaticMethod(_mode.Serde, "Write", output, RowDotMember(row, _state)));
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
            if (_mode.HasHA) {
                method.Block.AssignRef(RowDotMember(row, _state), StaticMethod(_mode.Serde, "Read", input));
            }
        }

        public static CodegenExpression CodegenGetAccessTableState(int column)
        {
            return MemberCol("state", column);
        }
    }
} // end of namespace