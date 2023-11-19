///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetProcessorUtil
    {
        public const string METHOD_ITERATORTODEQUE = "IteratorToDeque";
        public const string METHOD_TOPAIRNULLIFALLNULL = "ToPairNullIfAllNull";
        public const string METHOD_APPLYAGGVIEWRESULT = "ApplyAggViewResult";
        public const string METHOD_APPLYAGGJOINRESULT = "ApplyAggJoinResult";
        public const string METHOD_CLEARANDAGGREGATEUNGROUPED = "ClearAndAggregateUngrouped";
        public const string METHOD_POPULATESELECTJOINEVENTSNOHAVING = "PopulateSelectJoinEventsNoHaving";

        public const string METHOD_POPULATESELECTJOINEVENTSNOHAVINGWITHORDERBY =
            "PopulateSelectJoinEventsNoHavingWithOrderBy";

        public const string METHOD_POPULATESELECTEVENTSNOHAVING = "PopulateSelectEventsNoHaving";
        public const string METHOD_POPULATESELECTEVENTSNOHAVINGWITHORDERBY = "PopulateSelectEventsNoHavingWithOrderBy";
        public const string METHOD_GETSELECTJOINEVENTSNOHAVING = "GetSelectJoinEventsNoHaving";
        public const string METHOD_GETSELECTJOINEVENTSNOHAVINGWITHORDERBY = "GetSelectJoinEventsNoHavingWithOrderBy";
        public const string METHOD_GETSELECTEVENTSNOHAVING = "GetSelectEventsNoHaving";
        public const string METHOD_GETSELECTEVENTSNOHAVINGWITHORDERBY = "GetSelectEventsNoHavingWithOrderBy";
        public const string METHOD_ORDEROUTGOINGGETITERATOR = "OrderOutgoingGetEnumerator";

        public static void EvaluateHavingClauseCodegen(
            ExprForge optionalHavingClause,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (optionalHavingClause == null) {
                    method.Block.MethodReturn(ConstantTrue());
                }
                else {
                    method.Block
                        .Apply(Instblock(classScope, "qHavingClause", REF_EPS))
                        .DeclareVar<bool>(
                            "passed",
                            CodegenLegoMethodExpression.CodegenBooleanExpressionReturnTrueFalse(
                                optionalHavingClause,
                                classScope,
                                method,
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT))
                        .Apply(Instblock(classScope, "aHavingClause", Ref("passed")))
                        .MethodReturn(Ref("passed"));
                }
            };
            instance.Methods.AddMethod(
                typeof(bool),
                "EvaluateHavingClause",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="aggregationService">aggregations</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <param name="newData">istream</param>
        /// <param name="oldData">rstream</param>
        /// <param name="eventsPerStream">buf</param>
        public static void ApplyAggViewResult(
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext,
            EventBean[] newData,
            EventBean[] oldData,
            EventBean[] eventsPerStream)
        {
            if (newData != null) {
                // apply new data to aggregates
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    aggregationService.ApplyEnter(eventsPerStream, null, exprEvaluatorContext);
                }
            }

            if (oldData != null) {
                // apply old data to aggregates
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    aggregationService.ApplyLeave(eventsPerStream, null, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="aggregationService">aggregations</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <param name="newEvents">istream</param>
        /// <param name="oldEvents">rstream</param>
        public static void ApplyAggJoinResult(
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext,
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            if (newEvents != null) {
                // apply new data to aggregates
                foreach (var events in newEvents) {
                    aggregationService.ApplyEnter(events.Array, null, exprEvaluatorContext);
                }
            }

            if (oldEvents != null) {
                // apply old data to aggregates
                foreach (var events in oldEvents) {
                    aggregationService.ApplyLeave(events.Array, null, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectEventsNoHaving(
            SelectExprProcessor exprProcessor,
            EventBean[] events,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }

            var result = new EventBean[events.Length];
            var eventsPerStream = new EventBean[1];
            for (var i = 0; i < events.Length; i++) {
                eventsPerStream[0] = events[i];
                result[i] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
            }

            return result;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="aggregationService">aggregation svc</param>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">orders the outgoing events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectEventsNoHavingWithOrderBy(
            AggregationService aggregationService,
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            EventBean[] events,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }

            var result = new EventBean[events.Length];
            var eventGenerators = new EventBean[events.Length][];

            var eventsPerStream = new EventBean[1];
            for (var i = 0; i < events.Length; i++) {
                eventsPerStream[0] = events[i];
                result[i] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                eventGenerators[i] = new[] { events[i] };
            }

            return orderByProcessor.SortPlain(
                result,
                eventGenerators,
                isNewData,
                exprEvaluatorContext,
                aggregationService);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// <para>Also applies a having clause.</para>
        /// </summary>
        /// <param name="aggregationService">aggregation svc</param>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">for sorting output events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectEventsHavingWithOrderBy(
            AggregationService aggregationService,
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            EventBean[] events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }

            ArrayDeque<EventBean> result = null;
            ArrayDeque<EventBean[]> eventGenerators = null;

            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var generated = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (generated != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Length);
                        eventGenerators = new ArrayDeque<EventBean[]>(events.Length);
                    }

                    result.Add(generated);
                    eventGenerators.Add(new[] { theEvent });
                }
            }

            if (result != null) {
                return orderByProcessor.SortPlain(
                    ToArrayEvents(result),
                    ToArrayEventsArray(eventGenerators),
                    isNewData,
                    exprEvaluatorContext,
                    aggregationService);
            }

            return null;
        }

        public static CodegenMethod GetSelectEventsHavingWithOrderByCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNullReturnNull("events")
                    .DeclareVar<ArrayDeque<EventBean>>("result", ConstantNull())
                    .DeclareVar<ArrayDeque<EventBean[]>>("eventGenerators", ConstantNull())
                    .DeclareVar<EventBean[]>(NAME_EPS, NewArrayByLength(typeof(EventBean), Constant(1)));
                {
                    var forEach = methodNode.Block.ForEach<EventBean>("theEvent", Ref("events"));
                    forEach.AssignArrayElement(NAME_EPS, Constant(0), Ref("theEvent"));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "generated",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("generated")))
                        .IfCondition(EqualsNull(Ref("result")))
                        .AssignRef("result", NewInstance(typeof(ArrayDeque<EventBean>), ArrayLength(Ref("events"))))
                        .AssignRef("eventGenerators", NewInstance(typeof(ArrayDeque<EventBean[]>), ArrayLength(Ref("events"))))
                        .BlockEnd()
                        .ExprDotMethod(Ref("result"), "Add", Ref("generated"))
                        .DeclareVar<EventBean[]>("tmp", NewArrayByLength(typeof(EventBean), Constant(0)))
                        .AssignArrayElement("tmp", Constant(0), Ref("theEvent"))
                        .ExprDotMethod(Ref("eventGenerators"), "Add", Ref("tmp"))
                        .BlockEnd();
                }

                methodNode.Block.IfRefNullReturnNull("result")
                    .DeclareVar<EventBean[]>(
                        "arr",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("result")))
                    .DeclareVar<EventBean[][]>(
                        "gen",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTSARRAY, Ref("eventGenerators")))
                    .MethodReturn(
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "SortPlain",
                            Ref("arr"),
                            Ref("gen"),
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT,
                            MEMBER_AGGREGATIONSVC));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GetSelectEventsHavingWithOrderBy",
                CodegenNamedParam.From(
                    typeof(AggregationService),
                    MEMBER_AGGREGATIONSVC.Ref,
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    typeof(OrderByProcessor),
                    NAME_ORDERBYPROCESSOR,
                    typeof(EventBean[]),
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// <para />Also applies a having clause.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectEventsHaving(
            SelectExprProcessor exprProcessor,
            EventBean[] events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }

            ArrayDeque<EventBean> result = null;
            var eventsPerStream = new EventBean[1];

            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var generated = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (generated != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Length);
                    }

                    result.Add(generated);
                }
            }

            return ToArrayMayNull(result);
        }

        public static CodegenMethod GetSelectEventsHavingCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfRefNullReturnNull("events")
                    .DeclareVar<ArrayDeque<EventBean>>("result", ConstantNull())
                    .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

                {
                    var forEach = methodNode.Block.ForEach<EventBean>("theEvent", Ref("events"));
                    forEach.AssignArrayElement(REF_EPS, Constant(0), Ref("theEvent"));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "generated",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("generated")))
                        .IfCondition(EqualsNull(Ref("result")))
                        .AssignRef("result", NewInstance(typeof(ArrayDeque<EventBean>), ArrayLength(Ref("events"))))
                        .BlockEnd()
                        .ExprDotMethod(Ref("result"), "Add", Ref("generated"))
                        .BlockEnd();
                }
                methodNode.Block.MethodReturn(
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("result")));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GetSelectEventsHaving",
                CodegenNamedParam.From(
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    typeof(EventBean[]),
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="aggregationService">aggregation svc</param>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">for sorting output events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectJoinEventsNoHavingWithOrderBy(
            AggregationService aggregationService,
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }

            var result = new EventBean[events.Count];
            var eventGenerators = new EventBean[events.Count][];

            var count = 0;
            foreach (var key in events) {
                var eventsPerStream = key.Array;
                result[count] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                eventGenerators[count] = eventsPerStream;
                count++;
            }

            return orderByProcessor.SortPlain(
                result,
                eventGenerators,
                isNewData,
                exprEvaluatorContext,
                aggregationService);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectJoinEventsNoHaving(
            SelectExprProcessor exprProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }

            var result = new EventBean[events.Count];
            var count = 0;

            foreach (var key in events) {
                var eventsPerStream = key.Array;
                result[count] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                count++;
            }

            return result;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// <para />Also applies a having clause.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectJoinEventsHaving(
            SelectExprProcessor exprProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }

            ArrayDeque<EventBean> result = null;

            foreach (var key in events) {
                var eventsPerStream = key.Array;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Count);
                    }

                    result.Add(resultEvent);
                }
            }

            return ToArrayMayNull(result);
        }

        public static CodegenMethod GetSelectJoinEventsHavingCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfCondition(Or(EqualsNull(Ref("events")), ExprDotMethod(Ref("events"), "IsEmpty")))
                    .BlockReturn(ConstantNull())
                    .IfRefNullReturnNull("events")
                    .DeclareVar<ArrayDeque<EventBean>>("result", ConstantNull());
                {
                    var forEach = methodNode.Block.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "key", Ref("events"));
                    forEach.DeclareVar<EventBean[]>(
                        NAME_EPS,
                        Cast(typeof(EventBean[]), ExprDotName(Ref("key"), "Array")));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "generated",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("generated")))
                        .IfCondition(EqualsNull(Ref("result")))
                        .AssignRef(
                            "result",
                            NewInstance(typeof(ArrayDeque<EventBean>), ExprDotName(Ref("events"), "Count")))
                        .BlockEnd()
                        .ExprDotMethod(Ref("result"), "Add", Ref("generated"))
                        .BlockEnd();
                }
                methodNode.Block.MethodReturn(
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("result")));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GetSelectJoinEventsHaving",
                CodegenNamedParam.From(
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// <para>
        /// Also applies a having clause.
        /// </para>
        /// </summary>
        /// <param name="aggregationService">aggregation svc</param>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">for sorting output events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectJoinEventsHavingWithOrderBy(
            AggregationService aggregationService,
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }

            ArrayDeque<EventBean> result = null;
            ArrayDeque<EventBean[]> eventGenerators = null;

            foreach (var key in events) {
                var eventsPerStream = key.Array;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Count);
                        eventGenerators = new ArrayDeque<EventBean[]>(events.Count);
                    }

                    result.Add(resultEvent);
                    eventGenerators.Add(eventsPerStream);
                }
            }

            if (result != null) {
                return orderByProcessor.SortPlain(
                    CollectionUtil.ToArrayEvents(result),
                    CollectionUtil.ToArrayEventsArray(eventGenerators),
                    isNewData,
                    exprEvaluatorContext,
                    aggregationService);
            }

            return null;
        }

        public static CodegenMethod GetSelectJoinEventsHavingWithOrderByCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfCondition(Or(EqualsNull(Ref("events")), ExprDotMethod(Ref("events"), "IsEmpty")))
                    .BlockReturn(ConstantNull())
                    .IfRefNullReturnNull("events")
                    .DeclareVar<ArrayDeque<EventBean>>("result", ConstantNull())
                    .DeclareVar<ArrayDeque<EventBean[]>>("eventGenerators", ConstantNull());
                {
                    var forEach = methodNode.Block.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "key", Ref("events"));
                    forEach.DeclareVar<EventBean[]>(
                        NAME_EPS,
                        Cast(typeof(EventBean[]), ExprDotName(Ref("key"), "Array")));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "resultEvent",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("resultEvent")))
                        .IfCondition(EqualsNull(Ref("result")))
                        .AssignRef(
                            "result",
                            NewInstance(typeof(ArrayDeque<EventBean>), ExprDotName(Ref("events"), "Count")))
                        .AssignRef(
                            "eventGenerators",
                            NewInstance(typeof(ArrayDeque<EventBean[]>), ExprDotName(Ref("events"), "Count")))
                        .BlockEnd()
                        .ExprDotMethod(Ref("result"), "Add", Ref("resultEvent"))
                        .ExprDotMethod(Ref("eventGenerators"), "Add", Ref("eventsPerStream"))
                        .BlockEnd();
                }
                methodNode.Block.IfRefNullReturnNull("result")
                    .DeclareVar<EventBean[]>(
                        "arr",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("result")))
                    .DeclareVar<EventBean[][]>(
                        "gen",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTSARRAY, Ref("eventGenerators")))
                    .MethodReturn(
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "SortPlain",
                            Ref("arr"),
                            Ref("gen"),
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT,
                            MEMBER_AGGREGATIONSVC));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GetSelectJoinEventsHavingWithOrderBy",
                CodegenNamedParam.From(
                    typeof(AggregationService),
                    MEMBER_AGGREGATIONSVC.Ref,
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    typeof(OrderByProcessor),
                    NAME_ORDERBYPROCESSOR,
                    EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEANARRAY,
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        public static void PopulateSelectEventsNoHaving(
            SelectExprProcessor exprProcessor,
            EventBean[] events,
            bool isNewData,
            bool isSynthesize,
            ICollection<EventBean> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }

            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        public static void PopulateSelectEventsNoHavingWithOrderBy(
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            EventBean[] events,
            bool isNewData,
            bool isSynthesize,
            ICollection<EventBean> result,
            IList<object> sortKeys,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }

            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    sortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        public static void PopulateSelectEventsHaving(
            SelectExprProcessor exprProcessor,
            EventBean[] events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }

            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        public static CodegenMethod PopulateSelectEventsHavingCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfRefNull("events")
                    .BlockReturnNoValue()
                    .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

                {
                    var forEach = methodNode.Block.ForEach<EventBean>("theEvent", Ref("events"));
                    forEach.AssignArrayElement(REF_EPS, Constant(0), Ref("theEvent"));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "resultEvent",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("resultEvent")))
                        .ExprDotMethod(Ref("result"), "Add", Ref("resultEvent"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "PopulateSelectEventsHaving",
                CodegenNamedParam.From(
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    typeof(EventBean[]),
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "result",
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        public static void PopulateSelectEventsHavingWithOrderBy(
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            EventBean[] events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> result,
            IList<object> optSortKeys,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }

            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    optSortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        public static CodegenMethod PopulateSelectEventsHavingWithOrderByCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfRefNull("events")
                    .BlockReturnNoValue()
                    .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

                {
                    var forEach = methodNode.Block.ForEach<EventBean>("theEvent", Ref("events"));
                    forEach.AssignArrayElement(REF_EPS, Constant(0), Ref("theEvent"));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "resultEvent",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("resultEvent")))
                        .ExprDotMethod(Ref("result"), "Add", Ref("resultEvent"))
                        .ExprDotMethod(
                            Ref("optSortKeys"),
                            "Add",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "GetSortKey",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "PopulateSelectEventsHavingWithOrderBy",
                CodegenNamedParam.From(
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    typeof(OrderByProcessor),
                    NAME_ORDERBYPROCESSOR,
                    typeof(EventBean[]),
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "result",
                    typeof(IList<object>),
                    "optSortKeys",
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        public static void PopulateSelectJoinEventsHaving(
            SelectExprProcessor exprProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }

            foreach (var key in events) {
                var eventsPerStream = key.Array;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        public static CodegenMethod PopulateSelectJoinEventsHavingCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNull("events").BlockReturnNoValue();

                {
                    var forEach = methodNode.Block.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "key", Ref("events"));
                    forEach.DeclareVar<EventBean[]>(
                        NAME_EPS,
                        Cast(typeof(EventBean[]), ExprDotName(Ref("key"), "Array")));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "resultEvent",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("resultEvent")))
                        .ExprDotMethod(Ref("result"), "Add", Ref("resultEvent"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "PopulateSelectJoinEventsHavingCodegen",
                CodegenNamedParam.From(
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "result",
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        public static void PopulateSelectJoinEventsHavingWithOrderBy(
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            ExprEvaluator havingNode,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> result,
            IList<object> sortKeys,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }

            foreach (var key in events) {
                var eventsPerStream = key.Array;

                var passesHaving = EvaluateHavingClause(havingNode, eventsPerStream, isNewData, exprEvaluatorContext);
                if (!passesHaving) {
                    continue;
                }

                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    sortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        public static CodegenMethod PopulateSelectJoinEventsHavingWithOrderByCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNull("events").BlockReturnNoValue();

                {
                    var forEach = methodNode.Block.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "key", Ref("events"));
                    forEach.DeclareVar<EventBean[]>(
                        NAME_EPS,
                        Cast(typeof(EventBean[]), ExprDotName(Ref("key"), "Array")));
                    forEach.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_EXPREVALCONTEXT)))
                        .BlockContinue();
                    forEach.DeclareVar<EventBean>(
                            "resultEvent",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRNONMEMBER,
                                "Process",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(NotEqualsNull(Ref("resultEvent")))
                        .ExprDotMethod(Ref("result"), "Add", Ref("resultEvent"))
                        .ExprDotMethod(
                            Ref("sortKeys"),
                            "Add",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "GetSortKey",
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "PopulateSelectJoinEventsHavingWithOrderBy",
                CodegenNamedParam.From(
                    typeof(SelectExprProcessor),
                    NAME_SELECTEXPRPROCESSOR,
                    typeof(OrderByProcessor),
                    NAME_ORDERBYPROCESSOR,
                    EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEANARRAY,
                    "events",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "result",
                    typeof(IList<object>),
                    "sortKeys",
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        public static void PopulateSelectJoinEventsNoHaving(
            SelectExprProcessor exprProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var length = events?.Count ?? 0;
            if (length == 0) {
                return;
            }

            foreach (var key in events) {
                var eventsPerStream = key.Array;
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        public static void PopulateSelectJoinEventsNoHavingWithOrderBy(
            SelectExprProcessor exprProcessor,
            OrderByProcessor orderByProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            bool isNewData,
            bool isSynthesize,
            IList<EventBean> result,
            IList<object> optSortKeys,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var length = events?.Count ?? 0;
            if (length == 0) {
                return;
            }

            foreach (var key in events) {
                var eventsPerStream = key.Array;
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    optSortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        public static void ClearAndAggregateUngrouped(
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService,
            Viewable parent)
        {
            aggregationService.ClearResults(exprEvaluatorContext);
            using (var enumerator = parent.GetEnumerator()) {
                var eventsPerStream = new EventBean[1];
                while (enumerator.MoveNext()) {
                    eventsPerStream[0] = enumerator.Current;
                    aggregationService.ApplyEnter(eventsPerStream, null, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumerator">enumerator</param>
        /// <returns>deque</returns>
        public static ArrayDeque<EventBean> IteratorToDeque(IEnumerator<EventBean> enumerator)
        {
            var deque = new ArrayDeque<EventBean>();
            while (enumerator.MoveNext()) {
                deque.Add(enumerator.Current);
            }

            return deque;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="selectNewEvents">new events</param>
        /// <param name="selectOldEvents">old events</param>
        /// <returns>pair or null</returns>
        public static UniformPair<EventBean[]> ToPairNullIfAllNull(
            EventBean[] selectNewEvents,
            EventBean[] selectOldEvents)
        {
            if (selectNewEvents != null || selectOldEvents != null) {
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }

            return null;
        }

        public static void ProcessViewResultCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenInstanceAux instance,
            bool hasHaving,
            bool selectRStream,
            bool hasOrderBy,
            bool outputNullIfBothNull)
        {
            // generate new events using select expressions
            if (!hasHaving) {
                if (selectRStream) {
                    if (!hasOrderBy) {
                        method.Block.AssignRef(
                            "selectOldEvents",
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_GETSELECTEVENTSNOHAVING,
                                MEMBER_SELECTEXPRPROCESSOR,
                                REF_OLDDATA,
                                Constant(false),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                    else {
                        method.Block.AssignRef(
                            "selectOldEvents",
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_GETSELECTEVENTSNOHAVINGWITHORDERBY,
                                MEMBER_AGGREGATIONSVC,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                }

                if (!hasOrderBy) {
                    method.Block.AssignRef(
                        "selectNewEvents",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_NEWDATA,
                            Constant(true),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    method.Block.AssignRef(
                        "selectNewEvents",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTEVENTSNOHAVINGWITHORDERBY,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
            }
            else {
                if (selectRStream) {
                    if (!hasOrderBy) {
                        var select = GetSelectEventsHavingCodegen(classScope, instance);
                        method.Block.AssignRef(
                            "selectOldEvents",
                            LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                    else {
                        var select = GetSelectEventsHavingWithOrderByCodegen(classScope, instance);
                        method.Block.AssignRef(
                            "selectOldEvents",
                            LocalMethod(
                                select,
                                MEMBER_AGGREGATIONSVC,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                }

                if (!hasOrderBy) {
                    var select = GetSelectEventsHavingCodegen(classScope, instance);
                    method.Block.AssignRef(
                        "selectNewEvents",
                        LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    var select = GetSelectEventsHavingWithOrderByCodegen(classScope, instance);
                    method.Block.AssignRef(
                        "selectNewEvents",
                        LocalMethod(
                            select,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
            }

            if (outputNullIfBothNull) {
                method.Block
                    .IfCondition(And(EqualsNull(Ref("selectNewEvents")), EqualsNull(Ref("selectOldEvents"))))
                    .BlockReturn(ConstantNull());
            }

            method.Block.MethodReturn(
                NewInstance(typeof(UniformPair<EventBean[]>), Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        public static void ProcessJoinResultCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenInstanceAux instance,
            bool hasHaving,
            bool selectRStream,
            bool hasOrderBy,
            bool outputNullIfBothNull)
        {
            if (!hasHaving) {
                if (selectRStream) {
                    if (!hasOrderBy) {
                        method.Block.AssignRef(
                            "selectOldEvents",
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_GETSELECTJOINEVENTSNOHAVING,
                                MEMBER_SELECTEXPRPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                    else {
                        method.Block.AssignRef(
                            "selectOldEvents",
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_GETSELECTJOINEVENTSNOHAVINGWITHORDERBY,
                                ConstantNull(),
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                }

                if (!hasOrderBy) {
                    method.Block.AssignRef(
                        "selectNewEvents ",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTJOINEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    method.Block.AssignRef(
                        "selectNewEvents ",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTJOINEVENTSNOHAVINGWITHORDERBY,
                            ConstantNull(),
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
            }
            else {
                if (selectRStream) {
                    if (!hasOrderBy) {
                        var select = GetSelectJoinEventsHavingCodegen(classScope, instance);
                        method.Block.AssignRef(
                            "selectOldEvents",
                            LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                    else {
                        var select = GetSelectJoinEventsHavingWithOrderByCodegen(classScope, instance);
                        method.Block.AssignRef(
                            "selectOldEvents",
                            LocalMethod(
                                select,
                                MEMBER_AGGREGATIONSVC,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                REF_EXPREVALCONTEXT));
                    }
                }

                if (!hasOrderBy) {
                    var select = GetSelectJoinEventsHavingCodegen(classScope, instance);
                    method.Block.AssignRef(
                        "selectNewEvents",
                        LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    var select = GetSelectJoinEventsHavingWithOrderByCodegen(classScope, instance);
                    method.Block.AssignRef(
                        "selectNewEvents",
                        LocalMethod(
                            select,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
            }

            if (outputNullIfBothNull) {
                method.Block
                    .IfCondition(And(EqualsNull(Ref("selectNewEvents")), EqualsNull(Ref("selectOldEvents"))))
                    .BlockReturn(ConstantNull());
            }

            method.Block.MethodReturn(
                NewInstance(typeof(UniformPair<EventBean[]>), Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="outgoingEvents">events</param>
        /// <param name="orderKeys">keys</param>
        /// <param name="orderByProcessor">ordering</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <returns>ordered events</returns>
        public static ArrayEventEnumerator OrderOutgoingGetEnumerator(
            IList<EventBean> outgoingEvents,
            IList<object> orderKeys,
            OrderByProcessor orderByProcessor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var outgoingEventsArr = ToArrayEvents(outgoingEvents);
            var orderKeysArr = ToArrayObjects(orderKeys);
            var orderedEvents = orderByProcessor.SortWOrderKeys(outgoingEventsArr, orderKeysArr, exprEvaluatorContext);
            return new ArrayEventEnumerator(orderedEvents);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="count">count</param>
        /// <param name="events">events</param>
        /// <param name="keys">keys</param>
        /// <param name="currentGenerators">key-generators</param>
        /// <param name="isNewData">irstream</param>
        /// <param name="orderByProcessor">order-by</param>
        /// <param name="agentInstanceContext">ctx</param>
        /// <param name="aggregationService">aggregation svc</param>
        /// <returns>events for output</returns>
        public static EventBean[] OutputFromCountMaySort(
            int count,
            EventBean[] events,
            object[] keys,
            EventBean[][] currentGenerators,
            bool isNewData,
            OrderByProcessor orderByProcessor,
            AgentInstanceContext agentInstanceContext,
            AggregationService aggregationService)
        {
            // Resize if some rows were filtered out
            if (count != events.Length) {
                if (count == 0) {
                    return null;
                }

                events = ShrinkArrayEvents(count, events);

                if (orderByProcessor != null) {
                    keys = ShrinkArrayObjects(count, keys);
                    currentGenerators = ShrinkArrayEventArray(count, currentGenerators);
                }
            }

            if (orderByProcessor != null) {
                events = orderByProcessor.SortWGroupKeys(
                    events,
                    currentGenerators,
                    keys,
                    isNewData,
                    agentInstanceContext,
                    aggregationService);
            }

            return events;
        }

        public static void OutputFromCountMaySortCodegen(
            CodegenBlock block,
            CodegenExpressionRef count,
            CodegenExpressionRef events,
            CodegenExpressionRef keys,
            CodegenExpressionRef currentGenerators,
            bool hasOrderBy)
        {
            var resize = block.IfCondition(Not(EqualsIdentity(count, ArrayLength(events))));
            resize.IfCondition(EqualsIdentity(count, Constant(0)))
                .BlockReturn(ConstantNull())
                .AssignRef(events.Ref, StaticMethod(typeof(CollectionUtil), METHOD_SHRINKARRAYEVENTS, count, events));

            if (hasOrderBy) {
                resize.AssignRef(keys.Ref, StaticMethod(typeof(CollectionUtil), METHOD_SHRINKARRAYOBJECTS, count, keys))
                    .AssignRef(
                        currentGenerators.Ref,
                        StaticMethod(typeof(CollectionUtil), METHOD_SHRINKARRAYEVENTARRAY, count, currentGenerators));
            }

            if (hasOrderBy) {
                block.AssignRef(
                    events.Ref,
                    ExprDotMethod(
                        MEMBER_ORDERBYPROCESSOR,
                        "SortWGroupKeys",
                        events,
                        currentGenerators,
                        keys,
                        REF_ISNEWDATA,
                        MEMBER_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC));
            }

            block.MethodReturn(events);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="newEvents">newdata</param>
        /// <param name="newEventsSortKey">newdata sortkey</param>
        /// <param name="oldEvents">olddata</param>
        /// <param name="oldEventsSortKey">olddata sortkey</param>
        /// <param name="selectRStream">rstream flag</param>
        /// <param name="orderByProcessor">ordering</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <returns>pair</returns>
        public static UniformPair<EventBean[]> FinalizeOutputMaySortMayRStream(
            IList<EventBean> newEvents,
            IList<object> newEventsSortKey,
            IList<EventBean> oldEvents,
            IList<object> oldEventsSortKey,
            bool selectRStream,
            OrderByProcessor orderByProcessor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var newEventsArr = ToArrayNullForEmptyEvents(newEvents);
            EventBean[] oldEventsArr = null;
            if (selectRStream) {
                oldEventsArr = ToArrayNullForEmptyEvents(oldEvents);
            }

            if (orderByProcessor != null) {
                var sortKeysNew = ToArrayNullForEmptyObjects(newEventsSortKey);
                newEventsArr = orderByProcessor.SortWOrderKeys(newEventsArr, sortKeysNew, exprEvaluatorContext);
                if (selectRStream) {
                    var sortKeysOld = ToArrayNullForEmptyObjects(oldEventsSortKey);
                    oldEventsArr = orderByProcessor.SortWOrderKeys(oldEventsArr, sortKeysOld, exprEvaluatorContext);
                }
            }

            return ToPairNullIfAllNull(newEventsArr, oldEventsArr);
        }

        public static void FinalizeOutputMaySortMayRStreamCodegen(
            CodegenBlock block,
            CodegenExpressionRef newEvents,
            CodegenExpressionRef newEventsSortKey,
            CodegenExpressionRef oldEvents,
            CodegenExpressionRef oldEventsSortKey,
            bool selectRStream,
            bool hasOrderBy)
        {
            block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar<EventBean[]>(
                    "newEventsArrX",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, newEvents))
                .DeclareVar<EventBean[]>(
                    "oldEventsArrX",
                    selectRStream
                        ? StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, oldEvents)
                        : ConstantNull());

            if (hasOrderBy) {
                block.DeclareVar<object[]>(
                        "sortKeysNew",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYOBJECTS, newEventsSortKey))
                    .AssignRef(
                        "newEventsArrX",
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "SortWOrderKeys",
                            Ref("newEventsArrX"),
                            Ref("sortKeysNew"),
                            MEMBER_EXPREVALCONTEXT));
                if (selectRStream) {
                    block.DeclareVar<object[]>(
                            "sortKeysOld",
                            StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYOBJECTS, oldEventsSortKey))
                        .AssignRef(
                            "oldEventsArrX",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "SortWOrderKeys",
                                Ref("oldEventsArrX"),
                                Ref("sortKeysOld"),
                                MEMBER_EXPREVALCONTEXT));
                }
            }

            block.ReturnMethodOrBlock(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_TOPAIRNULLIFALLNULL,
                    Ref("newEventsArrX"),
                    Ref("oldEventsArrX")));
        }

        public static void PrefixCodegenNewOldEvents(
            CodegenBlock block,
            bool sorting,
            bool selectRStream)
        {
            block
                .DeclareVar<IList<EventBean>>("newEvents", NewInstance(typeof(List<EventBean>)))
                .DeclareVar<IList<EventBean>>("oldEvents", selectRStream ? NewInstance(typeof(List<EventBean>)) : ConstantNull());

            block
                .DeclareVar<IList<object>>("newEventsSortKey", ConstantNull())
                .DeclareVar<IList<object>>("oldEventsSortKey", ConstantNull());
            if (sorting) {
                block.AssignRef("newEventsSortKey", NewInstance(typeof(List<object>)))
                    .AssignRef(
                        "oldEventsSortKey",
                        selectRStream ? NewInstance(typeof(List<object>)) : ConstantNull());
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="istream">istream event</param>
        /// <param name="rstream">rstream event</param>
        /// <returns>pair</returns>
        public static UniformPair<EventBean[]> ToPairNullIfAllNullSingle(
            EventBean istream,
            EventBean rstream)
        {
            if (istream != null) {
                return new UniformPair<EventBean[]>(
                    new[] { istream },
                    rstream == null ? null : new[] { rstream });
            }

            return rstream == null ? null : new UniformPair<EventBean[]>(null, new[] { rstream });
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="istream">istream event</param>
        /// <returns>pair</returns>
        public static UniformPair<EventBean[]> ToPairNullIfNullIStream(EventBean istream)
        {
            return istream == null ? null : new UniformPair<EventBean[]>(new[] { istream }, null);
        }

        public static bool EvaluateHavingClause(
            ExprEvaluator havingEval,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var pass = havingEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            return pass != null && true.Equals(pass);
        }
    }
} // end of namespace