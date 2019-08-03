///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public abstract class SubselectForgeStrategyRowBase : SubselectForgeRow
    {
        internal readonly ExprSubselectRowNode subselect;

        public SubselectForgeStrategyRowBase(ExprSubselectRowNode subselect)
        {
            this.subselect = subselect;
        }

        public abstract CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope);

        public abstract CodegenExpression EvaluateGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope);

        public abstract CodegenExpression EvaluateGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope);

        public abstract CodegenExpression EvaluateGetBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        public CodegenExpression EvaluateTypableSinglerowCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.SelectClause == null) {
                return ConstantNull(); // no select-clause
            }

            var method = parent.MakeChild(typeof(object[]), GetType(), classScope);
            if (subselect.FilterExpr == null) {
                method.Block
                    .ApplyTri(SubselectForgeCodegenUtil.DECLARE_EVENTS_SHIFTED, method, symbols)
                    .AssignArrayElement(
                        REF_EVENTS_SHIFTED, Constant(0),
                        StaticMethod(
                            typeof(EventBeanUtility), "getNonemptyFirstEvent", symbols.GetAddMatchingEvents(method)));
            }
            else {
                CodegenExpression filter = ExprNodeUtilityCodegen.CodegenEvaluator(
                    subselect.FilterExpr, method, GetType(), classScope);
                method.Block
                    .ApplyTri(SubselectForgeCodegenUtil.DECLARE_EVENTS_SHIFTED, method, symbols)
                    .DeclareVar<EventBean>(
"subselectResult", StaticMethod(
                            typeof(EventBeanUtility), "evaluateFilterExpectSingleMatch",
                            REF_EVENTS_SHIFTED, symbols.GetAddIsNewData(method), symbols.GetAddMatchingEvents(method),
                            symbols.GetAddExprEvalCtx(method),
                            filter))
                    .IfRefNullReturnNull("subselectResult")
                    .AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("subselectResult"));
            }

            method.Block.DeclareVar<object[]>(
"results", NewArrayByLength(typeof(object), Constant(subselect.SelectClause.Length)));
            for (var i = 0; i < subselect.SelectClause.Length; i++) {
                var eval = CodegenLegoMethodExpression.CodegenExpression(
                    subselect.SelectClause[i].Forge, method, classScope);
                method.Block.AssignArrayElement(
                    "results", Constant(i),
                    LocalMethod(
                        eval, REF_EVENTS_SHIFTED, symbols.GetAddIsNewData(method), symbols.GetAddExprEvalCtx(method)));
            }

            method.Block.MethodReturn(Ref("results"));

            return LocalMethod(method);
        }

        public CodegenExpression EvaluateTypableMultirowCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.SelectClause == null) {
                return ConstantNull();
            }

            if (subselect.FilterExpr == null) {
                var method = parent.MakeChild(typeof(object[][]), GetType(), classScope);
                method.Block
                    .DeclareVar<object[][]>(
"rows",
                        NewArrayByLength(typeof(object[]), ExprDotMethod(symbols.GetAddMatchingEvents(method), "Size")))
                    .DeclareVar<int>("index", Constant(-1))
                    .ApplyTri(SubselectForgeCodegenUtil.DECLARE_EVENTS_SHIFTED, method, symbols);
                var foreachEvent = method.Block.ForEach(
                    typeof(EventBean), "@event", symbols.GetAddMatchingEvents(method));
                {
                    foreachEvent
                        .Increment("index")
                        .AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"))
                        .DeclareVar<object[]>(
"results",
                            NewArrayByLength(typeof(object), Constant(subselect.SelectClause.Length)))
                        .AssignArrayElement("rows", Ref("index"), Ref("results"));
                    for (var i = 0; i < subselect.SelectClause.Length; i++) {
                        var eval = CodegenLegoMethodExpression.CodegenExpression(
                            subselect.SelectClause[i].Forge, method, classScope);
                        foreachEvent.AssignArrayElement(
                            "results", Constant(i),
                            LocalMethod(
                                eval, REF_EVENTS_SHIFTED, symbols.GetAddIsNewData(method),
                                symbols.GetAddExprEvalCtx(method)));
                    }
                }
                method.Block.MethodReturn(Ref("rows"));
                return LocalMethod(method);
            }
            else {
                var method = parent.MakeChild(typeof(object[][]), GetType(), classScope);
                method.Block
                    .DeclareVar<ArrayDeque<object[]>>("rows", NewInstance(typeof(ArrayDeque<object[]>)))
                    .ApplyTri(SubselectForgeCodegenUtil.DECLARE_EVENTS_SHIFTED, method, symbols);
                var foreachEvent = method.Block.ForEach(
                    typeof(EventBean), "@event", symbols.GetAddMatchingEvents(method));
                {
                    foreachEvent.AssignArrayElement(REF_EVENTS_SHIFTED, Constant(0), Ref("@event"));

                    var filter = CodegenLegoMethodExpression.CodegenExpression(
                        subselect.FilterExpr, method, classScope);
                    CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                        foreachEvent, typeof(bool?),
                        LocalMethod(
                            filter, REF_EVENTS_SHIFTED, symbols.GetAddIsNewData(method),
                            symbols.GetAddExprEvalCtx(method)));

                    foreachEvent
                        .DeclareVar<object[]>(
"results",
                            NewArrayByLength(typeof(object), Constant(subselect.SelectClause.Length)))
                        .ExprDotMethod(Ref("rows"), "Add", Ref("results"));
                    for (var i = 0; i < subselect.SelectClause.Length; i++) {
                        var eval = CodegenLegoMethodExpression.CodegenExpression(
                            subselect.SelectClause[i].Forge, method, classScope);
                        foreachEvent.AssignArrayElement(
                            "results", Constant(i),
                            LocalMethod(
                                eval, REF_EVENTS_SHIFTED, symbols.GetAddIsNewData(method),
                                symbols.GetAddExprEvalCtx(method)));
                    }
                }
                method.Block
                    .IfCondition(ExprDotMethod(Ref("rows"), "IsEmpty"))
                    .BlockReturn(EnumValue(typeof(CollectionUtil), "OBJECTARRAYARRAY_EMPTY"))
                    .MethodReturn(
                        Cast(
                            typeof(object[][]),
                            ExprDotMethod(Ref("rows"), "toArray", NewArrayByLength(typeof(object[]), Constant(0)))));
                return LocalMethod(method);
            }
        }
    }
} // end of namespace