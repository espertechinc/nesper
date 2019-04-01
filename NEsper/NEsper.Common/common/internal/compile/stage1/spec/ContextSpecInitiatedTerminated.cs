///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.initterm;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecInitiatedTerminated : ContextSpec
    {
        public ContextSpecInitiatedTerminated(
            ContextSpecCondition startCondition, ContextSpecCondition endCondition, bool overlapping,
            ExprNode[] distinctExpressions)
        {
            StartCondition = startCondition;
            EndCondition = endCondition;
            IsOverlapping = overlapping;
            DistinctExpressions = distinctExpressions;
        }

        public ContextSpecCondition StartCondition { get; set; }

        public ContextSpecCondition EndCondition { get; set; }

        public bool IsOverlapping { get; }

        public ExprNode[] DistinctExpressions { get; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailInitiatedTerminated), GetType(), classScope);

            method.Block
                .DeclareVar(
                    typeof(ContextControllerDetailInitiatedTerminated), "detail",
                    NewInstance(typeof(ContextControllerDetailInitiatedTerminated)))
                .ExprDotMethod(Ref("detail"), "setStartCondition", StartCondition.Make(method, symbols, classScope))
                .ExprDotMethod(Ref("detail"), "setEndCondition", EndCondition.Make(method, symbols, classScope))
                .ExprDotMethod(Ref("detail"), "setOverlapping", Constant(IsOverlapping));
            if (DistinctExpressions != null && DistinctExpressions.Length > 0) {
                method.Block
                    .ExprDotMethod(
                        Ref("detail"), "setDistinctEval",
                        ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                            ExprNodeUtilityQuery.GetForges(DistinctExpressions), null, method, GetType(), classScope))
                    .ExprDotMethod(
                        Ref("detail"), "setDistinctTypes",
                        Constant(ExprNodeUtilityQuery.GetExprResultTypes(DistinctExpressions)));
            }

            method.Block.MethodReturn(Ref("detail"));
            return LocalMethod(method);
        }
    }
} // end of namespace