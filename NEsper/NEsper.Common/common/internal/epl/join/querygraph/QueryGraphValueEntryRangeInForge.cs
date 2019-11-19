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
    public class QueryGraphValueEntryRangeInForge : QueryGraphValueEntryRangeForge
    {
        public QueryGraphValueEntryRangeInForge(
            QueryGraphRangeEnum rangeType,
            ExprNode exprStart,
            ExprNode exprEnd,
            bool allowRangeReversal)
            : base(
                rangeType)
        {
            if (!rangeType.IsRange()) {
                throw new ArgumentException("Range type expected but received " + rangeType.GetName());
            }

            ExprStart = exprStart;
            ExprEnd = exprEnd;
            IsAllowRangeReversal = allowRangeReversal;
        }

        public bool IsAllowRangeReversal { get; }

        public ExprNode ExprStart { get; }

        public ExprNode ExprEnd { get; }

        public override ExprNode[] Expressions => new[] {ExprStart, ExprEnd};

        public override string ToQueryPlan()
        {
            return GetType().Name;
        }

        protected override Type ResultType {
            get { return ExprStart.Forge.EvaluationType; }
        }

        public override CodegenExpression Make(
            Type optCoercionType,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryRange), GetType(), classScope);
            method.Block
                .DeclareVar<ExprEvaluator>(
                    "start",
                    ExprNodeUtilityCodegen.CodegenEvaluatorWCoerce(
                        ExprStart.Forge,
                        optCoercionType,
                        method,
                        GetType(),
                        classScope))
                .DeclareVar<ExprEvaluator>(
                    "end",
                    ExprNodeUtilityCodegen.CodegenEvaluatorWCoerce(
                        ExprEnd.Forge,
                        optCoercionType,
                        method,
                        GetType(),
                        classScope))
                .MethodReturn(
                    NewInstance<QueryGraphValueEntryRangeIn>(
                        EnumValue(typeof(QueryGraphRangeEnum), type.GetName()),
                        Ref("start"),
                        Ref("end"),
                        Constant(IsAllowRangeReversal)));
            return LocalMethod(method);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(QueryGraphValueEntryRangeIn), GetType(), classScope);
            method.Block
                .DeclareVar<ExprEvaluator>(
                    "start",
                    ExprNodeUtilityCodegen.CodegenEvaluator(ExprStart.Forge, method, GetType(), classScope))
                .DeclareVar<ExprEvaluator>(
                    "end",
                    ExprNodeUtilityCodegen.CodegenEvaluator(ExprEnd.Forge, method, GetType(), classScope))
                .MethodReturn(
                    NewInstance<QueryGraphValueEntryRangeIn>(
                        EnumValue(typeof(QueryGraphRangeEnum), type.GetName()),
                        Ref("start"),
                        Ref("end"),
                        Constant(IsAllowRangeReversal)));
            return LocalMethod(method);
        }
    }
} // end of namespace