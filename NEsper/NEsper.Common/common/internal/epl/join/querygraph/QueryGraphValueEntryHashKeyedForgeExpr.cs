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
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueEntryHashKeyedForgeExpr : QueryGraphValueEntryHashKeyedForge
    {
        public QueryGraphValueEntryHashKeyedForgeExpr(ExprNode keyExpr, bool requiresKey) : base(keyExpr)
        {
            IsRequiresKey = requiresKey;
        }

        public bool IsRequiresKey { get; }

        public bool IsConstant => ExprNodeUtilityQuery.IsConstant(KeyExpr);

        public override string ToQueryPlan()
        {
            return ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(KeyExpr);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbol, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryHashKeyedExpr), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprEvaluator), "expression",
                    ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(KeyExpr.Forge, method, GetType(), classScope))
                .MethodReturn(
                    NewInstance(
                        typeof(QueryGraphValueEntryHashKeyedExpr),
                        Ref("expression"), Constant(IsRequiresKey)));
            return LocalMethod(method);
        }
    }
} // end of namespace