///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    ///     Represents a subselect in an expression tree.
    /// </summary>
    public class SubselectForgeRowUnfilteredSelectedGroupedWHaving : SubselectForgeStrategyRowPlain
    {
        public SubselectForgeRowUnfilteredSelectedGroupedWHaving(ExprSubselectRowNode subselect)
            : base(subselect)
        {
        }

        public override CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression aggService = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            var method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);

            method.Block
                .DeclareVar<int>("cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar<AggregationService>(
                    "aggregationService",
                    ExprDotMethod(aggService, "GetContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar<ICollection<object>>(
                    "groupKeys",
                    ExprDotMethod(Ref("aggregationService"), "GetGroupKeys", evalCtx))
                .IfCondition(ExprDotMethod(Ref("groupKeys"), "IsEmpty"))
                .BlockReturn(ConstantNull())
                .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                .DeclareVar<bool>("haveResult", ConstantFalse())
                .DeclareVar<object>("groupKeyMatch", ConstantNull());

            var forEach = method.Block.ForEach(typeof(object), "groupKey", Ref("groupKeys"));
            {
                var havingExpr = CodegenLegoMethodExpression.CodegenExpression(
                    subselect.HavingExpr,
                    method,
                    classScope);
                CodegenExpression havingCall = LocalMethod(
                    havingExpr,
                    REF_EVENTS_SHIFTED,
                    symbols.GetAddIsNewData(method),
                    evalCtx);

                forEach.ExprDotMethod(
                        Ref("aggregationService"),
                        "SetCurrentAccess",
                        Ref("groupKey"),
                        Ref("cpid"),
                        ConstantNull())
                    .DeclareVar<bool?>("pass", Cast(typeof(bool?), havingCall))
                    .IfCondition(And(NotEqualsNull(Ref("pass")), Ref("pass")))
                    .IfCondition(Ref("haveResult"))
                    .BlockReturn(ConstantNull())
                    .AssignRef("groupKeyMatch", Ref("groupKey"))
                    .AssignRef("haveResult", ConstantTrue());
            }

            method.Block.IfCondition(EqualsNull(Ref("groupKeyMatch")))
                .BlockReturn(ConstantNull())
                .ExprDotMethod(
                    Ref("aggregationService"),
                    "SetCurrentAccess",
                    Ref("groupKeyMatch"),
                    Ref("cpid"),
                    ConstantNull());

            if (subselect.SelectClause.Length == 1) {
                var eval = CodegenLegoMethodExpression.CodegenExpression(
                    subselect.SelectClause[0].Forge,
                    method,
                    classScope);
                method.Block.MethodReturn(
                    LocalMethod(eval, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method)));
            }
            else {
                var methodSelect = ExprNodeUtilityCodegen.CodegenMapSelect(
                    subselect.SelectClause,
                    subselect.SelectAsNames,
                    GetType(),
                    method,
                    classScope);
                CodegenExpression select = LocalMethod(
                    methodSelect,
                    REF_EVENTS_SHIFTED,
                    ConstantTrue(),
                    symbols.GetAddExprEvalCtx(method));
                method.Block.MethodReturn(select);
            }

            return LocalMethod(method);
        }

        public override CodegenExpression EvaluateGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression aggService = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber),
                typeof(AggregationResultFuture));
            var factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var subselectMultirowType = classScope.AddFieldUnshared(
                false,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    subselect.subselectMultirowType,
                    EPStatementInitServicesConstants.REF));

            var method = parent.MakeChild(typeof(ICollection<object>), GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);

            method.Block
                .DeclareVar<int>("cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar<AggregationService>(
                    "aggregationService",
                    ExprDotMethod(aggService, "GetContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar<ICollection<object>>(
                    "groupKeys",
                    ExprDotMethod(Ref("aggregationService"), "GetGroupKeys", evalCtx))
                .IfCondition(ExprDotMethod(Ref("groupKeys"), "IsEmpty"))
                .BlockReturn(ConstantNull())
                .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                .DeclareVar<ICollection<object>>(
                    "result",
                    NewInstance<ArrayDeque<object>>(ExprDotMethod(Ref("groupKeys"), "Size")));

            var forEach = method.Block.ForEach(typeof(object), "groupKey", Ref("groupKeys"));
            {
                var havingExpr = CodegenLegoMethodExpression.CodegenExpression(
                    subselect.HavingExpr,
                    method,
                    classScope);
                CodegenExpression havingCall = LocalMethod(
                    havingExpr,
                    REF_EVENTS_SHIFTED,
                    symbols.GetAddIsNewData(method),
                    evalCtx);

                forEach
                    .ExprDotMethod(
                        Ref("aggregationService"),
                        "SetCurrentAccess",
                        Ref("groupKey"),
                        Ref("cpid"),
                        ConstantNull())
                    .DeclareVar<bool?>("pass", Cast(typeof(bool?), havingCall))
                    .IfCondition(And(NotEqualsNull(Ref("pass")), Ref("pass")))
                    .DeclareVar<IDictionary<object, object>>(
                        "row",
                        LocalMethod(
                            subselect.EvaluateRowCodegen(method, classScope),
                            REF_EVENTS_SHIFTED,
                            ConstantTrue(),
                            symbols.GetAddExprEvalCtx(method)))
                    .DeclareVar<EventBean>(
                        "@event",
                        ExprDotMethod(factory, "AdapterForTypedMap", Ref("row"), subselectMultirowType))
                    .ExprDotMethod(Ref("result"), "Add", Ref("@event"));
            }
            method.Block.MethodReturn(Ref("result"));
            return LocalMethod(method);
        }

        public override CodegenExpression EvaluateGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public override CodegenExpression EvaluateGetBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace