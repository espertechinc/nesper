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
        public SubselectForgeStrategyRowPlain(ExprSubselectRowNode subselect)
            : base(subselect)
        {
        }

        public override CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(subselect.EvaluationType, this.GetType(), classScope);

            if (subselect.FilterExpr == null) {
                method.Block
                    .IfCondition(
                        Relational(
                            ExprDotName(symbols.GetAddMatchingEvents(method), "Count"),
                            CodegenExpressionRelational.CodegenRelational.GT,
                            Constant(1)))
                    .BlockReturn(ConstantNull());
                if (subselect.SelectClause == null) {
                    method.Block.MethodReturn(
                        Cast(
                            subselect.EvaluationType,
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
                CodegenBlock @foreach = method.Block.ForEach(
                    typeof(EventBean),
                    "@event",
                    symbols.GetAddMatchingEvents(method));
                {
                    @foreach.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), @Ref("@event"));
                    CodegenMethod filter = CodegenLegoMethodExpression.CodegenExpression(
                        subselect.FilterExpr,
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
                    @foreach.IfCondition(NotEqualsNull(@Ref("filtered")))
                        .BlockReturn(ConstantNull())
                        .AssignRef("filtered", @Ref("@event"));
                }

                if (subselect.SelectClause == null) {
                    method.Block.IfRefNullReturnNull("filtered")
                        .MethodReturn(Cast(subselect.EvaluationType, ExprDotUnderlying(@Ref("filtered"))));
                    return LocalMethod(method);
                }

                method.Block.IfRefNullReturnNull("filtered")
                    .AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), @Ref("filtered"));
            }

            CodegenExpression selectClause = GetSelectClauseExpr(method, symbols, classScope);
            method.Block.MethodReturn(selectClause);
            return LocalMethod(method);
        }

        public override CodegenExpression EvaluateGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.FilterExpr == null) {
                if (subselect.SelectClause == null) {
                    return symbols.GetAddMatchingEvents(parent);
                }
                else {
                    if (subselect.subselectMultirowType == null) {
                        ExprIdentNodeEvaluator eval = ((ExprIdentNode) subselect.SelectClause[0]).ExprEvaluatorIdent;
                        CodegenMethod method = parent.MakeChild(
                            typeof(ICollection<object>),
                            this.GetType(),
                            classScope);
                        method.Block.DeclareVar<ICollection<object>>(
                            "events",
                            NewInstance<ArrayDeque<object>>(
                                ExprDotName(symbols.GetAddMatchingEvents(method), "Count")));
                        CodegenBlock @foreach = method.Block.ForEach(
                            typeof(EventBean),
                            "@event",
                            symbols.GetAddMatchingEvents(method));
                        {
                            @foreach.DeclareVar<object>(
                                    "fragment",
                                    eval.Getter.EventBeanFragmentCodegen(@Ref("@event"), method, classScope))
                                .IfRefNull("fragment")
                                .BlockContinue()
                                .ExprDotMethod(@Ref("events"), "Add", @Ref("fragment"));
                        }
                        method.Block.MethodReturn(@Ref("events"));
                        return LocalMethod(method);
                    }

                    // when selecting a combined output row that contains multiple fields
                    CodegenMethod methodX = parent.MakeChild(typeof(ICollection<object>), this.GetType(), classScope);
                    CodegenExpressionField fieldEventType = classScope.AddFieldUnshared(
                        true,
                        typeof(EventType),
                        EventTypeUtility.ResolveTypeCodegen(
                            subselect.subselectMultirowType,
                            EPStatementInitServicesConstants.REF));
                    CodegenExpressionField eventBeanSvc =
                        classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);

                    methodX.Block
                        .DeclareVar<ICollection<object>>(
                            "result",
                            NewInstance<ArrayDeque<object>>(
                                ExprDotName(symbols.GetAddMatchingEvents(methodX), "Count")))
                        .ApplyTri(DECLARE_EVENTS_SHIFTED, methodX, symbols);
                    CodegenBlock foreachX = methodX.Block.ForEach(
                        typeof(EventBean),
                        "@event",
                        symbols.GetAddMatchingEvents(methodX));
                    {
                        foreachX.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), @Ref("@event"))
                            .DeclareVar<IDictionary<object, object>>(
                                "row",
                                LocalMethod(
                                    subselect.EvaluateRowCodegen(methodX, classScope),
                                    REF_EVENTS_SHIFTED,
                                    ConstantTrue(),
                                    symbols.GetAddExprEvalCtx(methodX)))
                            .DeclareVar<EventBean>(
                                "rowEvent",
                                ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", @Ref("row"), fieldEventType))
                            .ExprDotMethod(@Ref("result"), "Add", @Ref("rowEvent"));
                    }
                    methodX.Block.MethodReturn(@Ref("result"));
                    return LocalMethod(methodX);
                }
            }

            if (subselect.SelectClause != null) {
                return ConstantNull();
            }

            // handle filtered
            CodegenMethod methodY = parent.MakeChild(typeof(ICollection<object>), this.GetType(), classScope);

            methodY.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, methodY, symbols);

            methodY.Block.DeclareVar<ArrayDeque<object>>("filtered", ConstantNull());
            CodegenBlock foreachY = methodY.Block.ForEach(
                typeof(EventBean),
                "@event",
                symbols.GetAddMatchingEvents(methodY));
            {
                foreachY.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), @Ref("@event"));
                CodegenMethod filter = CodegenLegoMethodExpression.CodegenExpression(
                    subselect.FilterExpr,
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
                foreachY.IfCondition(EqualsNull(@Ref("filtered")))
                    .AssignRef("filtered", NewInstance(typeof(ArrayDeque<object>)))
                    .BlockEnd()
                    .ExprDotMethod(@Ref("filtered"), "Add", @Ref("@event"));
            }

            methodY.Block.MethodReturn(@Ref("filtered"));
            return LocalMethod(methodY);
        }

        public override CodegenExpression EvaluateGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.FilterExpr == null) {
                if (subselect.SelectClause == null) {
                    return ConstantNull();
                }
                else {
                    CodegenMethod method = parent.MakeChild(typeof(ICollection<object>), this.GetType(), classScope);
                    method.Block
                        .DeclareVar<IList<object>>("result", NewInstance(typeof(List<object>)))
                        .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);
                    CodegenExpression selectClause = GetSelectClauseExpr(method, symbols, classScope);
                    CodegenBlock @foreach = method.Block.ForEach(
                        typeof(EventBean),
                        "@event",
                        symbols.GetAddMatchingEvents(method));
                    {
                        @foreach.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), @Ref("@event"))
                            .DeclareVar<object>("value", selectClause)
                            .ExprDotMethod(@Ref("result"), "Add", @Ref("value"));
                    }
                    method.Block.MethodReturn(@Ref("result"));
                    return LocalMethod(method);
                }
            }

            if (subselect.SelectClause == null) {
                return ConstantNull();
            }

            CodegenMethod methodX = parent.MakeChild(typeof(ICollection<object>), this.GetType(), classScope);
            methodX.Block
                .DeclareVar<IList<object>>("result", NewInstance(typeof(List<object>)))
                .ApplyTri(DECLARE_EVENTS_SHIFTED, methodX, symbols);
            CodegenExpression selectClauseX = GetSelectClauseExpr(methodX, symbols, classScope);
            CodegenMethod filter =
                CodegenLegoMethodExpression.CodegenExpression(subselect.FilterExpr, methodX, classScope);
            CodegenBlock foreachX = methodX.Block.ForEach(
                typeof(EventBean),
                "@event",
                symbols.GetAddMatchingEvents(methodX));
            {
                foreachX.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), @Ref("@event"));
                CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                    foreachX,
                    typeof(bool?),
                    LocalMethod(
                        filter,
                        REF_EVENTS_SHIFTED,
                        symbols.GetAddIsNewData(methodX),
                        symbols.GetAddExprEvalCtx(methodX)));
                foreachX.DeclareVar<object>("value", selectClauseX)
                    .ExprDotMethod(@Ref("result"), "Add", @Ref("value"));
            }
            methodX.Block.MethodReturn(@Ref("result"));
            return LocalMethod(methodX);
        }

        public override CodegenExpression EvaluateGetBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.SelectClause == null) {
                return ConstantNull();
            }

            CodegenMethod method = parent.MakeChild(typeof(EventBean), this.GetType(), classScope);
            CodegenExpressionField eventBeanSvc =
                classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            CodegenExpressionField typeMember = classScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    subselect.subselectMultirowType,
                    EPStatementInitServicesConstants.REF));

            if (subselect.FilterExpr == null) {
                method.Block
                    .ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols)
                    .AssignArrayElement(
                        REF_EVENTS_SHIFTED,
                        Constant(0),
                        StaticMethod(
                            typeof(EventBeanUtility),
                            "GetNonemptyFirstEvent",
                            symbols.GetAddMatchingEvents(method)))
                    .DeclareVar<IDictionary<object, object>>(
                        "row",
                        LocalMethod(
                            subselect.EvaluateRowCodegen(method, classScope),
                            REF_EVENTS_SHIFTED,
                            ConstantTrue(),
                            symbols.GetAddExprEvalCtx(method)))
                    .DeclareVar<EventBean>(
                        "bean",
                        ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", @Ref("row"), typeMember))
                    .MethodReturn(@Ref("bean"));
                return LocalMethod(method);
            }

            CodegenExpression filter = ExprNodeUtilityCodegen.CodegenEvaluator(
                subselect.FilterExpr,
                method,
                this.GetType(),
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
                .IfRefNullReturnNull("subselectResult")
                .DeclareVar<IDictionary<object, object>>(
                    "row",
                    LocalMethod(
                        subselect.EvaluateRowCodegen(method, classScope),
                        REF_EVENTS_SHIFTED,
                        ConstantTrue(),
                        symbols.GetAddExprEvalCtx(method)))
                .DeclareVar<EventBean>(
                    "bean",
                    ExprDotMethod(eventBeanSvc, "AdapterForTypedMap", @Ref("row"), typeMember))
                .MethodReturn(@Ref("bean"));
            return LocalMethod(method);
        }

        private CodegenExpression GetSelectClauseExpr(
            CodegenMethod method,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.SelectClause.Length == 1) {
                CodegenMethod eval = CodegenLegoMethodExpression.CodegenExpression(
                    subselect.SelectClause[0].Forge,
                    method,
                    classScope);
                return LocalMethod(eval, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method));
            }

            CodegenMethod methodSelect = ExprNodeUtilityCodegen.CodegenMapSelect(
                subselect.SelectClause,
                subselect.SelectAsNames,
                this.GetType(),
                method,
                classScope);
            return LocalMethod(methodSelect, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method));
        }
    }
} // end of namespace