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
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
    /// <summary>
    ///     Result-set processor for the aggregate-grouped case:
    ///     there is a group-by and one or more non-aggregation event properties in the select clause are not listed in the
    ///     group by,
    ///     and there are aggregation functions.
    ///     <para />
    ///     This processor does perform grouping by computing MultiKey group-by keys for each row.
    ///     The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
    ///     <para />
    ///     Aggregation state is a table of rows held by aggegation service where the row key is the group-by MultiKey.
    /// </summary>
    public class ResultSetProcessorAggregateGroupedImpl
    {
        private const string NAME_OUTPUTALLHELPER = "outputAllHelper";
        private const string NAME_OUTPUTLASTHELPER = "outputLastHelper";
        private const string NAME_OUTPUTFIRSTHELPER = "outputFirstHelper";
        private const string NAME_OUTPUTALLGROUPREPS = "outputAllGroupReps";

        public static void ApplyViewResultCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            method.Block.DeclareVar<EventBean[]>(
                    "eventsPerStream",
                    NewArrayByLength(typeof(EventBean), Constant(1)))
                .IfCondition(NotEqualsNull(REF_NEWDATA))
                .ForEach(typeof(EventBean), "aNewData", REF_NEWDATA)
                .AssignArrayElement("eventsPerStream", Constant(0), Ref("aNewData"))
                .DeclareVar<object>(
                    "mk",
                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                .ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "applyEnter",
                    Ref("eventsPerStream"),
                    Ref("mk"),
                    REF_AGENTINSTANCECONTEXT)
                .BlockEnd()
                .BlockEnd()
                .IfCondition(NotEqualsNull(REF_OLDDATA))
                .ForEach(typeof(EventBean), "anOldData", REF_OLDDATA)
                .AssignArrayElement("eventsPerStream", Constant(0), Ref("anOldData"))
                .DeclareVar<object>(
                    "mk",
                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
                .ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "applyLeave",
                    Ref("eventsPerStream"),
                    Ref("mk"),
                    REF_AGENTINSTANCECONTEXT)
                .BlockEnd()
                .BlockEnd();
        }

        public static void ApplyJoinResultCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            method.Block
                .IfCondition(Not(ExprDotMethod(REF_NEWDATA, "isEmpty")))
                .ForEach(typeof(MultiKey<EventBean>), "aNewEvent", REF_NEWDATA)
                .DeclareVar<EventBean[]>(
                    "eventsPerStream",
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewEvent"), "getArray")))
                .DeclareVar<object>(
                    "mk",
                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                .ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "applyEnter",
                    Ref("eventsPerStream"),
                    Ref("mk"),
                    REF_AGENTINSTANCECONTEXT)
                .BlockEnd()
                .BlockEnd()
                .IfCondition(And(NotEqualsNull(REF_OLDDATA), Not(ExprDotMethod(REF_OLDDATA, "isEmpty"))))
                .ForEach(typeof(MultiKey<EventBean>), "anOldEvent", REF_OLDDATA)
                .DeclareVar<EventBean[]>(
                    "eventsPerStream",
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldEvent"), "getArray")))
                .DeclareVar<object>(
                    "mk",
                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
                .ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "applyLeave",
                    Ref("eventsPerStream"),
                    Ref("mk"),
                    REF_AGENTINSTANCECONTEXT)
                .BlockEnd()
                .BlockEnd();
        }

        public static void ProcessJoinResultCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

            method.Block.DeclareVar<object[]>(
                    "newDataGroupByKeys",
                    LocalMethod(generateGroupKeyArrayJoin, REF_NEWDATA, ConstantTrue()))
                .DeclareVar<object[]>(
                    "oldDataGroupByKeys",
                    LocalMethod(generateGroupKeyArrayJoin, REF_OLDDATA, ConstantFalse()));

            if (forge.IsUnidirectional) {
                method.Block.ExprDotMethod(Ref("this"), "clear");
            }

            method.Block.StaticMethod(
                typeof(ResultSetProcessorGroupedUtil),
                ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                REF_AGGREGATIONSVC,
                REF_AGENTINSTANCECONTEXT,
                REF_NEWDATA,
                Ref("newDataGroupByKeys"),
                REF_OLDDATA,
                Ref("oldDataGroupByKeys"));

            method.Block.DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsJoin,
                            REF_OLDDATA,
                            Ref("oldDataGroupByKeys"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE)
                        : ConstantNull())
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsJoin,
                        REF_NEWDATA,
                        Ref("newDataGroupByKeys"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        public static void ProcessViewResultCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayView = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayViewCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            var processViewResultNewDepthOne = ProcessViewResultNewDepthOneCodegen(forge, classScope, instance);
            var processViewResultPairDepthOneNoRStream =
                ProcessViewResultPairDepthOneCodegen(forge, classScope, instance);

            var ifShortcut = method.Block.IfCondition(
                And(NotEqualsNull(REF_NEWDATA), EqualsIdentity(ArrayLength(REF_NEWDATA), Constant(1))));
            ifShortcut.IfCondition(Or(EqualsNull(REF_OLDDATA), EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(0))))
                .BlockReturn(LocalMethod(processViewResultNewDepthOne, REF_NEWDATA, REF_ISSYNTHESIZE))
                .IfCondition(EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(1)))
                .BlockReturn(
                    LocalMethod(processViewResultPairDepthOneNoRStream, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE));

            method.Block.DeclareVar<object[]>(
                    "newDataGroupByKeys",
                    LocalMethod(generateGroupKeyArrayView, REF_NEWDATA, ConstantTrue()))
                .DeclareVar<object[]>(
                    "oldDataGroupByKeys",
                    LocalMethod(generateGroupKeyArrayView, REF_OLDDATA, ConstantFalse()))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    REF_NEWDATA,
                    Ref("newDataGroupByKeys"),
                    REF_OLDDATA,
                    Ref("oldDataGroupByKeys"),
                    Ref("eventsPerStream"));

            method.Block.DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView,
                            REF_OLDDATA,
                            Ref("oldDataGroupByKeys"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE,
                            Ref("eventsPerStream"))
                        : ConstantNull())
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsView,
                        REF_NEWDATA,
                        Ref("newDataGroupByKeys"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("eventsPerStream")))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        private static CodegenMethod GenerateOutputEventsViewCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNullReturnNull(Ref("outputEvents"))
                    .DeclareVar<EventBean[]>(
                        "events",
                        NewArrayByLength(typeof(EventBean), ArrayLength(Ref("outputEvents"))))
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ArrayLength(Ref("outputEvents"))));

                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar<EventBean[][]>(
                        "currentGenerators",
                        NewArrayByLength(typeof(EventBean[]), ArrayLength(Ref("outputEvents"))));
                }

                methodNode.Block.DeclareVar<int>("countOutputRows", Constant(0))
                    .DeclareVar<int>("cpid", ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"));

                {
                    var forLoop = methodNode.Block.ForLoopIntSimple("countInputRows", ArrayLength(Ref("outputEvents")));
                    forLoop.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")),
                            Ref("cpid"),
                            ConstantNull())
                        .AssignArrayElement(
                            ExprForgeCodegenNames.REF_EPS,
                            Constant(0),
                            ArrayAtIndex(Ref("outputEvents"), Ref("countInputRows")));

                    if (forge.OptionalHavingNode != null) {
                        forLoop.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockContinue();
                    }

                    forLoop.AssignArrayElement(
                            "events",
                            Ref("countOutputRows"),
                            ExprDotMethod(
                                REF_SELECTEXPRPROCESSOR,
                                "process",
                                ExprForgeCodegenNames.REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT))
                        .AssignArrayElement(
                            "keys",
                            Ref("countOutputRows"),
                            ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")));

                    if (forge.IsSorting) {
                        forLoop.AssignArrayElement(
                            "currentGenerators",
                            Ref("countOutputRows"),
                            NewArrayWithInit(
                                typeof(EventBean),
                                ArrayAtIndex(Ref("outputEvents"), Ref("countInputRows"))));
                    }

                    forLoop.Increment("countOutputRows")
                        .BlockEnd();
                }

                OutputFromCountMaySortCodegen(
                    methodNode.Block,
                    Ref("countOutputRows"),
                    Ref("events"),
                    Ref("keys"),
                    Ref("currentGenerators"),
                    forge.IsSorting);
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "generateOutputEventsView",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "outputEvents",
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS),
                typeof(ResultSetProcessorAggregateGroupedImpl),
                classScope,
                code);
        }

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTALLGROUPREPS));
            }

            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTALLHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTFIRSTHELPER));
            }
        }

        private static CodegenMethod GenerateOutputEventsJoinCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(ExprDotMethod(Ref("resultSet"), "isEmpty"))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<EventBean[]>(
                        "events",
                        NewArrayByLength(typeof(EventBean), ExprDotMethod(Ref("resultSet"), "size")))
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ExprDotMethod(Ref("resultSet"), "size")));

                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar<EventBean[][]>(
                        "currentGenerators",
                        NewArrayByLength(typeof(EventBean[]), ExprDotMethod(Ref("resultSet"), "size")));
                }

                methodNode.Block.DeclareVar<int>("countOutputRows", Constant(0))
                    .DeclareVar<int>("countInputRows", Constant(-1))
                    .DeclareVar<int>("cpid", ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"));

                {
                    var forLoop = methodNode.Block.ForEach(typeof(MultiKey<object>), "row", Ref("resultSet"));
                    forLoop.Increment("countInputRows")
                        .DeclareVar<EventBean[]>(
                            "eventsPerStream",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("row"), "getArray")))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")),
                            Ref("cpid"),
                            ConstantNull());

                    if (forge.OptionalHavingNode != null) {
                        forLoop.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockContinue();
                    }

                    forLoop.AssignArrayElement(
                            "events",
                            Ref("countOutputRows"),
                            ExprDotMethod(
                                REF_SELECTEXPRPROCESSOR,
                                "process",
                                ExprForgeCodegenNames.REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT))
                        .AssignArrayElement(
                            "keys",
                            Ref("countOutputRows"),
                            ArrayAtIndex(Ref("groupByKeys"), Ref("countInputRows")));

                    if (forge.IsSorting) {
                        forLoop.AssignArrayElement("currentGenerators", Ref("countOutputRows"), Ref("eventsPerStream"));
                    }

                    forLoop.Increment("countOutputRows")
                        .BlockEnd();
                }

                OutputFromCountMaySortCodegen(
                    methodNode.Block,
                    Ref("countOutputRows"),
                    Ref("events"),
                    Ref("keys"),
                    Ref("currentGenerators"),
                    forge.IsSorting);
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "generateOutputEventsJoin",
                CodegenNamedParam.From(
                    typeof(ISet<object>),
                    "resultSet",
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorAggregateGroupedImpl),
                classScope,
                code);
        }

        public static void GetIteratorViewCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsHistoricalOnly) {
                method.Block.MethodReturn(
                    LocalMethod(ObtainIteratorCodegen(forge, method, classScope, instance), REF_VIEWABLE));
                return;
            }

            var generateGroupKeySingle = ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT)
                .DeclareVar<IEnumerator<EventBean>>("it", ExprDotMethod(REF_VIEWABLE, "iterator"))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                method.Block.WhileLoop(ExprDotMethod(Ref("it"), "hasNext"))
                    .AssignArrayElement(
                        Ref("eventsPerStream"),
                        Constant(0),
                        Cast(typeof(EventBean), ExprDotMethod(Ref("it"), "next")))
                    .DeclareVar<object>(
                        "groupKey",
                        LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "applyEnter",
                        Ref("eventsPerStream"),
                        Ref("groupKey"),
                        REF_AGENTINSTANCECONTEXT)
                    .BlockEnd();
            }

            method.Block.DeclareVar<ArrayDeque<object>>(
                    "deque",
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ITERATORTODEQUE,
                        LocalMethod(ObtainIteratorCodegen(forge, method, classScope, instance), REF_VIEWABLE)))
                .ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "iterator"));
        }

        private static CodegenMethod ObtainIteratorCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenMethod parent,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var iterator = parent
                .MakeChild(typeof(IEnumerator<object>), typeof(ResultSetProcessorAggregateGroupedImpl), classScope)
                .AddParam(typeof(Viewable), NAME_VIEWABLE);
            if (!forge.IsSorting) {
                iterator.Block.MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorAggregateGroupedIterator),
                        "Create",
                        ExprDotMethod(REF_VIEWABLE, "iterator"),
                        Ref("this"),
                        REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT));
                return iterator;
            }

            var generateGroupKeySingle =
                ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                    forge.GroupKeyNodeExpressions,
                    classScope,
                    instance);

            // Pull all parent events, generate order keys
            iterator.Block.DeclareVar<EventBean[]>(
                    "eventsPerStream",
                    NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar<IList<object>>("outgoingEvents", NewInstance(typeof(List<object>)))
                .DeclareVar<IList<object>>("orderKeys", NewInstance(typeof(List<object>)));

            {
                var forLoop = iterator.Block.ForEach(typeof(EventBean), "candidate", REF_VIEWABLE);
                forLoop.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("candidate"))
                    .DeclareVar<object>(
                        "groupKey",
                        LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "SetCurrentAccess",
                        Ref("groupKey"),
                        ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                        ConstantNull());

                if (forge.OptionalHavingNode != null) {
                    forLoop.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    ConstantTrue(),
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockContinue();
                }

                forLoop.ExprDotMethod(
                        Ref("outgoingEvents"),
                        "add",
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR,
                            "process",
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT))
                    .ExprDotMethod(
                        Ref("orderKeys"),
                        "add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "getSortKey",
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT));
            }

            iterator.Block.MethodReturn(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_ORDEROUTGOINGGETITERATOR,
                    Ref("outgoingEvents"),
                    Ref("orderKeys"),
                    REF_ORDERBYPROCESSOR,
                    REF_AGENTINSTANCECONTEXT));
            return iterator;
        }

        public static void GetIteratorJoinCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

            method.Block
                .DeclareVar<object[]>(
                    "groupByKeys",
                    LocalMethod(
                        generateGroupKeyArrayJoin,
                        REF_JOINSET,
                        ConstantTrue()))
                .DeclareVar<EventBean[]>(
                    "result",
                    LocalMethod(
                        generateOutputEventsJoin,
                        REF_JOINSET,
                        Ref("groupByKeys"),
                        ConstantTrue(),
                        ConstantTrue()))
                .MethodReturn(
                    NewInstance<ArrayEventEnumerator>(Ref("result")));
        }

        public static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT);
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                ProcessOutputLimitedJoinDefaultCodegen(forge, classScope, method, instance);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                ProcessOutputLimitedJoinAllCodegen(forge, classScope, method, instance);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                ProcessOutputLimitedJoinFirstCodegen(forge, classScope, method, instance);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.LAST) {
                ProcessOutputLimitedJoinLastCodegen(forge, classScope, method, instance);
            }
            else {
                throw new IllegalStateException("Unrecognized output limit " + outputLimitLimitType);
            }
        }

        public static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                ProcessOutputLimitedViewDefaultCodegen(forge, classScope, method, instance);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                ProcessOutputLimitedViewAllCodegen(forge, classScope, method, instance);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                ProcessOutputLimitedViewFirstCodegen(forge, classScope, method, instance);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.LAST) {
                ProcessOutputLimitedViewLastCodegen(forge, classScope, method, instance);
            }
            else {
                throw new IllegalStateException("Unrecognized output limited type " + outputLimitLimitType);
            }
        }

        public static void StopMethodCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "destroy");
            }

            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "destroy");
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTFIRSTHELPER), "destroy");
            }
        }

        protected internal static CodegenMethod GenerateOutputBatchedJoinUnkeyedCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
                    .BlockReturnNoValue()
                    .DeclareVar<int>("count", Constant(0))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var forEach = methodNode.Block.ForEach(typeof(MultiKey<object>), "row", Ref("outputEvents"));
                    forEach.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ArrayAtIndex(Ref("groupByKeys"), Ref("count")),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotMethod(Ref("row"), "getArray")));

                    if (forge.OptionalHavingNode != null) {
                        forEach.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        Ref("eventsPerStream"),
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .Increment("count")
                            .BlockContinue();
                    }

                    forEach.ExprDotMethod(
                        Ref("resultEvents"),
                        "add",
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR,
                            "process",
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT));

                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("optSortKeys"),
                            "add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT));
                    }

                    forEach.Increment("count");
                }
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedJoinUnkeyed",
                CodegenNamedParam.From(
                    typeof(ISet<object>),
                    "outputEvents",
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(ICollection<object>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorAggregateGrouped),
                classScope,
                code);
        }

        protected internal static void GenerateOutputBatchedSingleCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("groupByKey"),
                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                    ConstantNull());

                if (forge.OptionalHavingNode != null) {
                    methodNode.Block
                        .IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                methodNode.Block.MethodReturn(
                    ExprDotMethod(
                        REF_SELECTEXPRPROCESSOR,
                        "process",
                        Ref("eventsPerStream"),
                        REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        REF_AGENTINSTANCECONTEXT));
            };
            instance.Methods.AddMethod(
                typeof(EventBean),
                "generateOutputBatchedSingle",
                CodegenNamedParam.From(
                    typeof(object),
                    "groupByKey",
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedViewPerKeyCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
                    .BlockReturnNoValue()
                    .DeclareVar<int>("count", Constant(0));

                {
                    var forEach = methodNode.Block.ForEach(typeof(EventBean), "outputEvent", Ref("outputEvents"));
                    forEach.DeclareVar<object>("groupKey", ArrayAtIndex(Ref("groupByKeys"), Ref("count")))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("groupKey"),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .AssignArrayElement(
                            Ref("eventsPerStream"),
                            Constant(0),
                            ArrayAtIndex(Ref("outputEvents"), Ref("count")));

                    if (forge.OptionalHavingNode != null) {
                        forEach.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockContinue();
                    }

                    forEach.ExprDotMethod(
                        Ref("resultEvents"),
                        "put",
                        Ref("groupKey"),
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR,
                            "process",
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT));

                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("optSortKeys"),
                            "put",
                            Ref("groupKey"),
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT));
                    }

                    forEach.Increment("count");
                }
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedViewPerKey",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "outputEvents",
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IDictionary<object, object>),
                    "resultEvents",
                    typeof(IDictionary<object, object>),
                    "optSortKeys",
                    typeof(EventBean[]),
                    "eventsPerStream"),
                typeof(ResultSetProcessorAggregateGrouped),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedJoinPerKeyCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
                    .BlockReturnNoValue()
                    .DeclareVar<int>("count", Constant(0));

                {
                    var forEach = methodNode.Block.ForEach(typeof(MultiKey<object>), "row", Ref("outputEvents"));
                    forEach.DeclareVar<object>("groupKey", ArrayAtIndex(Ref("groupByKeys"), Ref("count")))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("groupKey"),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .DeclareVar<EventBean[]>(
                            "eventsPerStream",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("row"), "getArray")));

                    if (forge.OptionalHavingNode != null) {
                        forEach.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockContinue();
                    }

                    forEach.ExprDotMethod(
                        Ref("resultEvents"),
                        "put",
                        Ref("groupKey"),
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR,
                            "process",
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT));

                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("optSortKeys"),
                            "put",
                            Ref("groupKey"),
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT));
                    }

                    forEach.Increment("count");
                }
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedJoinPerKey",
                CodegenNamedParam.From(
                    typeof(ISet<object>),
                    "outputEvents",
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IDictionary<object, object>),
                    "resultEvents",
                    typeof(IDictionary<object, object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorAggregateGrouped),
                classScope,
                code);
        }

        protected internal static void RemovedAggregationGroupKeyCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
                    method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "remove", Ref("key"));
                }

                if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                    method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "remove", Ref("key"));
                }

                if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                    method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "remove", Ref("key"));
                }

                if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                    method.Block.ExprDotMethod(Ref(NAME_OUTPUTFIRSTHELPER), "remove", Ref("key"));
                }
            };
            instance.Methods.AddMethod(
                typeof(void),
                "removedAggregationGroupKey",
                CodegenNamedParam.From(typeof(object), "key"),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        public static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "processView", classScope, method, instance);
        }

        private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            string methodName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var groupKeyTypes = Constant(forge.GroupKeyTypes);

            if (forge.IsOutputAll) {
                CodegenExpression eventTypes = classScope.AddFieldUnshared(
                    true,
                    typeof(EventType[]),
                    EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorAggregateGroupedOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "makeRSAggregateGroupedOutputAll",
                        REF_AGENTINSTANCECONTEXT,
                        Ref("this"),
                        groupKeyTypes,
                        eventTypes));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTALLHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
            else if (forge.IsOutputLast) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorAggregateGroupedOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "makeRSAggregateGroupedOutputLastOpt",
                        REF_AGENTINSTANCECONTEXT,
                        Ref("this"),
                        groupKeyTypes));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTLASTHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "processJoin", classScope, method, instance);
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenMethod method)
        {
            if (forge.IsOutputAll) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "outputView", REF_ISSYNTHESIZE));
            }
            else if (forge.IsOutputLast) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "outputView", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenMethod method)
        {
            if (forge.IsOutputAll) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "outputJoin", REF_ISSYNTHESIZE));
            }
            else if (forge.IsOutputLast) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "outputJoin", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        private static void ProcessOutputLimitedJoinLastCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedJoinPerKey = GenerateOutputBatchedJoinPerKeyCodegen(forge, classScope, instance);

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "lastPerGroupNew",
                    NewInstance(typeof(LinkedHashMap<object, object>)))
                .DeclareVar<IDictionary<object, object>>(
                    "lastPerGroupOld",
                    forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, object>)) : ConstantNull());

            method.Block.DeclareVar<IDictionary<object, object>>("newEventsSortKey", ConstantNull())
                .DeclareVar<IDictionary<object, object>>("oldEventsSortKey", ConstantNull());
            if (forge.IsSorting) {
                method.Block.AssignRef("newEventsSortKey", NewInstance(typeof(LinkedHashMap<object, object>)))
                    .AssignRef(
                        "oldEventsSortKey",
                        forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, object>)) : ConstantNull());
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<object>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<object>>(
                        "newData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<ISet<object>>(
                        "oldData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(generateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(generateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

                forEach.StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    Ref("oldData"),
                    Ref("oldDataMultiKey"));

                // generate old events using select expressions
                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedJoinPerKey,
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("lastPerGroupOld"),
                        Ref("oldEventsSortKey"));
                }

                forEach.InstanceMethod(
                    generateOutputBatchedJoinPerKey,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    ConstantTrue(),
                    REF_ISSYNTHESIZE,
                    Ref("lastPerGroupNew"),
                    Ref("newEventsSortKey"));
            }

            method.Block.DeclareVar<EventBean[]>(
                    "newEventsArr",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS, Ref("lastPerGroupNew")))
                .DeclareVar<EventBean[]>(
                    "oldEventsArr",
                    forge.IsSelectRStream
                        ? StaticMethod(
                            typeof(CollectionUtil),
                            METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS,
                            Ref("lastPerGroupOld"))
                        : ConstantNull());

            if (forge.IsSorting) {
                method.Block.DeclareVar<object[]>(
                        "sortKeysNew",
                        StaticMethod(
                            typeof(CollectionUtil),
                            METHOD_TOARRAYNULLFOREMPTYVALUEVALUES,
                            Ref("newEventsSortKey")))
                    .AssignRef(
                        "newEventsArr",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "sortWOrderKeys",
                            Ref("newEventsArr"),
                            Ref("sortKeysNew"),
                            REF_AGENTINSTANCECONTEXT));
                if (forge.IsSelectRStream) {
                    method.Block.DeclareVar<object[]>(
                            "sortKeysOld",
                            StaticMethod(
                                typeof(CollectionUtil),
                                METHOD_TOARRAYNULLFOREMPTYVALUEVALUES,
                                Ref("oldEventsSortKey")))
                        .AssignRef(
                            "oldEventsArr",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "sortWOrderKeys",
                                Ref("oldEventsArr"),
                                Ref("sortKeysOld"),
                                REF_AGENTINSTANCECONTEXT));
                }
            }

            method.Block.MethodReturn(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_TOPAIRNULLIFALLNULL,
                    Ref("newEventsArr"),
                    Ref("oldEventsArr")));
        }

        private static void ProcessOutputLimitedJoinFirstCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedAddToList = GenerateOutputBatchedAddToListCodegen(forge, classScope, instance);
            var generateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            var helperFactory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var outputFactory = classScope.AddFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            var groupKeyTypes = Constant(forge.GroupKeyTypes);
            instance.AddMember(NAME_OUTPUTFIRSTHELPER, typeof(ResultSetProcessorGroupedOutputFirstHelper));
            instance.ServiceCtor.Block.AssignRef(
                NAME_OUTPUTFIRSTHELPER,
                ExprDotMethod(
                    helperFactory,
                    "makeRSGroupedOutputFirst",
                    REF_AGENTINSTANCECONTEXT,
                    groupKeyTypes,
                    outputFactory,
                    ConstantNull(),
                    Constant(-1)));

            method.Block.DeclareVar<IList<object>>("newEvents", NewInstance(typeof(List<object>)));
            method.Block.DeclareVar<IList<object>>("newEventsSortKey", ConstantNull());
            if (forge.IsSorting) {
                method.Block.AssignRef("newEventsSortKey", NewInstance(typeof(List<object>)));
            }

            method.Block.DeclareVar<IDictionary<object, object>>(
                "workCollection",
                NewInstance(typeof(LinkedHashMap<object, object>)));

            if (forge.OptionalHavingNode == null) {
                {
                    var forEach = method.Block.ForEach(typeof(UniformPair<object>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<object>>(
                            "newData",
                            Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<ISet<object>>(
                            "oldData",
                            Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(generateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(generateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
                            .DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifNewData.ForEach(typeof(MultiKey<object>), "aNewData", Ref("newData"));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            ifPass.ExprDotMethod(Ref("workCollection"), "put", Ref("mk"), Ref("eventsPerStream"));
                            forloop.ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "applyEnter",
                                    Ref("eventsPerStream"),
                                    Ref("mk"),
                                    REF_AGENTINSTANCECONTEXT)
                                .Increment("count");
                        }
                    }
                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
                            .DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifOldData.ForEach(typeof(MultiKey<object>), "aOldData", Ref("oldData"));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aOldData"), "getArray")))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            ifPass.ExprDotMethod(Ref("workCollection"), "put", Ref("mk"), Ref("eventsPerStream"));
                            forloop.ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "applyLeave",
                                    Ref("eventsPerStream"),
                                    Ref("mk"),
                                    REF_AGENTINSTANCECONTEXT)
                                .Increment("count");
                        }
                    }

                    forEach.InstanceMethod(
                        generateOutputBatchedAddToList,
                        Ref("workCollection"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
                }
            }
            else {
                // having clause present, having clause evaluates at the level of individual posts
                {
                    var forEach = method.Block.ForEach(typeof(UniformPair<object>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<object>>(
                            "newData",
                            Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<ISet<object>>(
                            "oldData",
                            Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(generateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(generateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()))
                        .StaticMethod(
                            typeof(ResultSetProcessorGroupedUtil),
                            ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                            REF_AGGREGATIONSVC,
                            REF_AGENTINSTANCECONTEXT,
                            Ref("newData"),
                            Ref("newDataMultiKey"),
                            Ref("oldData"),
                            Ref("oldDataMultiKey"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
                            .DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifNewData.ForEach(typeof(MultiKey<object>), "aNewData", Ref("newData"));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                                .ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                                    ConstantNull())
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("evaluateHavingClause"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_AGENTINSTANCECONTEXT)))
                                .Increment("count")
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            forloop.IfCondition(Ref("pass"))
                                .ExprDotMethod(Ref("workCollection"), "put", Ref("mk"), Ref("eventsPerStream"));
                        }
                    }

                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
                            .DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifOldData.ForEach(typeof(MultiKey<object>), "aOldData", Ref("oldData"));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aOldData"), "getArray")))
                                .ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                                    ConstantNull())
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("evaluateHavingClause"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_AGENTINSTANCECONTEXT)))
                                .Increment("count")
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            forloop.IfCondition(Ref("pass"))
                                .ExprDotMethod(Ref("workCollection"), "put", Ref("mk"), Ref("eventsPerStream"));
                        }
                    }

                    forEach.InstanceMethod(
                        generateOutputBatchedAddToList,
                        Ref("workCollection"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
                }
            }

            method.Block.DeclareVar<EventBean[]>(
                "newEventsArr",
                StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));

            if (forge.IsSorting) {
                method.Block.DeclareVar<object[]>(
                        "sortKeysNew",
                        StaticMethod(
                            typeof(CollectionUtil),
                            METHOD_TOARRAYNULLFOREMPTYOBJECTS,
                            Ref("newEventsSortKey")))
                    .AssignRef(
                        "newEventsArr",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "sortWOrderKeys",
                            Ref("newEventsArr"),
                            Ref("sortKeysNew"),
                            REF_AGENTINSTANCECONTEXT));
            }

            method.Block.MethodReturn(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_TOPAIRNULLIFALLNULL,
                    Ref("newEventsArr"),
                    ConstantNull()));
        }

        private static void ProcessOutputLimitedJoinAllCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedJoinUnkeyed = GenerateOutputBatchedJoinUnkeyedCodegen(forge, classScope, instance);
            var generateOutputBatchedAddToListSingle =
                GenerateOutputBatchedAddToListSingleCodegen(forge, classScope, instance);

            var helperFactory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var groupKeyTypes = Constant(forge.GroupKeyTypes);
            CodegenExpression eventTypes = classScope.AddFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            instance.AddMember(NAME_OUTPUTALLGROUPREPS, typeof(ResultSetProcessorGroupedOutputAllGroupReps));
            instance.ServiceCtor.Block.AssignRef(
                NAME_OUTPUTALLGROUPREPS,
                ExprDotMethod(
                    helperFactory,
                    "makeRSGroupedOutputAllNoOpt",
                    REF_AGENTINSTANCECONTEXT,
                    groupKeyTypes,
                    eventTypes));

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                "workCollection",
                NewInstance(typeof(LinkedHashMap<object, object>)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<object>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<object>>(
                        "newData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<ISet<object>>(
                        "oldData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(generateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(generateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "clear");
                }

                {
                    var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
                        .DeclareVar<int>("count", Constant(0));

                    {
                        ifNewData.ForEach(typeof(MultiKey<object>), "aNewData", Ref("newData"))
                            .DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
                            .DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyEnter",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT)
                            .Increment("count")
                            .ExprDotMethod(Ref("workCollection"), "put", Ref("mk"), Ref("eventsPerStream"))
                            .ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "put", Ref("mk"), Ref("eventsPerStream"));
                    }

                    var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
                        .DeclareVar<int>("count", Constant(0));
                    {
                        ifOldData.ForEach(typeof(MultiKey<object>), "anOldData", Ref("oldData"))
                            .DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
                            .DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT)
                            .Increment("count");
                    }
                }

                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedJoinUnkeyed,
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"));
                }

                forEach.InstanceMethod(
                    generateOutputBatchedJoinUnkeyed,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    ConstantTrue(),
                    REF_ISSYNTHESIZE,
                    Ref("newEvents"),
                    Ref("newEventsSortKey"));
            }

            method.Block.DeclareVar<IEnumerator<object>>(
                "entryIterator",
                ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "entryIterator"));
            {
                method.Block.WhileLoop(ExprDotMethod(Ref("entryIterator"), "hasNext"))
                    .DeclareVar<KeyValuePair<object, object>>(
                        "entry",
                        Cast(typeof(KeyValuePair<object, object>), ExprDotMethod(Ref("entryIterator"), "next")))
                    .IfCondition(
                        Not(ExprDotMethod(Ref("workCollection"), "containsKey", ExprDotMethod(Ref("entry"), "getKey"))))
                    .InstanceMethod(
                        generateOutputBatchedAddToListSingle,
                        ExprDotMethod(Ref("entry"), "getKey"),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
            }

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedJoinDefaultCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedJoinUnkeyed = GenerateOutputBatchedJoinUnkeyedCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<object>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<object>>(
                        "newData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<ISet<object>>(
                        "oldData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(generateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(generateGroupKeyArrayJoin, Ref("oldData"), ConstantFalse()));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "clear");
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    ResultSetProcessorGroupedUtil.METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    Ref("oldData"),
                    Ref("oldDataMultiKey"));

                // generate old events using select expressions
                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedJoinUnkeyed,
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"));
                }

                forEach.InstanceMethod(
                    generateOutputBatchedJoinUnkeyed,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    ConstantTrue(),
                    REF_ISSYNTHESIZE,
                    Ref("newEvents"),
                    Ref("newEventsSortKey"));
            }

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedViewLastCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayView = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayViewCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedViewPerKey = GenerateOutputBatchedViewPerKeyCodegen(forge, classScope, instance);

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "lastPerGroupNew",
                    NewInstance(typeof(LinkedHashMap<object, object>)))
                .DeclareVar<IDictionary<object, object>>(
                    "lastPerGroupOld",
                    forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, object>)) : ConstantNull());

            method.Block.DeclareVar<IDictionary<object, object>>("newEventsSortKey", ConstantNull())
                .DeclareVar<IDictionary<object, object>>("oldEventsSortKey", ConstantNull());
            if (forge.IsSorting) {
                method.Block.AssignRef("newEventsSortKey", NewInstance(typeof(LinkedHashMap<object, object>)))
                    .AssignRef(
                        "oldEventsSortKey",
                        forge.IsSelectRStream ? NewInstance(typeof(LinkedHashMap<object, object>)) : ConstantNull());
            }

            method.Block.DeclareVar<EventBean[]>(
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<object>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(generateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(generateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

                forEach.StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    Ref("oldData"),
                    Ref("oldDataMultiKey"),
                    Ref("eventsPerStream"));

                // generate old events using select expressions
                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedViewPerKey,
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("lastPerGroupOld"),
                        Ref("oldEventsSortKey"),
                        Ref("eventsPerStream"));
                }

                forEach.InstanceMethod(
                    generateOutputBatchedViewPerKey,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    ConstantTrue(),
                    REF_ISSYNTHESIZE,
                    Ref("lastPerGroupNew"),
                    Ref("newEventsSortKey"),
                    Ref("eventsPerStream"));
            }

            method.Block.DeclareVar<EventBean[]>(
                    "newEventsArr",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS, Ref("lastPerGroupNew")))
                .DeclareVar<EventBean[]>(
                    "oldEventsArr",
                    forge.IsSelectRStream
                        ? StaticMethod(
                            typeof(CollectionUtil),
                            METHOD_TOARRAYNULLFOREMPTYVALUEEVENTS,
                            Ref("lastPerGroupOld"))
                        : ConstantNull());

            if (forge.IsSorting) {
                method.Block.DeclareVar<object[]>(
                        "sortKeysNew",
                        StaticMethod(
                            typeof(CollectionUtil),
                            METHOD_TOARRAYNULLFOREMPTYVALUEVALUES,
                            Ref("newEventsSortKey")))
                    .AssignRef(
                        "newEventsArr",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "sortWOrderKeys",
                            Ref("newEventsArr"),
                            Ref("sortKeysNew"),
                            REF_AGENTINSTANCECONTEXT));
                if (forge.IsSelectRStream) {
                    method.Block.DeclareVar<object[]>(
                            "sortKeysOld",
                            StaticMethod(
                                typeof(CollectionUtil),
                                METHOD_TOARRAYNULLFOREMPTYVALUEVALUES,
                                Ref("oldEventsSortKey")))
                        .AssignRef(
                            "oldEventsArr",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "sortWOrderKeys",
                                Ref("oldEventsArr"),
                                Ref("sortKeysOld"),
                                REF_AGENTINSTANCECONTEXT));
                }
            }

            method.Block.MethodReturn(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_TOPAIRNULLIFALLNULL,
                    Ref("newEventsArr"),
                    Ref("oldEventsArr")));
        }

        private static void ProcessOutputLimitedViewFirstCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedAddToList = GenerateOutputBatchedAddToListCodegen(forge, classScope, instance);
            var generateGroupKeyArrayView = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayViewCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            var helperFactory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var outputFactory = classScope.AddFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            var groupKeyTypes = Constant(forge.GroupKeyTypes);
            instance.AddMember(NAME_OUTPUTFIRSTHELPER, typeof(ResultSetProcessorGroupedOutputFirstHelper));
            instance.ServiceCtor.Block.AssignRef(
                NAME_OUTPUTFIRSTHELPER,
                ExprDotMethod(
                    helperFactory,
                    "makeRSGroupedOutputFirst",
                    REF_AGENTINSTANCECONTEXT,
                    groupKeyTypes,
                    outputFactory,
                    ConstantNull(),
                    Constant(-1)));

            method.Block.DeclareVar<IList<object>>("newEvents", NewInstance(typeof(List<object>)));
            method.Block.DeclareVar<IList<object>>("newEventsSortKey", ConstantNull());
            if (forge.IsSorting) {
                method.Block.AssignRef("newEventsSortKey", NewInstance(typeof(List<object>)));
            }

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "workCollection",
                    NewInstance(typeof(LinkedHashMap<object, object>)))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            if (forge.OptionalHavingNode == null) {
                {
                    var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                    forEach.DeclareVar<EventBean[]>(
                            "newData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<EventBean[]>(
                            "oldData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(generateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(generateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForLoopIntSimple("i", ArrayLength(Ref("newData")));
                            forloop.AssignArrayElement(
                                    "eventsPerStream",
                                    Constant(0),
                                    ArrayAtIndex(Ref("newData"), Ref("i")))
                                .DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            ifPass.ExprDotMethod(
                                Ref("workCollection"),
                                "put",
                                Ref("mk"),
                                NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("newData"), Ref("i"))));
                            forloop.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyEnter",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forloop = ifOldData.ForLoopIntSimple("i", ArrayLength(Ref("oldData")));
                            forloop.AssignArrayElement(
                                    "eventsPerStream",
                                    Constant(0),
                                    ArrayAtIndex(Ref("oldData"), Ref("i")))
                                .DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("i")))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            ifPass.ExprDotMethod(
                                Ref("workCollection"),
                                "put",
                                Ref("mk"),
                                NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("oldData"), Ref("i"))));
                            forloop.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }

                    forEach.InstanceMethod(
                        generateOutputBatchedAddToList,
                        Ref("workCollection"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
                }
            }
            else {
                // having clause present, having clause evaluates at the level of individual posts
                {
                    var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                    forEach.DeclareVar<EventBean[]>(
                            "newData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<EventBean[]>(
                            "oldData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(generateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(generateGroupKeyArrayView, Ref("oldData"), ConstantFalse()))
                        .StaticMethod(
                            typeof(ResultSetProcessorGroupedUtil),
                            ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                            REF_AGGREGATIONSVC,
                            REF_AGENTINSTANCECONTEXT,
                            Ref("newData"),
                            Ref("newDataMultiKey"),
                            Ref("oldData"),
                            Ref("oldDataMultiKey"),
                            Ref("eventsPerStream"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForLoopIntSimple("i", ArrayLength(Ref("newData")));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")))
                                .AssignArrayElement(
                                    "eventsPerStream",
                                    Constant(0),
                                    ArrayAtIndex(Ref("newData"), Ref("i")))
                                .ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                                    ConstantNull())
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("evaluateHavingClause"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_AGENTINSTANCECONTEXT)))
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            forloop.IfCondition(Ref("pass"))
                                .ExprDotMethod(
                                    Ref("workCollection"),
                                    "put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("newData"), Ref("i"))));
                        }
                    }

                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forloop = ifOldData.ForLoopIntSimple("i", ArrayLength(Ref("oldData")));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("i")))
                                .AssignArrayElement(
                                    "eventsPerStream",
                                    Constant(0),
                                    ArrayAtIndex(Ref("oldData"), Ref("i")))
                                .ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                                    ConstantNull())
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("evaluateHavingClause"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_AGENTINSTANCECONTEXT)))
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Ref(NAME_OUTPUTFIRSTHELPER),
                                        "getOrAllocate",
                                        Ref("mk"),
                                        REF_AGENTINSTANCECONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "updateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            forloop.IfCondition(Ref("pass"))
                                .ExprDotMethod(
                                    Ref("workCollection"),
                                    "put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("oldData"), Ref("i"))));
                        }
                    }

                    forEach.InstanceMethod(
                        generateOutputBatchedAddToList,
                        Ref("workCollection"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
                }
            }

            method.Block.DeclareVar<EventBean[]>(
                "newEventsArr",
                StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));

            if (forge.IsSorting) {
                method.Block.DeclareVar<object[]>(
                        "sortKeysNew",
                        StaticMethod(
                            typeof(CollectionUtil),
                            METHOD_TOARRAYNULLFOREMPTYOBJECTS,
                            Ref("newEventsSortKey")))
                    .AssignRef(
                        "newEventsArr",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "sortWOrderKeys",
                            Ref("newEventsArr"),
                            Ref("sortKeysNew"),
                            REF_AGENTINSTANCECONTEXT));
            }

            method.Block.MethodReturn(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_TOPAIRNULLIFALLNULL,
                    Ref("newEventsArr"),
                    ConstantNull()));
        }

        private static void ProcessOutputLimitedViewAllCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayView = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayViewCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedViewUnkeyed = GenerateOutputBatchedViewUnkeyedCodegen(forge, classScope, instance);
            var generateOutputBatchedAddToListSingle =
                GenerateOutputBatchedAddToListSingleCodegen(forge, classScope, instance);

            var helperFactory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var groupKeyTypes = Constant(forge.GroupKeyTypes);
            CodegenExpression eventTypes = classScope.AddFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            instance.AddMember(NAME_OUTPUTALLGROUPREPS, typeof(ResultSetProcessorGroupedOutputAllGroupReps));
            instance.ServiceCtor.Block.AssignRef(
                NAME_OUTPUTALLGROUPREPS,
                ExprDotMethod(
                    helperFactory,
                    "makeRSGroupedOutputAllNoOpt",
                    REF_AGENTINSTANCECONTEXT,
                    groupKeyTypes,
                    eventTypes));

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "workCollection",
                    NewInstance(typeof(LinkedHashMap<object, object>)))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(generateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(generateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

                {
                    var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")))
                        .DeclareVar<int>("count", Constant(0));

                    {
                        ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                            .DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
                            .AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("aNewData"))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyEnter",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT)
                            .Increment("count")
                            .ExprDotMethod(Ref("workCollection"), "put", Ref("mk"), Ref("eventsPerStream"))
                            .ExprDotMethod(
                                Ref(NAME_OUTPUTALLGROUPREPS),
                                "put",
                                Ref("mk"),
                                NewArrayWithInit(typeof(EventBean), Ref("aNewData")));
                    }

                    var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
                        .DeclareVar<int>("count", Constant(0));
                    {
                        ifOldData.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                            .DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
                            .AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("anOldData"))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT)
                            .Increment("count");
                    }
                }

                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedViewUnkeyed,
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"),
                        Ref("eventsPerStream"));
                }

                forEach.InstanceMethod(
                    generateOutputBatchedViewUnkeyed,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    ConstantTrue(),
                    REF_ISSYNTHESIZE,
                    Ref("newEvents"),
                    Ref("newEventsSortKey"),
                    Ref("eventsPerStream"));
            }

            method.Block.DeclareVar<IEnumerator<object>>(
                "entryIterator",
                ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "entryIterator"));
            {
                method.Block.WhileLoop(ExprDotMethod(Ref("entryIterator"), "hasNext"))
                    .DeclareVar<KeyValuePair<object, object>>(
                        "entry",
                        Cast(typeof(KeyValuePair<object, object>), ExprDotMethod(Ref("entryIterator"), "next")))
                    .IfCondition(
                        Not(ExprDotMethod(Ref("workCollection"), "containsKey", ExprDotMethod(Ref("entry"), "getKey"))))
                    .InstanceMethod(
                        generateOutputBatchedAddToListSingle,
                        ExprDotMethod(Ref("entry"), "getKey"),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
            }

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedViewDefaultCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayView = ResultSetProcessorGroupedUtil.GenerateGroupKeyArrayViewCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedViewUnkeyed = GenerateOutputBatchedViewUnkeyedCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<EventBean[]>(
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(generateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(generateGroupKeyArrayView, Ref("oldData"), ConstantFalse()));

                forEach.StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    ResultSetProcessorGroupedUtil.METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    Ref("oldData"),
                    Ref("oldDataMultiKey"),
                    Ref("eventsPerStream"));

                // generate old events using select expressions
                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedViewUnkeyed,
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"),
                        Ref("eventsPerStream"));
                }

                forEach.InstanceMethod(
                    generateOutputBatchedViewUnkeyed,
                    Ref("newData"),
                    Ref("newDataMultiKey"),
                    ConstantTrue(),
                    REF_ISSYNTHESIZE,
                    Ref("newEvents"),
                    Ref("newEventsSortKey"),
                    Ref("eventsPerStream"));
            }

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static CodegenMethod GenerateOutputBatchedAddToListCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedAddToListSingle =
                GenerateOutputBatchedAddToListSingleCodegen(forge, classScope, instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ForEach(
                        typeof(KeyValuePair<object, object>),
                        "entry",
                        ExprDotMethod(Ref("keysAndEvents"), "entrySet"))
                    .InstanceMethod(
                        generateOutputBatchedAddToListSingle,
                        ExprDotMethod(Ref("entry"), "getKey"),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")),
                        REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        Ref("resultEvents"),
                        Ref("optSortKeys"));
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedAddToList",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>),
                    "keysAndEvents",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorAggregateGroupedImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateOutputBatchedAddToListSingleCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                {
                    methodNode.Block.ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "SetCurrentAccess",
                        Ref("key"),
                        ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                        ConstantNull());

                    if (forge.OptionalHavingNode != null) {
                        methodNode.Block.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockReturnNoValue();
                    }

                    methodNode.Block.ExprDotMethod(
                        Ref("resultEvents"),
                        "add",
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR,
                            "process",
                            ExprForgeCodegenNames.REF_EPS,
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT));

                    if (forge.IsSorting) {
                        methodNode.Block.ExprDotMethod(
                            Ref("optSortKeys"),
                            "add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                ExprForgeCodegenNames.REF_EPS,
                                REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT));
                    }
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedAddToListSingle",
                CodegenNamedParam.From(
                    typeof(object),
                    "key",
                    typeof(EventBean[]),
                    "eventsPerStream",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorAggregateGroupedImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedViewUnkeyedCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(EqualsNull(Ref("outputEvents")))
                    .BlockReturnNoValue()
                    .DeclareVar<int>("count", Constant(0));

                {
                    var forEach = methodNode.Block.ForEach(typeof(EventBean), "outputEvent", Ref("outputEvents"));
                    forEach.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ArrayAtIndex(Ref("groupByKeys"), Ref("count")),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .AssignArrayElement(
                            Ref("eventsPerStream"),
                            Constant(0),
                            ArrayAtIndex(Ref("outputEvents"), Ref("count")));

                    if (forge.OptionalHavingNode != null) {
                        forEach.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        Ref("eventsPerStream"),
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .Increment("count")
                            .BlockContinue();
                    }

                    forEach.ExprDotMethod(
                        Ref("resultEvents"),
                        "add",
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR,
                            "process",
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT));

                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("optSortKeys"),
                            "add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT));
                    }

                    forEach.Increment("count");
                }
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedViewUnkeyed",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "outputEvents",
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(ICollection<object>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys",
                    typeof(EventBean[]),
                    "eventsPerStream"),
                typeof(ResultSetProcessorAggregateGrouped),
                classScope,
                code);
        }

        private static CodegenMethod ProcessViewResultPairDepthOneCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var shortcutEvalGivenKey = ShortcutEvalGivenKeyCodegen(forge.OptionalHavingNode, classScope, instance);
            var generateGroupKeySingle =
                ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                    forge.GroupKeyNodeExpressions,
                    classScope,
                    instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<object>(
                        "newGroupKey",
                        LocalMethod(generateGroupKeySingle, REF_NEWDATA, ConstantTrue()))
                    .DeclareVar<object>(
                        "oldGroupKey",
                        LocalMethod(generateGroupKeySingle, REF_OLDDATA, ConstantFalse()))
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "applyEnter",
                        REF_NEWDATA,
                        Ref("newGroupKey"),
                        REF_AGENTINSTANCECONTEXT)
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "applyLeave",
                        REF_OLDDATA,
                        Ref("oldGroupKey"),
                        REF_AGENTINSTANCECONTEXT)
                    .DeclareVar<EventBean>(
                        "istream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("newGroupKey"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE));
                if (!forge.IsSelectRStream) {
                    methodNode.Block.MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "toPairNullIfNullIStream", Ref("istream")));
                }
                else {
                    methodNode.Block.DeclareVar<EventBean>(
                            "rstream",
                            LocalMethod(
                                shortcutEvalGivenKey,
                                REF_OLDDATA,
                                Ref("oldGroupKey"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE))
                        .MethodReturn(
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                "toPairNullIfAllNullSingle",
                                Ref("istream"),
                                Ref("rstream")));
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "processViewResultPairDepthOne",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    NAME_NEWDATA,
                    typeof(EventBean[]),
                    NAME_OLDDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        private static CodegenMethod ProcessViewResultNewDepthOneCodegen(
            ResultSetProcessorAggregateGroupedForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var shortcutEvalGivenKey = ShortcutEvalGivenKeyCodegen(forge.OptionalHavingNode, classScope, instance);
            var generateGroupKeySingle =
                ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                    forge.GroupKeyNodeExpressions,
                    classScope,
                    instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .DeclareVar<object>(
                        "groupKey",
                        LocalMethod(generateGroupKeySingle, REF_NEWDATA, ConstantTrue()))
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "applyEnter",
                        REF_NEWDATA,
                        Ref("groupKey"),
                        REF_AGENTINSTANCECONTEXT)
                    .DeclareVar<EventBean>(
                        "istream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("groupKey"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE))
                    .MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "toPairNullIfNullIStream", Ref("istream")));
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "processViewResultNewDepthOneCodegen",
                CodegenNamedParam.From(typeof(EventBean[]), NAME_NEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        private static CodegenMethod ShortcutEvalGivenKeyCodegen(
            ExprForge optionalHavingNode,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("groupKey"),
                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                    ConstantNull());
                if (optionalHavingNode != null) {
                    methodNode.Block
                        .IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                methodNode.Block.MethodReturn(
                    ExprDotMethod(
                        REF_SELECTEXPRPROCESSOR,
                        "process",
                        ExprForgeCodegenNames.REF_EPS,
                        REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        REF_AGENTINSTANCECONTEXT));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean),
                "shortcutEvalGivenKey",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS,
                    typeof(object),
                    "groupKey",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }
    }
} // end of namespace