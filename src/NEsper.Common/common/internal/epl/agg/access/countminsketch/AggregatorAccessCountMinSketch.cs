///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    /// <summary>
    ///     Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregatorAccessCountMinSketch : AggregatorAccess
    {
        private readonly AggregationStateCountMinSketchForge _forge;
        private readonly CodegenExpressionMember _state;
        private readonly CodegenExpressionInstanceField _spec;

        public AggregatorAccessCountMinSketch(
            AggregationStateCountMinSketchForge forge,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            this._forge = forge;
            _state = membersColumnized.AddMember(col, typeof(CountMinSketchAggState), "state");
            _spec = classScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(CountMinSketchSpec),
                forge.specification.CodegenMake(classScope.NamespaceScope.InitMethod, classScope));
            rowCtor.Block.AssignRef(_state, ExprDotMethod(_spec, "MakeAggState"));
        }

        public void ApplyEnterCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodThrowUnsupported();
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodThrowUnsupported();
        }

        public void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(_state, ExprDotMethod(_spec, "MakeAggState"));
        }

        public void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Expression(
                StaticMethod(
                    typeof(AggregationStateSerdeCountMinSketch),
                    "WriteCountMinSketch",
                    output,
                    RowDotMember(row, _state)));
        }

        public void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenExpressionRef unitKey,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(
                RowDotMember(row, _state),
                StaticMethod(typeof(AggregationStateSerdeCountMinSketch), "ReadCountMinSketch", input, _spec));
        }

        public static CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return MemberCol("state", column);
        }
    }
} // end of namespace