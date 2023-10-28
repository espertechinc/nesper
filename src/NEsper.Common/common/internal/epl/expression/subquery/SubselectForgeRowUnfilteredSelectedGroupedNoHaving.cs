///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
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
    /// Represents a subselect in an expression tree.
    /// </summary>
    public class SubselectForgeRowUnfilteredSelectedGroupedNoHaving : SubselectForgeStrategyRowPlain
    {
        public SubselectForgeRowUnfilteredSelectedGroupedNoHaving(ExprSubselectRowNode subselect) : base(subselect)
        {
        }

        public override CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (Subselect.EvaluationType == null) {
                return ConstantNull();
            }

            var aggService = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(Subselect.SubselectNumber),
                typeof(AggregationResultFuture));
            var method = parent.MakeChild(Subselect.EvaluationType, GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);

            method.Block
                .DeclareVar<int>("cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar(
                    typeof(ICollection<object>),
                    "groupKeys",
                    ExprDotMethod(aggService, "GetGroupKeys", evalCtx))
                .IfCondition(Not(EqualsIdentity(ExprDotName(Ref("groupKeys"), "Count"), Constant(1))))
                .BlockReturn(ConstantNull())
                .ExprDotMethod(
                    aggService,
                    "SetCurrentAccess",
                    ExprDotMethodChain(Ref("groupKeys")).Add("First"),
                    Ref("cpid"),
                    ConstantNull())
                .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                .AssignArrayElement(
                    REF_EVENTS_SHIFTED,
                    Constant(0),
                    StaticMethod(
                        typeof(EventBeanUtility),
                        "GetNonemptyFirstEvent",
                        symbols.GetAddMatchingEvents(method)));

            if (Subselect.selectClause.Length == 1) {
                var eval = CodegenLegoMethodExpression.CodegenExpression(
                    Subselect.selectClause[0].Forge,
                    method,
                    classScope);
                method.Block.MethodReturn(
                    LocalMethod(eval, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method)));
            }
            else {
                var methodSelect = ExprNodeUtilityCodegen.CodegenMapSelect(
                    Subselect.selectClause,
                    Subselect.selectAsNames,
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
            var aggService = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(Subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            var method = parent.MakeChild(typeof(FlexCollection), GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);
            var eventBeanSvc =
                classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var typeMember = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(Subselect.SubselectMultirowType, EPStatementInitServicesConstants.REF));

            method.Block
                .DeclareVar<int>("cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar<AggregationService>("aggregationService", ExprDotMethod(aggService, "GetContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar(typeof(ICollection<object>), "groupKeys", ExprDotMethod(aggService, "GetGroupKeys", evalCtx))
                .IfCondition(ExprDotMethod(Ref("groupKeys"), "IsEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar(typeof(ICollection<EventBean>), "events", NewInstance(typeof(ArrayDeque<EventBean>), ExprDotName(Ref("groupKeys"), "Count")))
                .ForEach<object>("groupKey", Ref("groupKeys"))
                .ExprDotMethod(aggService, "SetCurrentAccess", Ref("groupKey"), Ref("cpid"), ConstantNull())
                .DeclareVar(
                    typeof(ICollection<string, object>),
                    "row",
                    LocalMethod(
                        Subselect.EvaluateRowCodegen(method, classScope),
                        ConstantNull(),
                        ConstantTrue(),
                        symbols.GetAddExprEvalCtx(method)))
                .DeclareVar<EventBean>(
                    "@event",
                    ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", Ref("row"), typeMember))
                .ExprDotMethod(Ref("events"), "Add", Ref("@event"))
                .BlockEnd()
                .MethodReturn(Ref("events"));
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

    public interface ICollection<T, T1>
    {
    }
} // end of namespace