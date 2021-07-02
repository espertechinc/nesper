///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    ///     <para>
    ///     Produces one row for each group that changed (and not one row per event). Computes MultiKey group-by keys for
    ///     each event and uses a set of the group-by keys to generate the result rows, using the first (old or new, anyone)
    ///     event for each distinct group-by key.
    ///     </para>
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
            method.Block
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .IfCondition(NotEqualsNull(REF_NEWDATA))
                .ForEach(typeof(EventBean), "aNewData", REF_NEWDATA)
                .AssignArrayElement("eventsPerStream", Constant(0), Ref("aNewData"))
                .DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("mk"), MEMBER_EXPREVALCONTEXT)
                .BlockEnd()
                .BlockEnd()
                .IfCondition(NotEqualsNull(REF_OLDDATA))
                .ForEach(typeof(EventBean), "anOldData", REF_OLDDATA)
                .AssignArrayElement("eventsPerStream", Constant(0), Ref("anOldData"))
                .DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyLeave", Ref("eventsPerStream"), Ref("mk"), MEMBER_EXPREVALCONTEXT)
                .BlockEnd()
                .BlockEnd();
        }

        public static void ApplyJoinResultCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block
                .IfCondition(Not(ExprDotMethod(REF_NEWDATA, "IsEmpty")))
                .ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewEvent", REF_NEWDATA)
                .DeclareVar<EventBean[]>("eventsPerStream", Cast(typeof(EventBean[]), ExprDotName(Ref("aNewEvent"), "Array")))
                .DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", Ref("eventsPerStream"), Ref("mk"), MEMBER_EXPREVALCONTEXT)
                .BlockEnd()
                .BlockEnd()
                .IfCondition(And(NotEqualsNull(REF_OLDDATA), Not(ExprDotMethod(REF_OLDDATA, "IsEmpty"))))
                .ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldEvent", REF_OLDDATA)
                .DeclareVar<EventBean[]>("eventsPerStream", Cast(typeof(EventBean[]), ExprDotName(Ref("anOldEvent"), "Array")))
                .DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyLeave", Ref("eventsPerStream"), Ref("mk"), MEMBER_EXPREVALCONTEXT)
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

            // NOTE FOR SELF: Is this supposed to be object, EventBean or object, EventBean[]?  Do we need
            // more context to know which one this should be using?  Leaving this note so that I can compare
            // different failure conditions.  I suspect there is some type-erasure assumptions here and what
            // is being passed around in java is LinkedHashMap with not key and value type information.  Thus,
            // there is no context required because they just cast the resultant value.  That will NOT work
            // in C# (for the better IMO).

            method.Block
                .DeclareVar<IDictionary<object, EventBean[]>>("keysAndEvents", NewInstance(typeof(Dictionary<object, EventBean[]>)))
                .DeclareVar<object[]>("newDataMultiKey", LocalMethod(generateGroupKeyArrayJoin, REF_NEWDATA, Ref("keysAndEvents"), ConstantTrue()))
                .DeclareVar<object[]>("oldDataMultiKey", LocalMethod(generateGroupKeyArrayJoin, REF_OLDDATA, Ref("keysAndEvents"), ConstantFalse()));

            if (forge.IsUnidirectional) {
                method.Block.ExprDotMethod(Ref("this"), "Clear");
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
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
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

            method.Block
                .DeclareVar<IDictionary<object, EventBean>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, EventBean>)))
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
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
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
                        NewArrayByLength(typeof(EventBean), ExprDotName(Ref("keysAndEvents"), "Count")))
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ExprDotName(Ref("keysAndEvents"), "Count")));

                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar<EventBean[][]>(
                        "currentGenerators",
                        NewArrayByLength(typeof(EventBean[]), ExprDotName(Ref("keysAndEvents"), "Count")));
                }

                methodNode.Block.DeclareVar<int>("count", Constant(0))
                    .DeclareVar<int>("cpid", ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"));

                {
                    var forEach = methodNode.Block.ForEach(
                        typeof(KeyValuePair<object, EventBean>),
                        "entry",
                        Ref("keysAndEvents"));
                    forEach.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ExprDotName(Ref("entry"), "Key"),
                            Ref("cpid"),
                            ConstantNull())
                        .AssignArrayElement(
                            ExprForgeCodegenNames.REF_EPS,
                            Constant(0),
                            Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Value")));

                    if (forge.OptionalHavingNode != null) {
                        forEach.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        ExprForgeCodegenNames.REF_ISNEWDATA,
                                        MEMBER_EXPREVALCONTEXT)))
                            .BlockContinue();
                    }

                    forEach.AssignArrayElement(
                            "events",
                            Ref("count"),
                            ExprDotMethod(
                                MEMBER_SELECTEXPRPROCESSOR,
                                "Process",
                                ExprForgeCodegenNames.REF_EPS,
                                ExprForgeCodegenNames.REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT))
                        .AssignArrayElement("keys", Ref("count"), ExprDotName(Ref("entry"), "Key"));

                    if (forge.IsSorting) {
                        forEach.AssignArrayElement(
                            "currentGenerators",
                            Ref("count"),
                            NewArrayWithInit(
                                typeof(EventBean),
                                Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Value"))));
                    }

                    forEach.IncrementRef("count")
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
                "GenerateOutputEventsView",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, EventBean>), "keysAndEvents",
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS),
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
                        typeof(KeyValuePair<object, EventBean>),
                        "entry",
                        Ref("keysAndEvents"));
                    forLoop.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ExprDotName(Ref("entry"), "Key"),
                            ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                            ConstantNull())
                        .AssignArrayElement(
                            "eventsPerStream",
                            Constant(0),
                            Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Value")));

                    if (forge.OptionalHavingNode != null) {
                        forLoop.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
                                        Ref("eventsPerStream"),
                                        ExprForgeCodegenNames.REF_ISNEWDATA,
                                        MEMBER_EXPREVALCONTEXT)))
                            .BlockContinue();
                    }

                    forLoop.ExprDotMethod(
                        Ref("resultEvents"),
                        "Add",
                        ExprDotMethod(
                            MEMBER_SELECTEXPRPROCESSOR,
                            "Process",
                            Ref("eventsPerStream"),
                            ExprForgeCodegenNames.REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));

                    if (forge.IsSorting) {
                        forLoop.ExprDotMethod(
                            Ref("optSortKeys"),
                            "Add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "GetSortKey",
                                Ref("eventsPerStream"),
                                ExprForgeCodegenNames.REF_ISNEWDATA,
                                MEMBER_EXPREVALCONTEXT));
                    }
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedRowFromMap",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, EventBean>), "keysAndEvents",
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>), "resultEvents",
                    typeof(IList<object>), "optSortKeys",
                    typeof(ExprEvaluatorContext), ExprForgeCodegenNames.NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorRowPerGroupImpl), 
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedArrFromEnumeratorCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);

            Consumer<CodegenMethod> code = method => method.Block
                .WhileLoop(ExprDotMethod(Ref("keysAndEvents"), "MoveNext"))
                .DeclareVar<KeyValuePair<object, EventBean[]>>(
                    "entry",
                    ExprDotName(Ref("keysAndEvents"), "Current"))
                .LocalMethod(
                    generateOutputBatchedRowAddToList,
                    Ref("join"),
                    ExprDotName(Ref("entry"), "Key"),
                    ExprDotName(Ref("entry"), "Value"),
                    ExprForgeCodegenNames.REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    Ref("resultEvents"),
                    Ref("optSortKeys"));

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedArrFromIterator",
                CodegenNamedParam.From(
                    typeof(bool), "join",
                    typeof(IEnumerator<KeyValuePair<object, EventBean[]>>), "keysAndEvents",
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>), "resultEvents",
                    typeof(IList<object>), "optSortKeys"),
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
                    MEMBER_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                    ConstantNull());

                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    ExprForgeCodegenNames.REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturnNoValue();
                }

                method.Block.ExprDotMethod(
                    Ref("resultEvents"),
                    "Add",
                    ExprDotMethod(
                        MEMBER_SELECTEXPRPROCESSOR,
                        "Process",
                        ExprForgeCodegenNames.REF_EPS,
                        ExprForgeCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        MEMBER_EXPREVALCONTEXT));

                if (forge.IsSorting) {
                    method.Block.ExprDotMethod(
                        Ref("optSortKeys"),
                        "Add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "GetSortKey",
                            ExprForgeCodegenNames.REF_EPS,
                            ExprForgeCodegenNames.REF_ISNEWDATA,
                            MEMBER_EXPREVALCONTEXT));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedRowAddToList",
                CodegenNamedParam.From(
                    typeof(bool), "join",
                    typeof(object), "mk",
                    typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS,
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>), "resultEvents",
                    typeof(IList<object>), "optSortKeys"),
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
                    MEMBER_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                    ConstantNull());

                if (forge.OptionalHavingNode != null) {
                    methodNode.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    ExprForgeCodegenNames.REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                methodNode.Block.MethodReturn(
                    ExprDotMethod(
                        MEMBER_SELECTEXPRPROCESSOR,
                        "Process",
                        ExprForgeCodegenNames.REF_EPS,
                        ExprForgeCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        MEMBER_EXPREVALCONTEXT));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean),
                "GenerateOutputBatchedNoSortWMap",
                CodegenNamedParam.From(
                    typeof(bool), "join",
                    typeof(object), "mk",
                    typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS,
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE),
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
                methodNode.Block
                    .DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), ExprDotName(Ref("keysAndEvents"), "Count")))
                    .DeclareVar<object[]>("keys", NewArrayByLength(typeof(object), ExprDotName(Ref("keysAndEvents"), "Count")));

                if (forge.IsSorting) {
                    methodNode.Block
                        .DeclareVar<EventBean[][]>("currentGenerators", NewArrayByLength(typeof(EventBean[]), ExprDotName(Ref("keysAndEvents"), "Count")));
                }

                methodNode.Block
                    .DeclareVar<int>("count", Constant(0))
                    .DeclareVar<int>("cpid", ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"));

                {
                    var forEach = methodNode.Block.ForEach(
                        typeof(KeyValuePair<object, EventBean[]>),
                        "entry",
                        Ref("keysAndEvents"));
                    forEach
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ExprDotName(Ref("entry"), "Key"),
                            Ref("cpid"),
                            ConstantNull())
                        .DeclareVar<EventBean[]>(
                            ExprForgeCodegenNames.NAME_EPS,
                            ExprDotName(Ref("entry"), "Value"));

                    if (forge.OptionalHavingNode != null) {
                        forEach.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        ExprForgeCodegenNames.REF_ISNEWDATA,
                                        MEMBER_EXPREVALCONTEXT)))
                            .BlockContinue();
                    }

                    forEach.AssignArrayElement(
                            "events",
                            Ref("count"),
                            ExprDotMethod(
                                MEMBER_SELECTEXPRPROCESSOR,
                                "Process",
                                ExprForgeCodegenNames.REF_EPS,
                                ExprForgeCodegenNames.REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT))
                        .AssignArrayElement("keys", Ref("count"), ExprDotName(Ref("entry"), "Key"));

                    if (forge.IsSorting) {
                        forEach.AssignArrayElement("currentGenerators", Ref("count"), Ref("eventsPerStream"));
                    }

                    forEach.IncrementRef("count")
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
                "GenerateOutputEventsJoin",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, EventBean[]>), "keysAndEvents",
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateGroupKeysKeepEventCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNullReturnNull("events")
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ArrayLength(Ref("events"))));
                {
                    methodNode.Block
                        .ForLoopIntSimple("i", ArrayLength(Ref("events")))
                        .AssignArrayElement(ExprForgeCodegenNames.REF_EPS, Constant(0), ArrayAtIndex(Ref("events"), Ref("i")))
                        .AssignArrayElement("keys", Ref("i"), LocalMethod(forge.GenerateGroupKeySingle, ExprForgeCodegenNames.REF_EPS, ExprForgeCodegenNames.REF_ISNEWDATA))
                        .ExprDotMethod(Ref("eventPerKey"), "Put", ArrayAtIndex(Ref("keys"), Ref("i")), ArrayAtIndex(Ref("events"), Ref("i")))
                        .BlockEnd();
                }
                methodNode.Block.MethodReturn(Ref("keys"));
            };

            return instance.Methods.AddMethod(
                typeof(object[]),
                "GenerateGroupKeysKeepEvent",
                CodegenNamedParam.From(
                    typeof(EventBean[]), "events", 
                    typeof(IDictionary<object, EventBean>), "eventPerKey",
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
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
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfCondition(Or(EqualsNull(Ref("resultSet")), ExprDotMethod(Ref("resultSet"), "IsEmpty")))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<object[]>("keys", NewArrayByLength(typeof(object), ExprDotName(Ref("resultSet"), "Count")))
                    .DeclareVar<int>("count", Constant(0));

                methodNode.Block
                    .ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "eventsPerStream", Ref("resultSet"))
                    .DeclareVar<EventBean[]>("eps", ExprDotName(Ref("eventsPerStream"), "Array"))
                    .AssignArrayElement("keys", Ref("count"), LocalMethod(forge.GenerateGroupKeySingle, Ref("eps"), ExprForgeCodegenNames.REF_ISNEWDATA))
                    .ExprDotMethod(Ref("eventPerKey"), "Put", ArrayAtIndex(Ref("keys"), Ref("count")), Ref("eps"))
                    .IncrementRef("count")
                    .BlockEnd();
                
                methodNode.Block.MethodReturn(Ref("keys"));
            };

            return instance.Methods.AddMethod(
                typeof(object[]),
                "GenerateGroupKeyArrayJoinTakingMapCodegen",
                CodegenNamedParam.From(
                    typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), "resultSet",
                    typeof(IDictionary<object, EventBean[]>), "eventPerKey",
                    typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        public static void GetEnumeratorViewCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsHistoricalOnly) {
                method.Block.MethodReturn(
                    LocalMethod(ObtainEnumeratorCodegen(forge, classScope, method, instance), REF_VIEWABLE));
                return;
            }

            method.Block.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT)
                .DeclareVar<IEnumerator<EventBean>>("enumerator", ExprDotMethod(REF_VIEWABLE, "GetEnumerator"))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                method.Block
                    .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                    .AssignArrayElement(
                        Ref("eventsPerStream"),
                        Constant(0),
                        Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                    .DeclareVar<object>(
                        "groupKey",
                        LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                    .ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "ApplyEnter",
                        Ref("eventsPerStream"),
                        Ref("groupKey"),
                        MEMBER_EXPREVALCONTEXT)
                    .BlockEnd();
            }

            method.Block.DeclareVar<ArrayDeque<EventBean>>(
                    "deque",
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ITERATORTODEQUE,
                        LocalMethod(ObtainEnumeratorCodegen(forge, classScope, method, instance), REF_VIEWABLE)))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "GetEnumerator"));
        }

        private static CodegenMethod ObtainEnumeratorCodegen(
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
                    StaticMethod<ResultSetProcessorRowPerGroupEnumerator>(
                        "For",
                        ExprDotMethod(REF_VIEWABLE, "GetEnumerator"),
                        Ref("this"),
                        MEMBER_AGGREGATIONSVC,
                        MEMBER_EXPREVALCONTEXT));
                return iterator;
            }

            var enumeratorSorted = GetEnumeratorSortedCodegen(forge, classScope, instance);
            iterator.Block.MethodReturn(
                LocalMethod(
                    enumeratorSorted,
                    ExprDotMethod(REF_VIEWABLE, "GetEnumerator")));
            return iterator;
        }

        protected internal static CodegenMethod GetEnumeratorSortedCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block.DeclareVar<EventBean[]>(
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar<List<EventBean>>("outgoingEvents", NewInstance(typeof(List<EventBean>)))
                    .DeclareVar<List<object>>("orderKeys", NewInstance(typeof(List<object>)))
                    .DeclareVar<ISet<object>>("priorSeenGroups", NewInstance(typeof(HashSet<object>)));

                {
                    var whileLoop = method.Block.WhileLoop(ExprDotMethod(Ref("parentIter"), "MoveNext"));
                    whileLoop.DeclareVar<EventBean>(
                            "candidate",
                            Cast(typeof(EventBean), ExprDotName(Ref("parentIter"), "Current")))
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("candidate"))
                        .DeclareVar<object>(
                            "groupKey",
                            LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("groupKey"),
                            ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                            ConstantNull());

                    if (forge.OptionalHavingNode != null) {
                        whileLoop.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
                                        ExprForgeCodegenNames.REF_EPS,
                                        ConstantTrue(),
                                        MEMBER_EXPREVALCONTEXT)))
                            .BlockContinue();
                    }

                    whileLoop.IfCondition(ExprDotMethod(Ref("priorSeenGroups"), "CheckedContains", Ref("groupKey")))
                        .BlockContinue();

                    whileLoop.ExprDotMethod(Ref("priorSeenGroups"), "Add", Ref("groupKey"))
                        .ExprDotMethod(
                            Ref("outgoingEvents"),
                            "Add",
                            ExprDotMethod(
                                MEMBER_SELECTEXPRPROCESSOR,
                                "Process",
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                ConstantTrue(),
                                MEMBER_EXPREVALCONTEXT))
                        .DeclareVar<object>(
                            "orderKey",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "GetSortKey",
                                Ref("eventsPerStream"),
                                ConstantTrue(),
                                MEMBER_EXPREVALCONTEXT))
                        .ExprDotMethod(Ref("orderKeys"), "Add", Ref("orderKey"));
                }

                method.Block.MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ORDEROUTGOINGGETITERATOR,
                        Ref("outgoingEvents"),
                        Ref("orderKeys"),
                        REF_ORDERBYPROCESSOR,
                        MEMBER_EXPREVALCONTEXT));
            };
            return instance.Methods.AddMethod(
                typeof(IEnumerator<EventBean>),
                "GetEnumeratorSorted",
                CodegenNamedParam.From(typeof(IEnumerator), "parentIter"),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        public static void GetEnumeratorJoinCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeyArrayJoin =
                GenerateGroupKeyArrayJoinTakingMapCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);
            method.Block
                .DeclareVar<IDictionary<object, EventBean[]>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, EventBean[]>)))
                .Expression(LocalMethod(generateGroupKeyArrayJoin, REF_JOINSET, Ref("keysAndEvents"), ConstantTrue()))
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(generateOutputEventsJoin, Ref("keysAndEvents"), ConstantTrue(), ConstantTrue()))
                .MethodReturn(NewInstance<ArrayEventEnumerator>(Ref("selectNewEvents")));
        }

        public static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT);
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
                    method.Block.ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "Remove", Ref("key"));
                }

                if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                    method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "Remove", Ref("key"));
                }

                if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                    method.Block.ExprDotMethod(Member(NAME_OUTPUTFIRSTHELPER), "Remove", Ref("key"));
                }
            };

            instance.Methods.AddMethod(
                typeof(void),
                "RemovedAggregationGroupKey",
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
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessView", classScope, method, instance);
        }

        private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            string methodName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var eventTypes = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);

            if (forge.IsOutputAll) {
                var stateMgmtSettings = forge.OutputAllOptHelperSettings.Invoke();
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowPerGroupOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerGroupOutputAllOpt",
                        MEMBER_EXPREVALCONTEXT,
                        Ref("this"),
                        Constant(forge.GroupKeyTypes),
                        groupKeyMKSerde,
                        eventTypes,
                        stateMgmtSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTALLHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
            else if (forge.IsOutputLast) {
                var stateMgmtSettings = forge.OutputLastOptHelperSettings.Invoke();
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowPerGroupOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerGroupOutputLastOpt",
                        MEMBER_EXPREVALCONTEXT,
                        Ref("this"),
                        Constant(forge.GroupKeyTypes),
                        groupKeyMKSerde,
                        eventTypes,
                        stateMgmtSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTLASTHELPER),
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
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessJoin", classScope, method, instance);
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenMethod method)
        {
            if (forge.IsOutputAll) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
            else if (forge.IsOutputLast) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "OutputView", REF_ISSYNTHESIZE));
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
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
            else if (forge.IsOutputLast) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "OutputJoin", REF_ISSYNTHESIZE));
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
                method.Block.ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTFIRSTHELPER), "Destroy");
            }
        }

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTALLGROUPREPS)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLGROUPREPS));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTFIRSTHELPER));
            }
        }

        private static void ProcessOutputLimitedJoinLastCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, EventBean[]>>(
                "groupRepsView",
                NewInstance(typeof(Dictionary<object, EventBean[]>)));
            {
                var forEach = method.Block
                    .ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "Clear");
                }

                {
                    var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(
                            typeof(MultiKeyArrayOfKeys<EventBean>),
                            "aNewData",
                            Ref("newData"));
                        forNew.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }

                {
                    var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOldData.ForEach(
                            typeof(MultiKeyArrayOfKeys<EventBean>),
                            "anOldData",
                            Ref("oldData"));
                        forOld.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.LocalMethod(
                generateOutputBatchedArrFromIterator,
                ConstantTrue(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("GetEnumerator"),
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
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromEnumerator =
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var outputFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            var groupKeyTypes = Constant(forge.GroupKeyTypes);
            var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
            var outputHelperSettings = forge.OutputFirstSettings.Invoke();
            
            instance.AddMember(NAME_OUTPUTFIRSTHELPER, typeof(ResultSetProcessorGroupedOutputFirstHelper));
            instance.ServiceCtor.Block.AssignRef(
                NAME_OUTPUTFIRSTHELPER,
                ExprDotMethod(
                    helperFactory,
                    "MakeRSGroupedOutputFirst",
                    MEMBER_EXPREVALCONTEXT,
                    groupKeyTypes,
                    outputFactory,
                    ConstantNull(),
                    Constant(-1),
                    groupKeyMKSerde,
                    outputHelperSettings.ToExpression()));

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, EventBean[]>>(
                "groupRepsView",
                NewInstance(typeof(LinkedHashMap<object, EventBean[]>)));

            if (forge.OptionalHavingNode == null) {
                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>),
                        "pair",
                        REF_JOINEVENTSSET);
                    forEach
                        .DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                            "newData",
                            ExprDotName(Ref("pair"), "First"))
                        .DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                            "oldData",
                            ExprDotName(Ref("pair"), "Second"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForEach(
                                typeof(MultiKeyArrayOfKeys<EventBean>),
                                "aNewData",
                                Ref("newData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
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
                                MEMBER_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forloop = ifOldData.ForEach(
                                typeof(MultiKeyArrayOfKeys<EventBean>),
                                "anOldData",
                                Ref("oldData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
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
                                MEMBER_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                }
            }
            else {
                method.Block.ExprDotMethod(Ref("groupRepsView"), "Clear");
                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>),
                        "pair",
                        REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                            "newData",
                            ExprDotName(Ref("pair"), "First"))
                        .DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                            "oldData",
                            ExprDotName(Ref("pair"), "Second"))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(forge.GenerateGroupKeyArrayJoin, Ref("oldData"), ConstantTrue()))
                        .StaticMethod(
                            typeof(ResultSetProcessorGroupedUtil),
                            METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_EXPREVALCONTEXT,
                            Ref("newData"),
                            Ref("newDataMultiKey"),
                            Ref("oldData"),
                            Ref("oldDataMultiKey"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        ifNewData.DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifNewData.ForEach(
                                typeof(MultiKeyArrayOfKeys<EventBean>),
                                "aNewData",
                                Ref("newData"));
                            forloop.DeclareVar<object>(
                                    "mk",
                                    ArrayAtIndex(Ref("newDataMultiKey"), Ref("count")))
                                .ExprDotMethod(
                                    MEMBER_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                    ConstantNull())
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("EvaluateHavingClause"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            MEMBER_EXPREVALCONTEXT)))
                                .IncrementRef("count")
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantTrue(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

                            forloop.IncrementRef("count");
                        }
                    }

                    {
                        var ifOldData = forEach.IfCondition(NotEqualsNull(Ref("oldData")))
                            .DeclareVar<int>("count", Constant(0));
                        {
                            var forloop = ifOldData.ForEach(
                                typeof(MultiKeyArrayOfKeys<EventBean>),
                                "anOldData",
                                Ref("oldData"));
                            forloop.DeclareVar<object>(
                                    "mk",
                                    ArrayAtIndex(Ref("oldDataMultiKey"), Ref("count")))
                                .ExprDotMethod(
                                    MEMBER_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                    ConstantNull())
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("EvaluateHavingClause"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            MEMBER_EXPREVALCONTEXT)))
                                .IncrementRef("count")
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
                                    generateOutputBatchedRowAddToList,
                                    ConstantFalse(),
                                    Ref("mk"),
                                    Ref("eventsPerStream"),
                                    ConstantFalse(),
                                    REF_ISSYNTHESIZE,
                                    Ref("oldEvents"),
                                    Ref("oldEventsSortKey"));
                            }

                            forloop.IncrementRef("count");
                        }
                    }
                }
            }

            method.Block.LocalMethod(
                generateOutputBatchedArrFromEnumerator,
                ConstantFalse(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("GetEnumerator"),
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
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
            var stateMgmtSettings = forge.OutputAllHelperSettings.Invoke();
            
            CodegenExpression eventTypes = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            instance.AddMember(NAME_OUTPUTALLGROUPREPS, typeof(ResultSetProcessorGroupedOutputAllGroupReps));
            instance.ServiceCtor.Block.AssignMember(
                NAME_OUTPUTALLGROUPREPS,
                ExprDotMethod(
                    helperFactory,
                    "MakeRSGroupedOutputAllNoOpt",
                    MEMBER_EXPREVALCONTEXT,
                    Constant(forge.GroupKeyTypes),
                    groupKeyMKSerde,
                    eventTypes,
                    stateMgmtSettings.ToExpression()));

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            if (forge.IsSelectRStream) {
                method.Block.LocalMethod(
                    generateOutputBatchedArrFromIterator,
                    ConstantTrue(),
                    ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "EntryEnumerator"),
                    ConstantFalse(),
                    REF_ISSYNTHESIZE,
                    Ref("oldEvents"),
                    Ref("oldEventsSortKey"));
            }

            {
                var forLoop = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forLoop.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));

                if (forge.IsUnidirectional) {
                    forLoop.ExprDotMethod(Ref("this"), "Clear");
                }

                {
                    var ifNewData = forLoop.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(
                            typeof(MultiKeyArrayOfKeys<EventBean>),
                            "aNewData",
                            Ref("newData"));
                        forNew.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Member(NAME_OUTPUTALLGROUPREPS),
                                    "Put",
                                    Ref("mk"),
                                    Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }

                {
                    var ifOldData = forLoop.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOldData.ForEach(
                            typeof(MultiKeyArrayOfKeys<EventBean>),
                            "anOldData",
                            Ref("oldData"));
                        forOld.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Member(NAME_OUTPUTALLGROUPREPS),
                                    "Put",
                                    Ref("mk"),
                                    Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.LocalMethod(
                generateOutputBatchedArrFromIterator,
                ConstantTrue(),
                ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "EntryEnumerator"),
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
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, EventBean[]>>(
                "keysAndEvents",
                NewInstance(typeof(Dictionary<object, EventBean[]>)));

            {
                var forEach = method.Block
                    .ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<ISet<MultiKeyArrayOfKeys<EventBean>>>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "Clear");
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
                    forEach.LocalMethod(
                        generateOutputBatchedArrFromIterator,
                        ConstantTrue(),
                        ExprDotMethodChain(Ref("keysAndEvents")).Add("GetEnumerator"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"));
                }

                forEach.StaticMethod(
                        typeof(ResultSetProcessorGroupedUtil),
                        METHOD_APPLYAGGJOINRESULTKEYEDJOIN,
                        MEMBER_AGGREGATIONSVC,
                        MEMBER_EXPREVALCONTEXT,
                        Ref("newData"),
                        Ref("newDataMultiKey"),
                        Ref("oldData"),
                        Ref("oldDataMultiKey"))
                    .LocalMethod(
                        generateOutputBatchedArrFromIterator,
                        ConstantTrue(),
                        ExprDotMethodChain(Ref("keysAndEvents")).Add("GetEnumerator"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"))
                    .ExprDotMethod(Ref("keysAndEvents"), "Clear");
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
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, EventBean[]>>(
                "groupRepsView",
                NewInstance(typeof(Dictionary<object, EventBean[]>)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));

                {
                    var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"));
                        forNew.DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
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
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Ref("groupRepsView"),
                                    "Push",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), Ref("anOldData")))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.LocalMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("GetEnumerator"),
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
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);
            var generateOutputBatchedArrFromIterator =
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var outputFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            var groupKeyTypes = Constant(forge.GroupKeyTypes);
            var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
            var outputHelperSettings = forge.OutputFirstSettings.Invoke();
            
            instance.AddMember(NAME_OUTPUTFIRSTHELPER, typeof(ResultSetProcessorGroupedOutputFirstHelper));
            instance.ServiceCtor.Block.AssignMember(
                NAME_OUTPUTFIRSTHELPER,
                ExprDotMethod(
                    helperFactory,
                    "MakeRSGroupedOutputFirst",
                    MEMBER_EXPREVALCONTEXT,
                    groupKeyTypes,
                    outputFactory,
                    ConstantNull(),
                    Constant(-1),
                    groupKeyMKSerde,
                    outputHelperSettings.ToExpression()));

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar<IDictionary<object, EventBean[]>>(
                "groupRepsView",
                NewInstance(typeof(Dictionary<object, EventBean[]>)));

            if (forge.OptionalHavingNode == null) {
                {
                    var forEach = method.Block.ForEach(
                        typeof(UniformPair<EventBean[]>),
                        "pair",
                        REF_VIEWEVENTSLIST);
                    forEach.DeclareVar<EventBean[]>(
                            "newData",
                            ExprDotName(Ref("pair"), "First"))
                        .DeclareVar<EventBean[]>(
                            "oldData",
                            ExprDotName(Ref("pair"), "Second"));

                    {
                        var ifNewData = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forloop = ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"));
                            forloop.DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "mk",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
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
                                MEMBER_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                MEMBER_EXPREVALCONTEXT);
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
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()))
                                .DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            var ifPass = forloop.IfCondition(Ref("pass"));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
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
                                MEMBER_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("mk"),
                                MEMBER_EXPREVALCONTEXT);
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
                        typeof(UniformPair<EventBean[]>),
                        "pair",
                        REF_VIEWEVENTSLIST);
                    forEach.DeclareVar<EventBean[]>(
                            "newData",
                            ExprDotName(Ref("pair"), "First"))
                        .DeclareVar<EventBean[]>(
                            "oldData",
                            ExprDotName(Ref("pair"), "Second"))
                        .DeclareVar<object[]>(
                            "newDataMultiKey",
                            LocalMethod(forge.GenerateGroupKeyArrayView, Ref("newData"), ConstantTrue()))
                        .DeclareVar<object[]>(
                            "oldDataMultiKey",
                            LocalMethod(forge.GenerateGroupKeyArrayView, Ref("oldData"), ConstantFalse()))
                        .StaticMethod(
                            typeof(ResultSetProcessorGroupedUtil),
                            METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_EXPREVALCONTEXT,
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
                                    MEMBER_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                    ConstantNull())
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("EvaluateHavingClause"),
                                            Ref("eventsPerStreamOneStream"),
                                            ConstantTrue(),
                                            MEMBER_EXPREVALCONTEXT)))
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(1),
                                        Constant(0)));
                            var ifPass = forloop.IfCondition(Ref("pass"))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("newData"), Ref("i"))));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
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
                                    MEMBER_AGGREGATIONSVC,
                                    "SetCurrentAccess",
                                    Ref("mk"),
                                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                    ConstantNull())
                                .IfCondition(
                                    Not(
                                        LocalMethod(
                                            instance.Methods.GetMethod("EvaluateHavingClause"),
                                            Ref("eventsPerStreamOneStream"),
                                            ConstantFalse(),
                                            MEMBER_EXPREVALCONTEXT)))
                                .BlockContinue();

                            forloop.DeclareVar<OutputConditionPolled>(
                                    "outputStateGroup",
                                    ExprDotMethod(
                                        Member(NAME_OUTPUTFIRSTHELPER),
                                        "GetOrAllocate",
                                        Ref("mk"),
                                        MEMBER_EXPREVALCONTEXT,
                                        outputFactory))
                                .DeclareVar<bool>(
                                    "pass",
                                    ExprDotMethod(
                                        Ref("outputStateGroup"),
                                        "UpdateOutputCondition",
                                        Constant(0),
                                        Constant(1)));
                            var ifPass = forloop.IfCondition(Ref("pass"))
                                .DeclareVar<EventBean[]>(
                                    "eventsPerStream",
                                    NewArrayWithInit(typeof(EventBean), ArrayAtIndex(Ref("oldData"), Ref("i"))));
                            var ifExists = ifPass.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(Ref("groupRepsView"), "Push", Ref("mk"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifExists.LocalMethod(
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

            method.Block.LocalMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethodChain(Ref("groupRepsView")).Add("GetEnumerator"),
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
                GenerateOutputBatchedArrFromEnumeratorCodegen(forge, classScope, instance);
            var generateOutputBatchedRowAddToList =
                GenerateOutputBatchedRowAddToListCodegen(forge, classScope, instance);

            var helperFactory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var eventTypes = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            var groupKeyMKSerde = forge.MultiKeyClassRef.GetExprMKSerde(method, classScope);
            var stateMgmtSettings = forge.OutputAllHelperSettings.Invoke();
            
            instance.AddMember(NAME_OUTPUTALLGROUPREPS, typeof(ResultSetProcessorGroupedOutputAllGroupReps));
            instance.ServiceCtor.Block.AssignRef(
                NAME_OUTPUTALLGROUPREPS,
                ExprDotMethod(
                    helperFactory,
                    "MakeRSGroupedOutputAllNoOpt",
                    MEMBER_EXPREVALCONTEXT,
                    Constant(forge.GroupKeyTypes),
                    groupKeyMKSerde,
                    eventTypes,
                    stateMgmtSettings.ToExpression()));

            method.Block.DeclareVar<EventBean[]>(
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            if (forge.IsSelectRStream) {
                method.Block.LocalMethod(
                    generateOutputBatchedArrFromIterator,
                    ConstantFalse(),
                    ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "EntryEnumerator"),
                    ConstantFalse(),
                    REF_ISSYNTHESIZE,
                    Ref("oldEvents"),
                    Ref("oldEventsSortKey"));
            }

            {
                var forLoop = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forLoop.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));

                {
                    var ifNewData = forLoop.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNewData.ForEach(typeof(EventBean), "aNewData", Ref("newData"));
                        forNew.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("aNewData"))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        var ifNotFound = forNew.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Member(NAME_OUTPUTALLGROUPREPS),
                                    "Put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), Ref("aNewData")))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }

                {
                    var ifOldData = forLoop.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOldData.ForEach(typeof(EventBean), "anOldData", Ref("oldData"));
                        forOld.AssignArrayElement(Ref("eventsPerStream"), Constant(0), Ref("anOldData"))
                            .DeclareVar<object>(
                                "mk",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        var ifNotFound = forOld.IfCondition(
                            EqualsNull(
                                ExprDotMethod(
                                    Member(NAME_OUTPUTALLGROUPREPS),
                                    "Put",
                                    Ref("mk"),
                                    NewArrayWithInit(typeof(EventBean), Ref("anOldData")))));
                        if (forge.IsSelectRStream) {
                            ifNotFound.LocalMethod(
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
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("mk"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.LocalMethod(
                generateOutputBatchedArrFromIterator,
                ConstantFalse(),
                ExprDotMethod(Member(NAME_OUTPUTALLGROUPREPS), "EntryEnumerator"),
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

            method.Block
                .DeclareVar<IDictionary<object, EventBean>>(
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, EventBean>)))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"))
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
                    forEach.LocalMethod(
                        generateOutputBatchedRowFromMap,
                        Ref("keysAndEvents"),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"),
                        MEMBER_EXPREVALCONTEXT);
                }

                forEach.StaticMethod(
                        typeof(ResultSetProcessorGroupedUtil),
                        METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                        MEMBER_AGGREGATIONSVC,
                        MEMBER_EXPREVALCONTEXT,
                        Ref("newData"),
                        Ref("newDataMultiKey"),
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        Ref("eventsPerStream"))
                    .LocalMethod(
                        generateOutputBatchedRowFromMap,
                        Ref("keysAndEvents"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"),
                        MEMBER_EXPREVALCONTEXT)
                    .ExprDotMethod(Ref("keysAndEvents"), "Clear");
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
                    MEMBER_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("groupKey"),
                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                    ConstantNull());
                if (optionalHavingNode != null) {
                    methodNode.Block
                        .IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    ExprForgeCodegenNames.REF_EPS,
                                    ExprForgeCodegenNames.REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                methodNode.Block.MethodReturn(
                    ExprDotMethod(
                        MEMBER_SELECTEXPRPROCESSOR,
                        "Process",
                        ExprForgeCodegenNames.REF_EPS,
                        ExprForgeCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        MEMBER_EXPREVALCONTEXT));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean),
                "ShortcutEvalGivenKey",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    ExprForgeCodegenNames.NAME_EPS,
                    typeof(object),
                    "groupKey",
                    typeof(bool),
                    ExprForgeCodegenNames.NAME_ISNEWDATA,
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

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<object>(
                        "newGroupKey",
                        LocalMethod(forge.GenerateGroupKeySingle, REF_NEWDATA, ConstantTrue()))
                    .DeclareVar<object>(
                        "oldGroupKey",
                        LocalMethod(forge.GenerateGroupKeySingle, REF_OLDDATA, ConstantFalse()))
                    .ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "ApplyEnter",
                        REF_NEWDATA,
                        Ref("newGroupKey"),
                        MEMBER_EXPREVALCONTEXT)
                    .ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "ApplyLeave",
                        REF_OLDDATA,
                        Ref("oldGroupKey"),
                        MEMBER_EXPREVALCONTEXT)
                    .IfCondition(
                        StaticMethod(typeof(object), "Equals", Ref("newGroupKey"), Ref("oldGroupKey")))
                    .DeclareVar<EventBean>(
                        "istream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("newGroupKey"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE))
                    .BlockReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "ToPairNullIfNullIStream", Ref("istream")))
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
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("newGroupKey"),
                            ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                            ConstantNull())
                        .DeclareVar<object>(
                            "newSortKey",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "GetSortKey",
                                REF_NEWDATA,
                                ConstantTrue(),
                                MEMBER_EXPREVALCONTEXT))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            Ref("newGroupKey"),
                            ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                            ConstantNull())
                        .DeclareVar<object>(
                            "oldSortKey",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "GetSortKey",
                                REF_OLDDATA,
                                ConstantTrue(),
                                MEMBER_EXPREVALCONTEXT))
                        .DeclareVar<EventBean[]>(
                            "sorted",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "SortTwoKeys",
                                Ref("newKeyEvent"),
                                Ref("newSortKey"),
                                Ref("oldKeyEvent"),
                                Ref("oldSortKey")))
                        .MethodReturn(NewInstance<UniformPair<EventBean[]>>(Ref("sorted"), ConstantNull()));
                }
                else {
                    methodNode.Block
                        .IfCondition(And(NotEqualsNull(Ref("newKeyEvent")), NotEqualsNull(Ref("oldKeyEvent"))))
                        .BlockReturn(NewInstance<UniformPair<EventBean[]>>(NewArrayWithInit(typeof(EventBean), Ref("newKeyEvent"), Ref("oldKeyEvent")), ConstantNull()))
                        .IfCondition(NotEqualsNull(Ref("newKeyEvent")))
                        .BlockReturn(NewInstance<UniformPair<EventBean[]>>(NewArrayWithInit(typeof(EventBean), Ref("newKeyEvent")), ConstantNull()))
                        .IfCondition(NotEqualsNull(Ref("oldKeyEvent")))
                        .BlockReturn(NewInstance<UniformPair<EventBean[]>>(NewArrayWithInit(typeof(EventBean), Ref("oldKeyEvent")), ConstantNull()))
                        .MethodReturn(ConstantNull());
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "ProcessViewResultPairDepthOneNoRStream",
                CodegenNamedParam.From(
                    typeof(EventBean[]), NAME_NEWDATA,
                    typeof(EventBean[]), NAME_OLDDATA,
                    typeof(bool), NAME_ISSYNTHESIZE),
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

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<object>(
                    "groupKey",
                    LocalMethod(forge.GenerateGroupKeySingle, REF_NEWDATA, ConstantTrue()));
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
                        MEMBER_AGGREGATIONSVC,
                        "ApplyEnter",
                        REF_NEWDATA,
                        Ref("groupKey"),
                        MEMBER_EXPREVALCONTEXT)
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
                            "ToPairNullIfAllNullSingle",
                            Ref("istream"),
                            Ref("rstream")));
                }
                else {
                    methodNode.Block.MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "ToPairNullIfNullIStream", Ref("istream")));
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "ProcessViewResultNewDepthOneCodegen",
                CodegenNamedParam.From(typeof(EventBean[]), NAME_NEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }
    }
} // end of namespace