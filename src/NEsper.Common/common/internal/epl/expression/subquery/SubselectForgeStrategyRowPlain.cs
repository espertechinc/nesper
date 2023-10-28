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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeStrategyRowPlain : SubselectForgeStrategyRowBase
    {
        public SubselectForgeStrategyRowPlain(ExprSubselectRowNode subselect) : base(subselect)
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

            var method = parent.MakeChild(Subselect.EvaluationType, GetType(), classScope);

            if (Subselect.filterExpr == null) {
                method.Block
                    .IfCondition(
                        Relational(
                            ExprDotName(symbols.GetAddMatchingEvents(method), "Count"),
                            CodegenExpressionRelational.CodegenRelational.GT,
                            Constant(1)))
                    .BlockReturn(ConstantNull());
                if (Subselect.selectClause == null) {
                    method.Block.MethodReturn(
                        Cast(
                            Subselect.EvaluationType,
                            StaticMethod(
                                typeof(EventBeanUtility),
                                "GetNonemptyFirstEventUnderlying",
                                symbols.GetAddMatchingEvents(method))));
                    return LocalMethod(method);
                }
                else {
                    method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                        .AssignArrayElement(
                            REF_EVENTS_SHIFTED,
                            Constant(0),
                            StaticMethod(
                                typeof(EventBeanUtility),
                                "GetNonemptyFirstEvent",
                                symbols.GetAddMatchingEvents(method)));
                }
            }
            else {
                method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);

                method.Block.DeclareVar<EventBean>("filtered", ConstantNull());
                var @foreach = method.Block.ForEach(
                    typeof(EventBean),
                    "@event",
                    symbols.GetAddMatchingEvents(method));
                {
                    @foreach.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"));
                    var filter = CodegenLegoMethodExpression.CodegenExpression(
                        Subselect.filterExpr,
                        method,
                        classScope);
                    CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                        @foreach,
                        typeof(bool?),
                        LocalMethod(
                            filter,
                            REF_EVENTS_SHIFTED,
                            symbols.GetAddIsNewData(method),
                            symbols.GetAddExprEvalCtx(method)));
                    @foreach.IfCondition(NotEqualsNull(Ref("filtered")))
                        .BlockReturn(ConstantNull())
                        .AssignRef("filtered", Ref("@event"));
                }

                if (Subselect.selectClause == null) {
                    method.Block.IfRefNullReturnNull("filtered")
                        .MethodReturn(Cast(Subselect.EvaluationType, ExprDotUnderlying(Ref("filtered"))));
                    return LocalMethod(method);
                }

                method.Block.IfRefNullReturnNull("filtered")
                    .AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("filtered"));
            }

            var selectClause = GetSelectClauseExpr(method, symbols, classScope);
            method.Block.MethodReturn(selectClause);
            return LocalMethod(method);
        }

        public override CodegenExpression EvaluateGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (Subselect.filterExpr == null) {
                if (Subselect.selectClause == null) {
                    return symbols.GetAddMatchingEvents(parent);
                }
                else {
                    if (Subselect.SubselectMultirowType == null) {
                        var eval = ((ExprIdentNode)Subselect.selectClause[0]).ExprEvaluatorIdent;
                        var method = parent.MakeChild(
                            typeof(FlexCollection),
                            GetType(),
                            classScope);
                        method.Block.DeclareVar<ICollection<EventBean>>(
                            "events",
                            NewInstance<ArrayDeque<EventBean>>(
                                ExprDotName(symbols.GetAddMatchingEvents(method), "Count")));
                        var @foreach = method.Block.ForEach(
                            typeof(EventBean),
                            "@event",
                            symbols.GetAddMatchingEvents(method));
                        {
                            @foreach.DeclareVar<object>(
                                    "fragment",
                                    eval.Getter.EventBeanFragmentCodegen(Ref("@event"), method, classScope))
                                .IfRefNull("fragment")
                                .BlockContinue()
                                .ExprDotMethod(Ref("events"), "Add", Ref("fragment"));
                        }
                        method.Block.MethodReturn(Ref("events"));
                        return LocalMethod(method);
                    }

                    // when selecting a combined output row that contains multiple fields
                    var methodX = parent.MakeChild(
                        typeof(FlexCollection),
                        GetType(),
                        classScope);
                    var fieldEventType = classScope.AddDefaultFieldUnshared(
                        true,
                        typeof(EventType),
                        EventTypeUtility.ResolveTypeCodegen(
                            Subselect.SubselectMultirowType,
                            EPStatementInitServicesConstants.REF));
                    var eventBeanSvc =
                        classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);

                    methodX.Block
                        .DeclareVar<ICollection<EventBean>>(
                            "result",
                            NewInstance<ArrayDeque<EventBean>>(
                                ExprDotName(symbols.GetAddMatchingEvents(methodX), "Count")))
                        .ApplyTri(DECLARE_EVENTS_SHIFTED, methodX, symbols);
                    var foreachX = methodX.Block.ForEach(
                        typeof(EventBean),
                        "@event",
                        symbols.GetAddMatchingEvents(methodX));
                    {
                        foreachX.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"))
                            .DeclareVar<IDictionary<string, object>>(
                                "row",
                                LocalMethod(
                                    Subselect.EvaluateRowCodegen(methodX, classScope),
                                    REF_EVENTS_SHIFTED,
                                    ConstantTrue(),
                                    symbols.GetAddExprEvalCtx(methodX)))
                            .DeclareVar<EventBean>(
                                "rowEvent",
                                ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", Ref("row"), fieldEventType))
                            .ExprDotMethod(Ref("result"), "Add", Ref("rowEvent"));
                    }
                    methodX.Block.MethodReturn(Ref("result"));
                    return LocalMethod(methodX);
                }
            }

            if (Subselect.selectClause != null) {
                return ConstantNull();
            }

            // handle filtered
            var methodY = parent.MakeChild(typeof(FlexCollection), GetType(), classScope);

            methodY.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, methodY, symbols);

            methodY.Block.DeclareVar<ArrayDeque<EventBean>>("filtered", ConstantNull());
            var foreachY = methodY.Block.ForEach(
                typeof(EventBean),
                "@event",
                symbols.GetAddMatchingEvents(methodY));
            {
                foreachY.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"));
                var filter = CodegenLegoMethodExpression.CodegenExpression(
                    Subselect.filterExpr,
                    methodY,
                    classScope);
                CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                    foreachY,
                    typeof(bool?),
                    LocalMethod(
                        filter,
                        REF_EVENTS_SHIFTED,
                        symbols.GetAddIsNewData(methodY),
                        symbols.GetAddExprEvalCtx(methodY)));
                foreachY.IfCondition(EqualsNull(Ref("filtered")))
                    .AssignRef("filtered", NewInstance(typeof(ArrayDeque<EventBean>)))
                    .BlockEnd()
                    .ExprDotMethod(Ref("filtered"), "Add", Ref("@event"));
            }

            methodY.Block.MethodReturn(FlexWrap(Ref("filtered")));
            return LocalMethod(methodY);
        }

        public override CodegenExpression EvaluateGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (Subselect.filterExpr == null) {
                if (Subselect.selectClause == null) {
                    return ConstantNull();
                }
                else {
                    var method = parent.MakeChild(typeof(FlexCollection), GetType(), classScope);
                    method.Block
                        .DeclareVar<IList<object>>("result", NewInstance(typeof(List<object>)))
                        .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);
                    var selectClause = GetSelectClauseExpr(method, symbols, classScope);
                    var @foreach = method.Block.ForEach(
                        typeof(EventBean),
                        "@event",
                        symbols.GetAddMatchingEvents(method));
                    {
                        @foreach.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"))
                            .DeclareVar<object>("value", selectClause)
                            .ExprDotMethod(Ref("result"), "Add", Ref("value"));
                    }
                    method.Block.MethodReturn(FlexWrap(Ref("result")));
                    return LocalMethod(method);
                }
            }

            if (Subselect.SelectClause == null) {
                return ConstantNull();
            }

            var methodX = parent.MakeChild(typeof(FlexCollection), GetType(), classScope);
            methodX.Block
                .DeclareVar<IList<object>>("result", NewInstance(typeof(List<object>)))
                .ApplyTri(DECLARE_EVENTS_SHIFTED, methodX, symbols);
            var selectClauseX = GetSelectClauseExpr(methodX, symbols, classScope);
            var filter = CodegenLegoMethodExpression.CodegenExpression(Subselect.FilterExpr, methodX, classScope);
            var foreachX = methodX.Block.ForEach(
                typeof(EventBean),
                "@event",
                symbols.GetAddMatchingEvents(methodX));
            {
                foreachX.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"));
                CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                    foreachX,
                    typeof(bool?),
                    LocalMethod(
                        filter,
                        REF_EVENTS_SHIFTED,
                        symbols.GetAddIsNewData(methodX),
                        symbols.GetAddExprEvalCtx(methodX)));
                foreachX.DeclareVar<object>("value", selectClauseX)
                    .ExprDotMethod(Ref("result"), "Add", Ref("value"));
            }
            methodX.Block.MethodReturn(FlexWrap(Ref("result")));
            return LocalMethod(methodX);
        }

        public override CodegenExpression EvaluateGetBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(EventBean), GetType(), classScope);

            if (Subselect.SelectClause == null) {
                if (Subselect.FilterExpr == null) {
                    method.Block
                        .IfCondition(
                            Relational(
                                ExprDotName(
                                    symbols.GetAddMatchingEvents(method),
                                    "Count"),
                                CodegenExpressionRelational.CodegenRelational.GT,
                                Constant(1)))
                        .BlockReturn(ConstantNull())
                        .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                        .MethodReturn(
                            StaticMethod(
                                typeof(EventBeanUtility),
                                "GetNonemptyFirstEvent",
                                symbols.GetAddMatchingEvents(method)));
                    return LocalMethod(method);
                }

                CodegenExpression filterX = ExprNodeUtilityCodegen.CodegenEvaluator(
                    Subselect.FilterExpr,
                    method,
                    GetType(),
                    classScope);
                method.Block
                    .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                    .DeclareVar<EventBean>(
                        "subSelectResult",
                        StaticMethod(
                            typeof(EventBeanUtility),
                            "EvaluateFilterExpectSingleMatch",
                            REF_EVENTS_SHIFTED,
                            symbols.GetAddIsNewData(method),
                            symbols.GetAddMatchingEvents(method),
                            symbols.GetAddExprEvalCtx(method),
                            filterX))
                    .MethodReturn(Ref("subSelectResult"));
                return LocalMethod(method);
            }

            var eventBeanSvc =
                classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var typeMember = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    Subselect.SubselectMultirowType,
                    EPStatementInitServicesConstants.REF));

            if (Subselect.FilterExpr == null) {
                method.Block
                    .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                    .AssignArrayElement(
                        REF_EVENTS_SHIFTED,
                        Constant(0),
                        StaticMethod(
                            typeof(EventBeanUtility),
                            "GetNonemptyFirstEvent",
                            symbols.GetAddMatchingEvents(method)))
                    .DeclareVar<IDictionary<string, object>>(
                        "row",
                        LocalMethod(
                            Subselect.EvaluateRowCodegen(method, classScope),
                            REF_EVENTS_SHIFTED,
                            ConstantTrue(),
                            symbols.GetAddExprEvalCtx(method)))
                    .DeclareVar<EventBean>(
                        "bean",
                        ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", Ref("row"), typeMember))
                    .MethodReturn(Ref("bean"));
                return LocalMethod(method);
            }

            var filter = ExprNodeUtilityCodegen.CodegenEvaluator(
                Subselect.FilterExpr,
                method,
                GetType(),
                classScope);
            method.Block
                .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                .DeclareVar<EventBean>(
                    "subSelectResult",
                    StaticMethod(
                        typeof(EventBeanUtility),
                        "EvaluateFilterExpectSingleMatch",
                        REF_EVENTS_SHIFTED,
                        symbols.GetAddIsNewData(method),
                        symbols.GetAddMatchingEvents(method),
                        symbols.GetAddExprEvalCtx(method),
                        filter))
                .IfRefNullReturnNull("subSelectResult")
                .DeclareVar<IDictionary<string, object>>(
                    "row",
                    LocalMethod(
                        Subselect.EvaluateRowCodegen(method, classScope),
                        REF_EVENTS_SHIFTED,
                        ConstantTrue(),
                        symbols.GetAddExprEvalCtx(method)))
                .DeclareVar<EventBean>(
                    "bean",
                    ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", Ref("row"), typeMember))
                .MethodReturn(Ref("bean"));
            return LocalMethod(method);
        }

        private CodegenExpression GetSelectClauseExpr(
            CodegenMethod method,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (Subselect.SelectClause.Length == 1) {
                var eval = CodegenLegoMethodExpression.CodegenExpression(
                    Subselect.SelectClause[0].Forge,
                    method,
                    classScope);
                return LocalMethod(eval, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method));
            }

            var methodSelect = ExprNodeUtilityCodegen.CodegenMapSelect(
                Subselect.SelectClause,
                Subselect.SelectAsNames,
                GetType(),
                method,
                classScope);
            return LocalMethod(methodSelect, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method));
        }
    }
} // end of namespace