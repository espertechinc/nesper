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
        private readonly AggregationStateCountMinSketchForge forge;
        private readonly CodegenExpressionRef state;
        private readonly CodegenExpressionInstanceField spec;

        public AggregatorAccessCountMinSketch(
            AggregationStateCountMinSketchForge forge,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            this.forge = forge;
            state = membersColumnized.AddMember(col, typeof(CountMinSketchAggState), "state");
            spec = classScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(CountMinSketchSpec),
                forge.specification.CodegenMake(classScope.NamespaceScope.InitMethod, classScope));
            rowCtor.Block.AssignRef(state, ExprDotMethod(spec, "MakeAggState"));
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
            method.Block.MethodThrowUnsupported();
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
                    RowDotRef(row, state)));
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
                RowDotRef(row, state),
                StaticMethod(typeof(AggregationStateSerdeCountMinSketch), "ReadCountMinSketch", input, spec));
        }

        public static CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return RefCol("state", column);
        }
    }
} // end of namespace