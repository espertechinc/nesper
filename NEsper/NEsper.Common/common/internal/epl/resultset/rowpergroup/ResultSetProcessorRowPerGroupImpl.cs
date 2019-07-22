///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    /// <summary>
    ///     Result set processor for the fully-grouped case:
    ///     there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
    ///     and there are aggregation functions.
    ///     <para />
    ///     Produces one row for each group that changed (and not one row per event). Computes MultiKey group-by keys for
    ///     each event and uses a set of the group-by keys to generate the result rows, using the first (old or new, anyone)
    ///     event
    ///     for each distinct group-by key.
    /// </summary>
    public class ResultSetProcessorRowPerGroupImpl
    {
        private const string NAME_OUTPUTALLHELPER = "outputAllHelper";
        private const string NAME_OUTPUTLASTHELPER = "outputLastHelper";
        private const string NAME_OUTPUTFIRSTHELPER = "outputFirstHelper";
        private const string NAME_OUTPUTALLGROUPREPS = "outputAllGroupReps";

        public static void ApplyViewResultCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
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
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
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
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin =
                GenerateGroupKeyArrayJoinTakingMapCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, object>)))
                .DeclareVar<object[]>(
                    "newDataMultiKey",
                    LocalMethod(generateGroupKeyArrayJoin, REF_NEWDATA, Ref("keysAndEvents"), ConstantTrue()))
                .DeclareVar<object[]>(
                    "oldDataMultiKey",
                    LocalMethod(generateGroupKeyArrayJoin, REF_OLDDATA, Ref("keysAndEvents"), ConstantFalse()));

            if (forge.IsUnidirectional) {
                method.Block.ExprDotMethod(Ref("this"), "clear");
            }

            method.Block.DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsJoin,
                            Ref("keysAndEvents"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE)
                        : ConstantNull())
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    REF_NEWDATA,
                    Ref("newDataMultiKey"),
                    REF_OLDDATA,
                    Ref("oldDataMultiKey"))
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(generateOutputEventsJoin, Ref("keysAndEvents"), ConstantTrue(), REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        public static void ProcessViewResultCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysKeepEvent = GenerateGroupKeysKeepEventCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);
            var processViewResultNewDepthOne =
                ProcessViewResultNewDepthOneCodegen(forge, classScope, instance);
            var processViewResultPairDepthOneNoRStream =
                ProcessViewResultPairDepthOneNoRStreamCodegen(forge, classScope, instance);

            var ifShortcut = method.Block.IfCondition(
                And(NotEqualsNull(REF_NEWDATA), EqualsIdentity(ArrayLength(REF_NEWDATA), Constant(1))));
            ifShortcut.IfCondition(Or(EqualsNull(REF_OLDDATA), EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(0))))
                .BlockReturn(LocalMethod(processViewResultNewDepthOne, REF_NEWDATA, REF_ISSYNTHESIZE));
            if (!forge.IsSelectRStream) {
                ifShortcut.IfCondition(EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(1)))
                    .BlockReturn(
                        LocalMethod(
                            processViewResultPairDepthOneNoRStream,
                            REF_NEWDATA,
                            REF_OLDDATA,
                            REF_ISSYNTHESIZE));
            }

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, object>)))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar<object[]>(
                    "newDataMultiKey",
                    LocalMethod(
                        generateGroupKeysKeepEvent,
                        REF_NEWDATA,
                        Ref("keysAndEvents"),
                        ConstantTrue(),
                        Ref("eventsPerStream")))
                .DeclareVar<object[]>(
                    "oldDataMultiKey",
                    LocalMethod(
                        generateGroupKeysKeepEvent,
                        REF_OLDDATA,
                        Ref("keysAndEvents"),
                        ConstantFalse(),
                        Ref("eventsPerStream")))
                .DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView,
                            Ref("keysAndEvents"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE,
                            Ref("eventsPerStream"))
                        : ConstantNull())
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
                    REF_NEWDATA,
                    Ref("newDataMultiKey"),
                    REF_OLDDATA,
                    Ref("oldDataMultiKey"),
                    Ref("eventsPerStream"))
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsView,
                        Ref("keysAndEvents"),
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

        protected internal static CodegenMethod GenerateOutputEventsViewCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<EventBean[]>(
                        "events",
                        NewArrayByLength(typeof(EventBean), ExprDotMethod(Ref("keysAndEvents"), "size")))
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ExprDotMethod(Ref("keysAndEvents"), "size")));

                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar<EventBean[][]>(
                        "currentGenerators",
                        NewArrayByLength(typeof(EventBean[]), ExprDotMethod(Ref("keysAndEvents"), "size")));
                }

                methodNode.Block.DeclareVar<int>("count", Constant(0))
                    .DeclareVar<int>("cpid", ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"));

                {
                    var forEach = methodNode.Block.ForEach(
                        typeof(KeyValuePair<object, object>),
                        "entry",
                        ExprDotMethod(Ref("keysAndEvents"), "entrySet"));
                    forEach.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ExprDotMethod(Ref("entry"), "getKey"),
                            Ref("cpid"),
                            ConstantNull())
                        .AssignArrayElement(
                            ExprForgeCodegenNames.REF_EPS,
                            Constant(0),
                            Cast(typeof(EventBean), ExprDotMethod(Ref("entry"), "getValue")));

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

                    forEach.AssignArrayElement(
                            "events",
                            Ref("count"),
                            ExprDotMethod(
                                REF_SELECTEXPRPROCESSOR,
                                "process",
                                ExprForgeCodegenNames.REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT))
                        .AssignArrayElement("keys", Ref("count"), ExprDotMethod(Ref("entry"), "getKey"));

                    if (forge.IsSorting) {
                        forEach.AssignArrayElement(
                            "currentGenerators",
                            Ref("count"),
                            NewArrayWithInit(
                                typeof(EventBean),
                                Cast(typeof(EventBean), ExprDotMethod(Ref("entry"), "getValue"))));
                    }

                    forEach.Increment("count")
                        .BlockEnd();
                }

                OutputFromCountMaySortCodegen(
                    methodNode.Block,
                    Ref("count"),
                    Ref("events"),
                    Ref("keys"),
                    Ref("currentGenerators"),
                    forge.IsSorting);
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "generateOutputEventsView",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>),
                    "keysAndEvents",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedRowFromMapCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block.DeclareVar<EventBean[]>(
                    "eventsPerStream",
                    NewArrayByLength(typeof(EventBean), Constant(1)));
                {
                    var forLoop = method.Block.ForEach(
                        typeof(KeyValuePair<object, object>),
                        "entry",
                        ExprDotMethod(Ref("keysAndEvents"), "entrySet"));
                    forLoop.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ExprDotMethod(Ref("entry"), "getKey"),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .AssignArrayElement(
                            "eventsPerStream",
                            Constant(0),
                            Cast(typeof(EventBean), ExprDotMethod(Ref("entry"), "getValue")));

                    if (forge.OptionalHavingNode != null) {
                        forLoop.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        Ref("eventsPerStream"),
                                        REF_ISNEWDATA,
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockContinue();
                    }

                    forLoop.ExprDotMethod(
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
                        forLoop.ExprDotMethod(
                            Ref("optSortKeys"),
                            "add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT));
                    }
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedRowFromMap",
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
                    "optSortKeys",
                    typeof(AgentInstanceContext),
                    NAME_AGENTINSTANCECONTEXT),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedArrFromIteratorCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);

            Consumer<CodegenMethod> code = method => method.Block
                .WhileLoop(ExprDotMethod(Ref("keysAndEvents"), "hasNext"))
                .DeclareVar<KeyValuePair<object, object>>(
                    "entry",
                    Cast(typeof(KeyValuePair<object, object>), ExprDotMethod(Ref("keysAndEvents"), "next")))
                .InstanceMethod(
                    generateOutputBatchedRowAddToList,
                    Ref("join"),
                    ExprDotMethod(Ref("entry"), "getKey"),
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")),
                    REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    Ref("resultEvents"),
                    Ref("optSortKeys"));

            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedArrFromIterator",
                CodegenNamedParam.From(
                    typeof(bool),
                    "join",
                    typeof(IEnumerator),
                    "keysAndEvents",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateOutputBatchedRowAddToListCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                    ConstantNull());

                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturnNoValue();
                }

                method.Block.ExprDotMethod(
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
                    method.Block.ExprDotMethod(
                        Ref("optSortKeys"),
                        "add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "getSortKey",
                            ExprForgeCodegenNames.REF_EPS,
                            REF_ISNEWDATA,
                            REF_AGENTINSTANCECONTEXT));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "generateOutputBatchedRowAddToList",
                CodegenNamedParam.From(
                    typeof(bool),
                    "join",
                    typeof(object),
                    "mk",
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedNoSortWMapCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
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
                "generateOutputBatchedNoSortWMap",
                CodegenNamedParam.From(
                    typeof(bool),
                    "join",
                    typeof(object),
                    "mk",
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateOutputEventsJoinCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<EventBean[]>(
                        "events",
                        NewArrayByLength(typeof(EventBean), ExprDotMethod(Ref("keysAndEvents"), "size")))
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ExprDotMethod(Ref("keysAndEvents"), "size")));

                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar<EventBean[][]>(
                        "currentGenerators",
                        NewArrayByLength(typeof(EventBean[]), ExprDotMethod(Ref("keysAndEvents"), "size")));
                }

                methodNode.Block.DeclareVar<int>("count", Constant(0))
                    .DeclareVar<int>("cpid", ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"));

                {
                    var forEach = methodNode.Block.ForEach(
                        typeof(KeyValuePair<object, object>),
                        "entry",
                        ExprDotMethod(Ref("keysAndEvents"), "entrySet"));
                    forEach.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ExprDotMethod(Ref("entry"), "getKey"),
                            Ref("cpid"),
                            ConstantNull())
                        .DeclareVar<EventBean[]>(
                            ExprForgeCodegenNames.NAME_EPS,
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")));

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

                    forEach.AssignArrayElement(
                            "events",
                            Ref("count"),
                            ExprDotMethod(
                                REF_SELECTEXPRPROCESSOR,
                                "process",
                                ExprForgeCodegenNames.REF_EPS,
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT))
                        .AssignArrayElement("keys", Ref("count"), ExprDotMethod(Ref("entry"), "getKey"));

                    if (forge.IsSorting) {
                        forEach.AssignArrayElement("currentGenerators", Ref("count"), Ref("eventsPerStream"));
                    }

                    forEach.Increment("count")
                        .BlockEnd();
                }

                OutputFromCountMaySortCodegen(
                    methodNode.Block,
                    Ref("count"),
                    Ref("events"),
                    Ref("keys"),
                    Ref("currentGenerators"),
                    forge.IsSorting);
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "generateOutputEventsJoin",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>),
                    "keysAndEvents",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateGroupKeysKeepEventCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var key = GenerateGroupKeySingleCodegen(forge.GroupKeyNodeExpressions, classScope, instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNullReturnNull("events")
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ArrayLength(Ref("events"))));
                {
                    methodNode.Block.ForLoopIntSimple("i", ArrayLength(Ref("events")))
                        .AssignArrayElement(
                            ExprForgeCodegenNames.REF_EPS,
                            Constant(0),
                            ArrayAtIndex(Ref("events"), Ref("i")))
                        .AssignArrayElement(
                            "keys",
                            Ref("i"),
                            LocalMethod(key, ExprForgeCodegenNames.REF_EPS, REF_ISNEWDATA))
                        .ExprDotMethod(
                            Ref("eventPerKey"),
                            "put",
                            ArrayAtIndex(Ref("keys"), Ref("i")),
                            ArrayAtIndex(Ref("events"), Ref("i")))
                        .BlockEnd();
                }
                methodNode.Block.MethodReturn(Ref("keys"));
            };

            return instance.Methods.AddMethod(
                typeof(object[]),
                "generateGroupKeysKeepEvent",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "events",
                    typeof(IDictionary<object, object>),
                    "eventPerKey",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateGroupKeyArrayJoinTakingMapCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var key = GenerateGroupKeySingleCodegen(forge.GroupKeyNodeExpressions, classScope, instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfCondition(Or(EqualsNull(Ref("resultSet")), ExprDotMethod(Ref("resultSet"), "isEmpty")))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ExprDotMethod(Ref("resultSet"), "size")))
                    .DeclareVar<int>("count", Constant(0));
                {
                    methodNode.Block.ForEach(typeof(MultiKey<EventBean>), "eventsPerStream", Ref("resultSet"))
                        .DeclareVar<EventBean[]>(
                            "eps",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("eventsPerStream"), "getArray")))
                        .AssignArrayElement("keys", Ref("count"), LocalMethod(key, Ref("eps"), REF_ISNEWDATA))
                        .ExprDotMethod(
                            Ref("eventPerKey"),
                            "put",
                            ArrayAtIndex(Ref("keys"), Ref("count")),
                            Ref("eps"))
                        .Increment("count")
                        .BlockEnd();
                }
                methodNode.Block.MethodReturn(Ref("keys"));
            };

            return instance.Methods.AddMethod(
                typeof(object[]),
                "generateGroupKeyArrayJoinTakingMapCodegen",
                CodegenNamedParam.From(
                    typeof(ISet<EventBean>),
                    "resultSet",
                    typeof(IDictionary<object, object>),
                    "eventPerKey",
                    typeof(bool),
                    NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        public static void GetIteratorViewCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsHistoricalOnly) {
                method.Block.MethodReturn(
                    LocalMethod(ObtainIteratorCodegen(forge, classScope, method, instance), REF_VIEWABLE));
                return;
            }

            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT)
                .DeclareVar<IEnumerator>("it", ExprDotMethod(REF_VIEWABLE, "iterator"))
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

            method.Block.DeclareVar<ArrayDeque<EventBean>>(
                    "deque",
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ITERATORTODEQUE,
                        LocalMethod(ObtainIteratorCodegen(forge, classScope, method, instance), REF_VIEWABLE)))
                .ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "iterator"));
        }

        private static CodegenMethod ObtainIteratorCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent,
            CodegenInstanceAux instance)
        {
            var iterator = parent
                .MakeChild(typeof(IEnumerator<EventBean>), typeof(ResultSetProcessorRowPerGroupImpl), classScope)
                .AddParam(typeof(Viewable), NAME_VIEWABLE);
            if (!forge.IsSorting) {
                iterator.Block.MethodReturn(
                    NewInstance<ResultSetProcessorRowPerGroupEnumerator>(
                        ExprDotMethod(REF_VIEWABLE, "iterator"),
                        Ref("this"),
                        REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT));
                return iterator;
            }

            var getIteratorSorted = GetIteratorSortedCodegen(forge, classScope, instance);
            iterator.Block.MethodReturn(LocalMethod(getIteratorSorted, ExprDotMethod(REF_VIEWABLE, "iterator")));
            return iterator;
        }

        protected internal static CodegenMethod GetIteratorSortedCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyViewSingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            Consumer<CodegenMethod> code = method => {
                method.Block.DeclareVar<EventBean[]>(
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar<List<object>>("outgoingEvents", NewInstance(typeof(List<object>)))
                    .DeclareVar<List<object>>("orderKeys", NewInstance(typeof(List<object>)))
                    .DeclareVar<ISet<EventBean>>("priorSeenGroups", NewInstance(typeof(HashSet<object>)));

                {
                    var whileLoop = method.Block.WhileLoop(ExprDotMethod(Ref("parentIter"), "hasNext"));
                    whileLoop.DeclareVar<EventBean>(
                            "candidate",
                            Cast(typeof(EventBean), ExprDotMethod(Ref("parentIter"), "next")))
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("candidate"))
                        .DeclareVar<object>(
                            "groupKey",
                            LocalMethod(generateGroupKeyViewSingle, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("groupKey"),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull());

                    if (forge.OptionalHavingNode != null) {
                        whileLoop.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("evaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        ConstantTrue(),
                                        REF_AGENTINSTANCECONTEXT)))
                            .BlockContinue();
                    }

                    whileLoop.IfCondition(ExprDotMethod(Ref("priorSeenGroups"), "contains", Ref("groupKey")))
                        .BlockContinue();

                    whileLoop.ExprDotMethod(Ref("priorSeenGroups"), "add", Ref("groupKey"))
                        .ExprDotMethod(
                            Ref("outgoingEvents"),
                            "add",
                            ExprDotMethod(
                                REF_SELECTEXPRPROCESSOR,
                                "process",
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                ConstantTrue(),
                                REF_AGENTINSTANCECONTEXT))
                        .DeclareVar<object>(
                            "orderKey",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                REF_AGENTINSTANCECONTEXT))
                        .ExprDotMethod(Ref("orderKeys"), "add", Ref("orderKey"));
                }

                method.Block.MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ORDEROUTGOINGGETITERATOR,
                        Ref("outgoingEvents"),
                        Ref("orderKeys"),
                        REF_ORDERBYPROCESSOR,
                        REF_AGENTINSTANCECONTEXT));
            };
            return instance.Methods.AddMethod(
                typeof(IEnumerator),
                "getIteratorSorted",
                CodegenNamedParam.From(typeof(IEnumerator), "parentIter"),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        public static void GetIteratorJoinCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin =
                GenerateGroupKeyArrayJoinTakingMapCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);
            method.Block
                .DeclareVar<IDictionary<object, object>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, object>)))
                .Expression(LocalMethod(generateGroupKeyArrayJoin, REF_JOINSET, Ref("keysAndEvents"), ConstantTrue()))
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(generateOutputEventsJoin, Ref("keysAndEvents"), ConstantTrue(), ConstantTrue()))
                .MethodReturn(NewInstance<ArrayEventEnumerator>(Ref("selectNewEvents")));
        }

        public static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT);
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                ProcessOutputLimitedJoinDefaultCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                ProcessOutputLimitedJoinAllCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                ProcessOutputLimitedJoinFirstCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.LAST) {
                ProcessOutputLimitedJoinLastCodegen(forge, classScope, method, instance);
                return;
            }

            throw new IllegalStateException("Unrecognized output limit type " + outputLimitLimitType);
        }

        public static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                ProcessOutputLimitedViewDefaultCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                ProcessOutputLimitedViewAllCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                ProcessOutputLimitedViewFirstCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.LAST) {
                ProcessOutputLimitedViewLastCodegen(forge, classScope, method, instance);
                return;
            }

            throw new IllegalStateException("Unrecognized output limit type " + outputLimitLimitType);
        }

        protected internal static void RemovedAggregationGroupKeyCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
                    method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "remove", Ref("key"));
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
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "processView", classScope, method, instance);
        }

        private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            string methodName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory =
                classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            CodegenExpression eventTypes = classScope.AddFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));

            if (forge.IsOutputAll) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowPerGroupOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "makeRSRowPerGroupOutputAllOpt",
                        REF_AGENTINSTANCECONTEXT,
                        Ref("this"),
                        Constant(forge.GroupKeyTypes),
                        eventTypes));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTALLHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
            else if (forge.IsOutputLast) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowPerGroupOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "makeRSRowPerGroupOutputLastOpt",
                        REF_AGENTINSTANCECONTEXT,
                        Ref("this"),
                        Constant(forge.GroupKeyTypes),
                        eventTypes));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTLASTHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "processJoin", classScope, method, instance);
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupForge forge,
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
            ResultSetProcessorRowPerGroupForge forge,
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

        public static void StopMethodCodegenBound(
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

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTALLGROUPREPS));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTALLHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTFIRSTHELPER));
            }
        }

        private static void ProcessOutputLimitedJoinLastCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                "groupRepsView",
                NewInstance(typeof(Dictionary<object, object>)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<EventBean>>(
                        "newData",
                        Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<ISet<EventBean>>(
                        "oldData",
                        Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "clear");
                }

                {
                    var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(
                            typeof(MultiKey<EventBean>),
                            "aNewData",
                            Ref("newData"));
                        forNew.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forNew.ExprDotMethod(
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
                        var forOld = ifOldData.ForEach(
                            typeof(MultiKey<EventBean>),
                            "anOldData",
                            Ref("oldData"));
                        forOld.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "applyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            REF_AGENTINSTANCECONTEXT);
                    }
                }
            }

            method.Block.InstanceMethod(
                generateOutputBatchedArrFromIterator,
                ConstantTrue(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("entrySet").Add("iterator"),
                ConstantTrue(),
                REF_ISSYNTHESIZE,
                Ref("newEvents"),
                Ref("newEventsSortKey"));

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedJoinFirstCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateGroupKeyArrayJoin = GenerateGroupKeyArrayJoinCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
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

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                "groupRepsView",
                NewInstance(typeof(LinkedHashMap<object, object>)));

            if (forge.OptionalHavingNode == null) {
                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<EventBean>),
                        "pair",
                        REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<EventBean>>(
                            "newData",
                            Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<ISet<EventBean>>(
                            "oldData",
                            Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForEach(
                                typeof(MultiKey<EventBean>),
                                "aNewData",
                                Ref("newData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
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
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantTrue(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

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
                            var forloop = ifOldData.ForEach(
                                typeof(MultiKey<EventBean>),
                                "anOldData",
                                Ref("oldData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
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
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantFalse(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

                            forloop.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                }
            }
            else {
                method.Block.ExprDotMethod(Ref("groupRepsView"), "clear");
                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<EventBean>),
                        "pair",
                        REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<EventBean>>(
                            "newData",
                            Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<ISet<EventBean>>(
                            "oldData",
                            Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(generateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(generateGroupKeyArrayJoin, Ref("oldData"), ConstantTrue()))
                        .StaticMethod(
                            typeof(ResultSetProcessorGroupedUtil),
                            METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                            REF_AGGREGATIONSVC,
                            REF_AGENTINSTANCECONTEXT,
                            Ref("newData"),
                            Ref("newDataMultiKey"),
                            Ref("oldData"),
                            Ref("oldDataMultiKey"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        ifNewData.DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifNewData.ForEach(
                                typeof(MultiKey<EventBean>),
                                "aNewData",
                                Ref("newData"));
                            forloop.DeclareVar<object>(
                                    "mk",
                                    ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
                                .ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                                    ConstantNull())
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
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
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantTrue(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

                            forloop.Increment("count");
                        }
                    }

                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
                            .DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifOldData.ForEach(
                                typeof(MultiKey<EventBean>),
                                "anOldData",
                                Ref("oldData"));
                            forloop.DeclareVar<object>(
                                    "mk",
                                    ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
                                .ExprDotMethod(
                                    REF_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                                    ConstantNull())
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
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
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantFalse(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

                            forloop.Increment("count");
                        }
                    }
                }
            }

            method.Block.InstanceMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("entrySet").Add("iterator"),
                ConstantTrue(),
                REF_ISSYNTHESIZE,
                Ref("newEvents"),
                Ref("newEventsSortKey"));

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedJoinAllCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
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
                    Constant(forge.GroupKeyTypes),
                    eventTypes));

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            if (forge.IsSelectRStream) {
                method.Block.InstanceMethod(
                    generateOutputBatchedArrFromIterator,
                    ConstantTrue(),
                    ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "entryIterator"),
                    ConstantFalse(),
                    REF_ISSYNTHESIZE,
                    Ref("oldEvents"),
                    Ref("oldEventsSortKey"));
            }

            {
                var forLoop = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_JOINEVENTSSET);
                forLoop.DeclareVar<ISet<EventBean>>(
                        "newData",
                        Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<ISet<EventBean>>(
                        "oldData",
                        Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")));

                if (forge.IsUnidirectional) {
                    forLoop.ExprDotMethod(Ref("this"), "clear");
                }

                {
                    var ifNewData = forLoop.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(
                            typeof(MultiKey<EventBean>),
                            "aNewData",
                            Ref("newData"));
                        forNew.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Ref(NAME_OUTPUTALLGROUPREPS),
                                    "put",
                                    Ref("mk"),
                                    Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "applyEnter",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            REF_AGENTINSTANCECONTEXT);
                    }
                }

                {
                    var ifOldData = forLoop.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOldData.ForEach(
                            typeof(MultiKey<EventBean>),
                            "anOldData",
                            Ref("oldData"));
                        forOld.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Ref(NAME_OUTPUTALLGROUPREPS),
                                    "put",
                                    Ref("mk"),
                                    Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "applyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            REF_AGENTINSTANCECONTEXT);
                    }
                }
            }

            method.Block.InstanceMethod(
                generateOutputBatchedArrFromIterator,
                ConstantTrue(),
                ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "entryIterator"),
                ConstantTrue(),
                REF_ISSYNTHESIZE,
                Ref("newEvents"),
                Ref("newEventsSortKey"));

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
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoinTakingMap =
                GenerateGroupKeyArrayJoinTakingMapCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                "keysAndEvents",
                NewInstance(typeof(Dictionary<object, object>)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<EventBean>>(
                        "newData",
                        Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<ISet<EventBean>>(
                        "oldData",
                        Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "clear");
                }

                forEach.DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeyArrayJoinTakingMap,
                            Ref("newData"),
                            Ref("keysAndEvents"),
                            ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeyArrayJoinTakingMap,
                            Ref("oldData"),
                            Ref("keysAndEvents"),
                            ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedArrFromIterator,
                        ConstantTrue(),
                        ExprDotMethodChain(Ref("keysAndEvents")).Add("entrySet").Add("iterator"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"));
                }

                forEach.StaticMethod(
                        typeof(ResultSetProcessorGroupedUtil),
                        METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                        REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT,
                        Ref("newData"),
                        Ref("newDataMultiKey"),
                        Ref("oldData"),
                        Ref("oldDataMultiKey"))
                    .InstanceMethod(
                        generateOutputBatchedArrFromIterator,
                        ConstantTrue(),
                        ExprDotMethodChain(Ref("keysAndEvents")).Add("entrySet").Add("iterator"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"))
                    .ExprDotMethod(Ref("keysAndEvents"), "clear");
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
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                "groupRepsView",
                NewInstance(typeof(Dictionary<object, object>)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")));

                {
                    var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"));
                        forNew.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forNew.ExprDotMethod(
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
                        var forOld = ifOldData.ForEach(typeof(EventBean), "anOldData", Ref("oldData"));
                        forOld.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Ref("groupRepsView"),
                                    "put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), Ref("anOldData")))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "applyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            REF_AGENTINSTANCECONTEXT);
                    }
                }
            }

            method.Block.InstanceMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("entrySet").Add("iterator"),
                ConstantTrue(),
                REF_ISSYNTHESIZE,
                Ref("newEvents"),
                Ref("newEventsSortKey"));

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedViewFirstCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateGroupKeyArrayView = GenerateGroupKeyArrayViewCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
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

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                "groupRepsView",
                NewInstance(typeof(LinkedHashMap<object, object>)));

            if (forge.OptionalHavingNode == null) {
                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<EventBean>),
                        "pair",
                        REF_VIEWEVENTSLIST);
                    forEach.DeclareVar<EventBean[]>(
                            "newData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar<EventBean[]>(
                            "oldData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
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
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantTrue(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

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
                            var forloop = ifOldData.ForEach(typeof(EventBean), "anOldData", Ref("oldData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
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
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantFalse(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

                            forloop.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "applyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                }
            }
            else {
                // having clause present, having clause evaluates at the level of individual posts
                method.Block.DeclareVar<EventBean[]>(
                    "eventsPerStreamOneStream",
                    NewArrayByLength(typeof(EventBean), Constant(1)));

                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<EventBean>),
                        "pair",
                        REF_VIEWEVENTSLIST);
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
                            METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                            REF_AGGREGATIONSVC,
                            REF_AGENTINSTANCECONTEXT,
                            Ref("newData"),
                            Ref("newDataMultiKey"),
                            Ref("oldData"),
                            Ref("oldDataMultiKey"),
                            Ref("eventsPerStreamOneStream"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForLoopIntSimple("i", ArrayLength(Ref("newData")));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")))
                                .AssignArrayElement(
                                    "eventsPerStreamOneStream",
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
                                            Ref("eventsPerStreamOneStream"),
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
                            var ifPass = forloop.IfCondition(Ref("pass"))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("newData"), Ref("i"))));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantTrue(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }
                        }
                    }

                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forloop = ifOldData.ForLoopIntSimple("i", ArrayLength(Ref("oldData")));
                            forloop.DeclareVar<object>("mk", ArrayAtIndex(Ref("oldDataMultiKey"), Ref("i")))
                                .AssignArrayElement(
                                    "eventsPerStreamOneStream",
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
                                            Ref("eventsPerStreamOneStream"),
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
                            var ifPass = forloop.IfCondition(Ref("pass"))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("oldData"), Ref("i"))));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "put", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.InstanceMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantFalse(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }
                        }
                    }
                }
            }

            method.Block.InstanceMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("entrySet").Add("iterator"),
                ConstantTrue(),
                REF_ISSYNTHESIZE,
                Ref("newEvents"),
                Ref("newEventsSortKey"));

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void ProcessOutputLimitedViewAllCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromIteratorCodegen(forge, classScope, instance);
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
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
                    Constant(forge.GroupKeyTypes),
                    eventTypes));

            method.Block.DeclareVar<EventBean[]>(
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            if (forge.IsSelectRStream) {
                method.Block.InstanceMethod(
                    generateOutputBatchedArrFromIterator,
                    ConstantFalse(),
                    ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "entryIterator"),
                    ConstantFalse(),
                    REF_ISSYNTHESIZE,
                    Ref("oldEvents"),
                    Ref("oldEventsSortKey"));
            }

            {
                var forLoop = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_VIEWEVENTSLIST);
                forLoop.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")));

                {
                    var ifNewData = forLoop.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"));
                        forNew.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("aNewData"))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Ref(NAME_OUTPUTALLGROUPREPS),
                                    "put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), Ref("aNewData")))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "applyEnter",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            REF_AGENTINSTANCECONTEXT);
                    }
                }

                {
                    var ifOldData = forLoop.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOldData.ForEach(typeof(EventBean), "anOldData", Ref("oldData"));
                        forOld.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("anOldData"))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Ref(NAME_OUTPUTALLGROUPREPS),
                                    "put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), Ref("anOldData")))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.InstanceMethod(
                                generateOutputBatchedRowAddToList,
                                ConstantFalse(),
                                Ref("mk"),
                                Ref("eventsPerStream"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"));
                        }

                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "applyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            REF_AGENTINSTANCECONTEXT);
                    }
                }
            }

            method.Block.InstanceMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethod(Ref(NAME_OUTPUTALLGROUPREPS), "entryIterator"),
                ConstantTrue(),
                REF_ISSYNTHESIZE,
                Ref("newEvents"),
                Ref("newEventsSortKey"));

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
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysKeepEvent = GenerateGroupKeysKeepEventCodegen(forge, classScope, instance);
            var generateOutputBatchedRowFromMap =
                GenerateOutputBatchedRowFromMapCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, object>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, object>)))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysKeepEvent,
                            Ref("newData"),
                            Ref("keysAndEvents"),
                            ConstantTrue(),
                            Ref("eventsPerStream")))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysKeepEvent,
                            Ref("oldData"),
                            Ref("keysAndEvents"),
                            ConstantFalse(),
                            Ref("eventsPerStream")));

                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
                        generateOutputBatchedRowFromMap,
                        Ref("keysAndEvents"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"),
                        REF_AGENTINSTANCECONTEXT);
                }

                forEach.StaticMethod(
                        typeof(ResultSetProcessorGroupedUtil),
                        METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                        REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT,
                        Ref("newData"),
                        Ref("newDataMultiKey"),
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        Ref("eventsPerStream"))
                    .InstanceMethod(
                        generateOutputBatchedRowFromMap,
                        Ref("keysAndEvents"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"),
                        REF_AGENTINSTANCECONTEXT)
                    .ExprDotMethod(Ref("keysAndEvents"), "clear");
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

        protected internal static CodegenMethod ShortcutEvalGivenKeyCodegen(
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

        private static CodegenMethod ProcessViewResultPairDepthOneNoRStreamCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var shortcutEvalGivenKey =
                ShortcutEvalGivenKeyCodegen(forge.OptionalHavingNode, classScope, instance);
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
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
                    .IfCondition(
                        StaticMethod(typeof(CompatExtensions), "AreEquals", Ref("newGroupKey"), Ref("oldGroupKey")))
                    .DeclareVar<EventBean>(
                        "istream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("newGroupKey"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE))
                    .BlockReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "toPairNullIfNullIStream", Ref("istream")))
                    .DeclareVar<EventBean>(
                        "newKeyEvent",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("newGroupKey"),
                            Constant(true),
                            REF_ISSYNTHESIZE))
                    .DeclareVar<EventBean>(
                        "oldKeyEvent",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_OLDDATA,
                            Ref("oldGroupKey"),
                            Constant(true),
                            REF_ISSYNTHESIZE));

                if (forge.IsSorting) {
                    methodNode.Block.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("newGroupKey"),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .DeclareVar<object>(
                            "newSortKey",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                REF_NEWDATA,
                                ConstantTrue(),
                                REF_AGENTINSTANCECONTEXT))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("newGroupKey"),
                            ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"),
                            ConstantNull())
                        .DeclareVar<object>(
                            "oldSortKey",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "getSortKey",
                                REF_OLDDATA,
                                ConstantTrue(),
                                REF_AGENTINSTANCECONTEXT))
                        .DeclareVar<EventBean[]>(
                            "sorted",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "sortTwoKeys",
                                Ref("newKeyEvent"),
                                Ref("newSortKey"),
                                Ref("oldKeyEvent"),
                                Ref("oldSortKey")))
                        .MethodReturn(NewInstance<UniformPair<EventBean>>(Ref("sorted"), ConstantNull()));
                }
                else {
                    methodNode.Block.MethodReturn(
                        NewInstance<UniformPair<EventBean>>(
                            NewArrayWithInit(typeof(EventBean), Ref("newKeyEvent"), Ref("oldKeyEvent")),
                            ConstantNull()));
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean>),
                "processViewResultPairDepthOneNoRStream",
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

        protected internal static CodegenMethod ProcessViewResultNewDepthOneCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var shortcutEvalGivenKey =
                ShortcutEvalGivenKeyCodegen(forge.OptionalHavingNode, classScope, instance);
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<object>(
                    "groupKey",
                    LocalMethod(generateGroupKeySingle, REF_NEWDATA, ConstantTrue()));
                if (forge.IsSelectRStream) {
                    methodNode.Block.DeclareVar<EventBean>(
                        "rstream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("groupKey"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE));
                }

                methodNode.Block.ExprDotMethod(
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
                            REF_ISSYNTHESIZE));
                if (forge.IsSelectRStream) {
                    methodNode.Block.MethodReturn(
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            "toPairNullIfAllNullSingle",
                            Ref("istream"),
                            Ref("rstream")));
                }
                else {
                    methodNode.Block.MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "toPairNullIfNullIStream", Ref("istream")));
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean>),
                "processViewResultNewDepthOneCodegen",
                CodegenNamedParam.From(typeof(EventBean[]), NAME_NEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }
    }
} // end of namespace