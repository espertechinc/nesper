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
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [Serializable]
    public class QueryGraphValueEntryRangeRelOpForge : QueryGraphValueEntryRangeForge
    {
        public QueryGraphValueEntryRangeRelOpForge(
            QueryGraphRangeEnum type,
            ExprNode expression,
            bool isBetweenPart)
            : base(type)
        {
            if (type.IsRange) {
                throw new ArgumentException("Invalid ctor for use with ranges");
            }

            Expression = expression;
            IsBetweenPart = isBetweenPart;
        }

        public ExprNode Expression { get; }

        public bool IsBetweenPart { get; }

        public override ExprNode[] Expressions => new[] {Expression};

        protected override Type ResultType => Expression.Forge.EvaluationType;

        public override string ToQueryPlan()
        {
            return Type.StringOp + " on " + ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(Expression);
        }

        public override CodegenExpression Make(
            Type optCoercionType,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryRange), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprEvaluator), "expression",
                    ExprNodeUtilityCodegen.CodegenEvaluatorWCoerce(
                        Expression.Forge, optCoercionType, method, GetType(), classScope))
                .MethodReturn(
                    NewInstance<QueryGraphValueEntryRangeRelOp>(
                        EnumValue(typeof(QueryGraphRangeEnum), type.GetName()),
                        Ref("expression"), Constant(IsBetweenPart)));
            return LocalMethod(method);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryRangeRelOp), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprEvaluator), "expression",
                    ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(Expression.Forge, method, GetType(), classScope))
                .MethodReturn(
                    NewInstance<QueryGraphValueEntryRangeRelOp>(
                        EnumValue(typeof(QueryGraphRangeEnum), type.GetName()),
                        Ref("expression"), Constant(IsBetweenPart)));
            return LocalMethod(method);
        }
    }
} // end of namespace