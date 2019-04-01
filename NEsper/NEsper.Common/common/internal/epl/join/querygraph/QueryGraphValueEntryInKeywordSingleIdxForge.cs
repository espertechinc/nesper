///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [Serializable]
    public class QueryGraphValueEntryInKeywordSingleIdxForge : QueryGraphValueEntryForge
    {
        internal QueryGraphValueEntryInKeywordSingleIdxForge(ExprNode[] keyExprs)
        {
            KeyExprs = keyExprs;
        }

        public ExprNode[] KeyExprs { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryInKeywordSingleIdx), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprEvaluator[]), "expressions",
                    ExprNodeUtilityCodegen.CodegenEvaluators(KeyExprs, method, GetType(), classScope))
                .MethodReturn(NewInstance(typeof(QueryGraphValueEntryInKeywordSingleIdx), Ref("expressions")));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return "in-keyword single-indexed multiple key lookup " +
                   ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsList(KeyExprs);
        }
    }
} // end of namespace