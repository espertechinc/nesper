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

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategyFactoryForge : SubordTableLookupStrategyFactoryForge
    {
        internal readonly ExprNode exprNode;
        internal readonly bool isNWOnTrigger;
        internal readonly int streamCountOuter;

        public SubordInKeywordMultiTableLookupStrategyFactoryForge(
            bool isNWOnTrigger,
            int streamCountOuter,
            ExprNode exprNode)
        {
            this.streamCountOuter = streamCountOuter;
            this.isNWOnTrigger = isNWOnTrigger;
            this.exprNode = exprNode;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(SubordInKeywordMultiTableLookupStrategyFactory),
                GetType(),
                classScope);
            var expression = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode);
            CodegenExpression eval = ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                exprNode.Forge,
                method,
                GetType(),
                classScope);
            method.Block.MethodReturn(
                NewInstance<SubordInKeywordMultiTableLookupStrategyFactory>(
                    Constant(isNWOnTrigger),
                    Constant(streamCountOuter),
                    eval,
                    Constant(expression)));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace