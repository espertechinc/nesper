///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public class SubordInKeywordSingleTableLookupStrategyFactoryForge : SubordTableLookupStrategyFactoryForge
    {
        private readonly IList<ExprNode> exprNodes;
        private readonly bool _isNwOnTrigger;
        private readonly int _streamCountOuter;

        public SubordInKeywordSingleTableLookupStrategyFactoryForge(
            bool isNWOnTrigger,
            int streamCountOuter,
            IList<ExprNode> exprNodes)
        {
            this._streamCountOuter = streamCountOuter;
            this._isNwOnTrigger = isNWOnTrigger;
            this.exprNodes = exprNodes;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(SubordInKeywordSingleTableLookupStrategyFactory), GetType(), classScope);

            var expressions = new string[exprNodes.Count];
            method.Block.DeclareVar(
                typeof(ExprEvaluator[]), "evals", NewArrayByLength(typeof(ExprEvaluator), Constant(exprNodes.Count)));
            for (var i = 0; i < exprNodes.Count; i++) {
                CodegenExpression eval = ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                    exprNodes[i].Forge, method, GetType(), classScope);
                method.Block.AssignArrayElement(Ref("evals"), Constant(i), eval);
                expressions[i] = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNodes[i]);
            }

            method.Block.MethodReturn(
                NewInstance<SubordInKeywordSingleTableLookupStrategyFactory>(
                    Constant(_isNwOnTrigger), Constant(_streamCountOuter), Ref("evals"), Constant(expressions)));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace