///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;
using static com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup.
    ResultSetProcessorRowPerGroupRollupUtil;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public class ResultSetProcessorRowPerGroupRollupImpl
    {
        private const string NAME_OUTPUTALLHELPER = "outputAllHelper";
        private const string NAME_OUTPUTLASTHELPER = "outputLastHelper";
        private const string NAME_OUTPUTFIRSTHELPERS = "outputFirstHelpers";
        private const string NAME_EVENTPERGROUPBUFJOIN = "eventPerGroupBufJoin";
        private const string NAME_EVENTPERGROUPBUFVIEW = "eventPerGroupBufView";
        private const string NAME_GROUPREPSPERLEVELBUF = "groupRepsPerLevelBuf";
        private const string NAME_RSTREAMEVENTSORTARRAYBUF = "rstreamEventSortArrayBuf";

        internal static void ProcessJoinResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            AddEventPerGroupBufCodegen(
                NAME_EVENTPERGROUPBUFJOIN,
                EPTYPE_MAPARRAY_OBJECT_EVENTBEANARRAY,
                forge,
                instance,
                method,
                classScope);
            var resetEventPerGroupJoinBuf =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFJOIN, classScope, instance);
            var generateGroupKeysJoin = GenerateGroupKeysJoinCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

            if (forge.IsUnidirectional) {
                method.Block.ExprDotMethod(Ref("this"), "Clear");
            }

            method.Block.LocalMethod(resetEventPerGroupJoinBuf)
                .DeclareVar(
                    typeof(object[][]),
                    "newDataMultiKey",
                    LocalMethod(generateGroupKeysJoin, REF_NEWDATA, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue()))
                .DeclareVar(
                    typeof(object[][]),
                    "oldDataMultiKey",
                    LocalMethod(generateGroupKeysJoin, REF_OLDDATA, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantFalse()))
                .DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsJoin,
                            Ref(NAME_EVENTPERGROUPBUFJOIN),
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
                    LocalMethod(
                        generateOutputEventsJoin,
                        Ref(NAME_EVENTPERGROUPBUFJOIN),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        internal static void ProcessViewResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            AddEventPerGroupBufCodegen(
                NAME_EVENTPERGROUPBUFVIEW,
                EPTYPE_MAPARRAY_OBJECT_EVENTBEAN,
                forge,
                instance,
                method,
                classScope);
            var resetEventPerGroupBufView =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFVIEW, classScope, instance);
            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            method.Block.LocalMethod(resetEventPerGroupBufView)
                .DeclareVar(
                    typeof(object[][]),
                    "newDataMultiKey",
                    LocalMethod(generateGroupKeysView, REF_NEWDATA, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue()))
                .DeclareVar(
                    typeof(object[][]),
                    "oldDataMultiKey",
                    LocalMethod(generateGroupKeysView, REF_OLDDATA, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantFalse()))
                .DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView,
                            Ref(NAME_EVENTPERGROUPBUFVIEW),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE)
                        : ConstantNull())
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
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
                        Ref(NAME_EVENTPERGROUPBUFVIEW),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        private static void AddEventPerGroupBufCodegen(
            string memberName,
            Type eventType,
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var eventPerGroupBufTypeElement = typeof(LinkedHashMap<string, object>);
            var eventPerGroupBufType = eventPerGroupBufTypeElement.MakeArrayType();
            
            if (!instance.HasMember(memberName)) {
                var init = method.MakeChild(
                    eventPerGroupBufType,
                    typeof(ResultSetProcessorRowPerGroupRollupImpl),
                    classScope);
                instance.AddMember(memberName, eventType);
                instance.ServiceCtor.Block.AssignMember(memberName, LocalMethod(init));
                var levelCount = forge.GroupByRollupDesc.Levels.Length;
                init.Block
                    .DeclareVar(
                        eventPerGroupBufType,
                        memberName,
                        NewArrayByLength(eventPerGroupBufTypeElement, Constant(levelCount)))
                    .ForLoopIntSimple("i", Constant(levelCount))
                    .AssignArrayElement(memberName, Ref("i"), NewInstance(eventPerGroupBufTypeElement))
                    .BlockEnd()
                    .MethodReturn(Ref(memberName));
            }
        }

        internal static CodegenMethod GenerateOutputEventsViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<EventBean[]>(
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar(
                        typeof(IList<EventBean>),
                        "events",
                        NewInstance(typeof(List<EventBean>), Constant(1)))
                    .DeclareVar(
                        typeof(IList<GroupByRollupKey>),
                        "currentGenerators",
                        forge.IsSorting ? NewInstance(typeof(List<GroupByRollupKey>), Constant(1)) : ConstantNull())
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]),
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                {
                    var forLevels = methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        Ref("levels"));
                    {
                        var forEvents = forLevels.ForEachVar(
                            "entry",
                            ArrayAtIndex(Ref("keysAndEvents"), ExprDotName(Ref("level"), "LevelNumber")));
                        forEvents.DeclareVar<object>("groupKey", ExprDotName(Ref("entry"), "Key"))
                            .ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "SetCurrentAccess",
                                Ref("groupKey"),
                                ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                Ref("level"))
                            .AssignArrayElement(
                                Ref("eventsPerStream"),
                                Constant(0),
                                Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Value")));

                        if (forge.PerLevelForges.OptionalHavingForges != null) {
                            var having = ArrayAtIndex(
                                MEMBER_HAVINGEVALUATOR_ARRAY,
                                ExprDotName(Ref("level"), "LevelNumber"));
                            forEvents.IfCondition(
                                    Not(
                                        ExprDotMethod(
                                            having,
                                            "evaluateHaving",
                                            REF_EPS,
                                            REF_ISNEWDATA,
                                            MEMBER_EXPREVALCONTEXT)))
                                .BlockContinue();
                        }

                        forEvents.ExprDotMethod(
                            Ref("events"),
                            "Add",
                            ExprDotMethod(
                                ArrayAtIndex(
                                    MEMBER_SELECTEXPRPROCESSOR_ARRAY,
                                    ExprDotName(Ref("level"), "LevelNumber")),
                                "Process",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));

                        if (forge.IsSorting) {
                            forEvents.DeclareVar<EventBean[]>(
                                    "currentEventsPerStream",
                                    NewArrayWithInit(
                                        typeof(EventBean),
                                        Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Value"))))
                                .ExprDotMethod(
                                    Ref("currentGenerators"),
                                    "Add",
                                    NewInstance(
                                        typeof(GroupByRollupKey),
                                        Ref("currentEventsPerStream"),
                                        Ref("level"),
                                        Ref("groupKey")));
                        }
                    }
                }

                methodNode.Block.IfCondition(ExprDotMethod(Ref("events"), "IsEmpty"))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<EventBean[]>(
                        "outgoing",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")));
                if (forge.IsSorting) {
                    methodNode.Block.IfCondition(Relational(ArrayLength(Ref("outgoing")), GT, Constant(1)))
                        .BlockReturn(
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "sortRollup",
                                Ref("outgoing"),
                                Ref("currentGenerators"),
                                REF_ISNEWDATA,
                                MEMBER_EXPREVALCONTEXT,
                                MEMBER_AGGREGATIONSVC));
                }

                methodNode.Block.MethodReturn(Ref("outgoing"));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GenerateOutputEventsView",
                CodegenNamedParam.From(
                    EPTYPE_MAPARRAY_OBJECT_EVENTBEAN,
                    "keysAndEvents",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateOutputEventsJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(
                        typeof(IList<GroupByRollupKey>),
                        "events",
                        NewInstance(typeof(List<GroupByRollupKey>), Constant(1)))
                    .DeclareVar(
                        typeof(IList<GroupByRollupKey>),
                        "currentGenerators",
                        forge.IsSorting ? NewInstance(typeof(List<GroupByRollupKey>), Constant(1)) : ConstantNull())
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]),
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                {
                    var forLevels = methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        Ref("levels"));
                    {
                        var forEvents = forLevels.ForEachVar(
                            "entry",
                            ArrayAtIndex(Ref("eventPairs"), ExprDotName(Ref("level"), "LevelNumber")));
                        forEvents.DeclareVar<object>("groupKey", ExprDotName(Ref("entry"), "Key"))
                            .ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "SetCurrentAccess",
                                Ref("groupKey"),
                                ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                Ref("level"))
                            .DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")));

                        if (forge.PerLevelForges.OptionalHavingForges != null) {
                            var having = ArrayAtIndex(
                                MEMBER_HAVINGEVALUATOR_ARRAY,
                                ExprDotName(Ref("level"), "LevelNumber"));
                            forEvents.IfCondition(
                                    Not(
                                        ExprDotMethod(
                                            having,
                                            "evaluateHaving",
                                            REF_EPS,
                                            REF_ISNEWDATA,
                                            MEMBER_EXPREVALCONTEXT)))
                                .BlockContinue();
                        }

                        forEvents.ExprDotMethod(
                            Ref("events"),
                            "Add",
                            ExprDotMethod(
                                ArrayAtIndex(
                                    MEMBER_SELECTEXPRPROCESSOR_ARRAY,
                                    ExprDotName(Ref("level"), "LevelNumber")),
                                "Process",
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));

                        if (forge.IsSorting) {
                            forEvents.ExprDotMethod(
                                Ref("currentGenerators"),
                                "Add",
                                NewInstance(
                                    typeof(GroupByRollupKey),
                                    Ref("eventsPerStream"),
                                    Ref("level"),
                                    Ref("groupKey")));
                        }
                    }
                }

                methodNode.Block.IfCondition(ExprDotMethod(Ref("events"), "IsEmpty"))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<EventBean[]>(
                        "outgoing",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")));
                if (forge.IsSorting) {
                    methodNode.Block.IfCondition(Relational(ArrayLength(Ref("outgoing")), GT, Constant(1)))
                        .BlockReturn(
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "sortRollup",
                                Ref("outgoing"),
                                Ref("currentGenerators"),
                                REF_ISNEWDATA,
                                MEMBER_EXPREVALCONTEXT,
                                MEMBER_AGGREGATIONSVC));
                }

                methodNode.Block.MethodReturn(Ref("outgoing"));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GenerateOutputEventsJoin",
                CodegenNamedParam.From(
                    EPTYPE_MAPARRAY_OBJECT_EVENTBEANARRAY,
                    "eventPairs",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        internal static void GetEnumeratorViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
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
                .DeclareVar(typeof(IEnumerator<EventBean>), "en", ExprDotMethod(REF_VIEWABLE, "GetEnumerator"))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar<object[]>(
                    "groupKeys",
                    NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                .DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]),
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));
            {
                method.Block.WhileLoop(ExprDotMethod(Ref("en"), "MoveNext"))
                    .AssignArrayElement(
                        Ref("eventsPerStream"),
                        Constant(0),
                        Cast(typeof(EventBean), ExprDotMethod(Ref("en"), "next")))
                    .DeclareVar<object>(
                        "groupKeyComplete",
                        LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                    .ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                    .DeclareVar<object>(
                        "subkey",
                        ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "computeSubkey", Ref("groupKeyComplete")))
                    .AssignArrayElement("groupKeys", Ref("j"), Ref("subkey"))
                    .BlockEnd()
                    .ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "ApplyEnter",
                        Ref("eventsPerStream"),
                        Ref("groupKeys"),
                        MEMBER_EXPREVALCONTEXT)
                    .BlockEnd();
            }

            method.Block.DeclareVar(
                    typeof(ArrayDeque<EventBean>),
                    "deque",
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ITERATORTODEQUE,
                        LocalMethod(ObtainEnumeratorCodegen(forge, classScope, method, instance), REF_VIEWABLE)))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "GetEnumerator"));
        }

        private static CodegenMethod ObtainEnumeratorCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent,
            CodegenInstanceAux instance)
        {
            var resetEventPerGroupBufView =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFVIEW, classScope, instance);
            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            var iterator = parent.MakeChild(
                    typeof(IEnumerator<EventBean>),
                    typeof(ResultSetProcessorRowPerGroupRollupImpl),
                    classScope)
                .AddParam<Viewable>(NAME_VIEWABLE);
            iterator.Block.LocalMethod(resetEventPerGroupBufView)
                .DeclareVar<EventBean[]>(
                    "events",
                    StaticMethod(
                        typeof(CollectionUtil),
                        METHOD_ENUMERATORTOARRAYEVENTS,
                        ExprDotMethod(REF_VIEWABLE, "GetEnumerator")))
                .LocalMethod(generateGroupKeysView, Ref("events"), Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue())
                .DeclareVar<EventBean[]>(
                    "output",
                    LocalMethod(
                        generateOutputEventsView,
                        Ref(NAME_EVENTPERGROUPBUFVIEW),
                        ConstantTrue(),
                        ConstantTrue()))
                .MethodReturn(NewInstance(typeof(ArrayEventEnumerator), Ref("output")));
            return iterator;
        }

        internal static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessView", classScope, method, instance);
        }

        internal static void GetEnumeratorJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysJoin = GenerateGroupKeysJoinCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);
            var resetEventPerGroupBuf = ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFJOIN, classScope, instance);
            method.Block.LocalMethod(resetEventPerGroupBuf)
                .LocalMethod(generateGroupKeysJoin, REF_JOINSET, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue())
                .DeclareVar<EventBean[]>(
                    "output",
                    LocalMethod(
                        generateOutputEventsJoin,
                        Ref(NAME_EVENTPERGROUPBUFJOIN),
                        ConstantTrue(),
                        ConstantTrue()))
                .MethodReturn(NewInstance(typeof(ArrayEventEnumerator), Ref("output")));
        }

        internal static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT);
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsOutputDefault) {
                HandleOutputLimitDefaultJoinCodegen(forge, classScope, method, instance);
                return;
            }
            else if (forge.IsOutputAll) {
                HandleOutputLimitAllJoinCodegen(forge, classScope, method, instance);
                return;
            }
            else if (forge.IsOutputFirst) {
                HandleOutputLimitFirstJoinCodegen(forge, classScope, method, instance);
                return;
            }

            HandleOutputLimitLastJoinCodegen(forge, classScope, method, instance);
        }

        internal static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsOutputDefault) {
                HandleOutputLimitDefaultViewCodegen(forge, classScope, method, instance);
                return;
            }
            else if (forge.IsOutputAll) {
                HandleOutputLimitAllViewCodegen(forge, classScope, method, instance);
                return;
            }
            else if (forge.IsOutputFirst) {
                HandleOutputLimitFirstViewCodegen(forge, classScope, method, instance);
                return;
            }

            HandleOutputLimitLastViewCodegen(forge, classScope, method, instance);
        }

        internal static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                method.Block.ForEach(
                        typeof(ResultSetProcessorGroupedOutputFirstHelper),
                        "helper",
                        Member(NAME_OUTPUTFIRSTHELPERS))
                    .ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Ref("helper"));
            }
        }

        private static void HandleOutputLimitFirstViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.ForEach(typeof(IDictionary<object, EventBean[]>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "Clear");

            method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");

            method.Block.DeclareVarNoInit(typeof(int), "count");
            if (forge.PerLevelForges.OptionalHavingForges == null) {
                var handleOutputLimitFirstViewNoHaving =
                    HandleOutputLimitFirstViewNoHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstViewNoHaving,
                        REF_VIEWEVENTSLIST,
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel")));
            }
            else {
                var handleOutputLimitFirstViewHaving =
                    HandleOutputLimitFirstViewHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstViewHaving,
                        REF_VIEWEVENTSLIST,
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel")));
            }

            method.Block.MethodReturn(
                LocalMethod(generateAndSort, Ref(NAME_GROUPREPSPERLEVELBUF), REF_ISSYNTHESIZE, Ref("count")));
        }

        private static void HandleOutputLimitFirstJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.ForEach(typeof(IDictionary<object, EventBean[]>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "Clear");

            method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");

            method.Block.DeclareVarNoInit(typeof(int), "count");
            if (forge.PerLevelForges.OptionalHavingForges == null) {
                var handleOutputLimitFirstJoinNoHaving =
                    HandleOutputLimitFirstJoinNoHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstJoinNoHaving,
                        REF_JOINEVENTSSET,
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel")));
            }
            else {
                var handleOutputLimitFirstJoinHaving =
                    HandleOutputLimitFirstJoinHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstJoinHaving,
                        REF_JOINEVENTSSET,
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel")));
            }

            method.Block.MethodReturn(
                LocalMethod(generateAndSort, Ref(NAME_GROUPREPSPERLEVELBUF), REF_ISSYNTHESIZE, Ref("count")));
        }

        private static CodegenMethod HandleOutputLimitFirstViewHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);
            var outputFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<int>("count", Constant(0));

                {
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                    forEach.DeclareVar<EventBean[]>(
                            "newData",
                            Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "First")))
                        .DeclareVar<EventBean[]>(
                            "oldData",
                            Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Second")))
                        .DeclareVar<object[]>(
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                        .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach<EventBean>("aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forNew.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach<EventBean>("anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forOld.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }

                        var ifNewFirst = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNewFirst = ifNewFirst.ForEach<EventBean>("aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forNewFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        MEMBER_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    MEMBER_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "evaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantTrue(),
                                                MEMBER_EXPREVALCONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>(
                                        "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(1),
                                            Constant(0)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                        }

                        var ifOldFirst = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOldFirst = ifOldFirst.ForEach<EventBean>("anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forOldFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        MEMBER_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    MEMBER_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "evaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantFalse(),
                                                MEMBER_EXPREVALCONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>(
                                        "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(0),
                                            Constant(1)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "HandleOutputLimitFirstViewHaving",
                CodegenNamedParam.From(
                    EPTYPE_LIST_UNIFORMPAIR_EVENTBEANARRAY,
                    "viewEventsList",
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>[]),
                    "oldEventsPerLevel",
                    typeof(IList<object>[]),
                    "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static CodegenMethod HandleOutputLimitFirstJoinNoHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);

            var outputFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<int>("count", Constant(0))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar(
                            EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                            "newData",
                            Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "First")))
                        .DeclareVar(
                            EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                            "oldData",
                            Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "Second")))
                        .DeclareVar<object[]>(
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)));

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var forLvl = forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>(
                                        "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(1),
                                            Constant(0)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                            forNew.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                var forLvl = forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>(
                                        "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(0),
                                            Constant(1)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                            forOld.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "HandleOutputLimitFirstJoinNoHaving",
                CodegenNamedParam.From(
                    EPTYPE_LIST_UNIFORMPAIR_SET_MKARRAY_EVENTBEAN,
                    NAME_JOINEVENTSSET,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>[]),
                    "oldEventsPerLevel",
                    typeof(IList<object>[]),
                    "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static CodegenMethod HandleOutputLimitFirstJoinHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);

            var outputFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<int>("count", Constant(0))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar(
                            EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                            "newData",
                            Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "First")))
                        .DeclareVar(
                            EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                            "oldData",
                            Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "Second")))
                        .DeclareVar<object[]>(
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)));
                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forNew.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>("groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forOld.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }

                        var ifNewFirst = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNewFirst = ifNewFirst.ForEach(
                                    typeof(MultiKeyArrayOfKeys<EventBean>),
                                    "aNewData",
                                    Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>("groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forNewFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>("groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        MEMBER_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    MEMBER_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "evaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantTrue(),
                                                MEMBER_EXPREVALCONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>("outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>("pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(1),
                                            Constant(0)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                        }

                        var ifOldFirst = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOldFirst = ifOldFirst.ForEach(
                                    typeof(MultiKeyArrayOfKeys<EventBean>),
                                    "anOldData",
                                    Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>("groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forOldFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>("groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        MEMBER_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    MEMBER_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "evaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantFalse(),
                                                MEMBER_EXPREVALCONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>("outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>("pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(0),
                                            Constant(1)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "HandleOutputLimitFirstJoinHaving",
                CodegenNamedParam.From(
                    EPTYPE_LIST_UNIFORMPAIR_SET_MKARRAY_EVENTBEAN,
                    NAME_JOINEVENTSSET,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>[]),
                    "oldEventsPerLevel",
                    typeof(IList<object>[]),
                    "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static CodegenMethod HandleOutputLimitFirstViewNoHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);

            var outputFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.NamespaceScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<int>("count", Constant(0));

                {
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                    forEach.DeclareVar(
                            typeof(EventBean[]),
                            "newData",
                            Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "First")))
                        .DeclareVar(
                            typeof(EventBean[]),
                            "oldData",
                            Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Second")))
                        .DeclareVar(
                            typeof(object[]),
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                        .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach<EventBean>("aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>("groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var forLvl = forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>("groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>("outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>("pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(1),
                                            Constant(0)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                            forNew.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach<EventBean>("anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>("groupKeyComplete",
                                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                var forLvl = forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>("groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>("outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Member(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            MEMBER_EXPREVALCONTEXT,
                                            outputFactory))
                                    .DeclareVar<bool>("pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"),
                                            "UpdateOutputCondition",
                                            Constant(0),
                                            Constant(1)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                            "Put",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .IncrementRef("count");
                                }
                            }
                            forOld.ExprDotMethod(
                                MEMBER_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "HandleOutputLimitFirstNoViewHaving",
                CodegenNamedParam.From(
                    EPTYPE_LIST_UNIFORMPAIR_EVENTBEANARRAY,
                    "viewEventsList",
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>[]),
                    "oldEventsPerLevel",
                    typeof(IList<object>[]),
                    "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static void HandleOutputLimitDefaultViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedCollectView = GenerateOutputBatchedCollectViewCodegen(forge, classScope, instance);
            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);
            var resetEventPerGroupBufView =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFVIEW, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar(
                typeof(EventBean[]),
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar(
                        typeof(EventBean[]),
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar(
                        typeof(EventBean[]),
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Second")))
                    .LocalMethod(resetEventPerGroupBufView)
                    .DeclareVar(
                        typeof(object[]),
                        "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysView,
                            Ref("newData"),
                            Ref(NAME_EVENTPERGROUPBUFVIEW),
                            ConstantTrue()))
                    .DeclareVar(
                        typeof(object[]),
                        "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysView,
                            Ref("oldData"),
                            Ref(NAME_EVENTPERGROUPBUFVIEW),
                            ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(
                        generateOutputBatchedCollectView,
                        Ref(NAME_EVENTPERGROUPBUFVIEW),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        Ref("oldEvents"),
                        Ref("oldEventsSortKey"),
                        Ref("eventsPerStream"));
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
                        generateOutputBatchedCollectView,
                        Ref(NAME_EVENTPERGROUPBUFVIEW),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"),
                        Ref("eventsPerStream"));
            }

            ResultSetProcessorUtil.FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        private static void HandleOutputLimitDefaultJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedCollectJoin = GenerateOutputBatchedCollectJoinCodegen(forge, classScope, instance);
            var generateGroupKeysJoin = GenerateGroupKeysJoinCodegen(forge, classScope, instance);
            var resetEventPerGroupBufJoin =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFJOIN, classScope, instance);

            ResultSetProcessorUtil.PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar(
                        EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                        "newData",
                        Cast<ISet<MultiKeyArrayOfKeys<EventBean>>>(ExprDotName(Ref("pair"), "First")))
                    .DeclareVar(
                        EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                        "oldData",
                        Cast<ISet<MultiKeyArrayOfKeys<EventBean>>>(ExprDotName(Ref("pair"), "Second")))
                    .LocalMethod(resetEventPerGroupBufJoin)
                    .DeclareVar(
                        typeof(object[]),
                        "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysJoin,
                            Ref("newData"),
                            Ref(NAME_EVENTPERGROUPBUFJOIN),
                            ConstantTrue()))
                    .DeclareVar(
                        typeof(object[]),
                        "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysJoin,
                            Ref("oldData"),
                            Ref(NAME_EVENTPERGROUPBUFJOIN),
                            ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(
                        generateOutputBatchedCollectJoin,
                        Ref(NAME_EVENTPERGROUPBUFJOIN),
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
                        generateOutputBatchedCollectJoin,
                        Ref(NAME_EVENTPERGROUPBUFJOIN),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));
            }

            ResultSetProcessorUtil.FinalizeOutputMaySortMayRStreamCodegen(
                method.Block,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);
        }

        internal static void RemovedAggregationGroupKeyCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => method.Block.MethodThrowUnsupported();
            instance.Methods.AddMethod(
                typeof(void),
                "RemovedAggregationGroupKey",
                CodegenNamedParam.From(typeof(object), "key"),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateOutputBatchedGivenArrayCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatched = GenerateOutputBatchedCodegen(forge, instance, classScope);
            Consumer<CodegenMethod> code = methodNode => methodNode.Block.DeclareVar(
                    typeof(IList<EventBean>),
                    "resultList",
                    ArrayAtIndex(Ref("resultEvents"), ExprDotName(Ref("level"), "LevelNumber")))
                .DeclareVarNoInit(typeof(IList<object>), "sortKeys")
                .IfCondition(EqualsNull(Ref("optSortKeys")))
                .AssignRef("sortKeys", ConstantNull())
                .IfElse()
                .AssignRef("sortKeys", ArrayAtIndex(Ref("optSortKeys"), ExprDotName(Ref("level"), "LevelNumber")))
                .BlockEnd()
                .LocalMethod(
                    generateOutputBatched,
                    Ref("mk"),
                    Ref("level"),
                    Ref("eventsPerStream"),
                    REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    Ref("resultList"),
                    Ref("sortKeys"));
            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedGivenArrayCodegen",
                CodegenNamedParam.From(
                    typeof(bool),
                    "join",
                    typeof(object),
                    "mk",
                    typeof(AggregationGroupByRollupLevel),
                    "level",
                    typeof(EventBean[]),
                    "eventsPerStream",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>[]),
                    "resultEvents",
                    typeof(IList<object>[]),
                    "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        internal static CodegenMethod GenerateOutputBatchedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    MEMBER_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                    Ref("level"));

                if (forge.PerLevelForges.OptionalHavingForges != null) {
                    methodNode.Block.IfCondition(
                            Not(
                                ExprDotMethod(
                                    ArrayAtIndex(
                                        MEMBER_HAVINGEVALUATOR_ARRAY,
                                        ExprDotName(Ref("level"), "LevelNumber")),
                                    "evaluateHaving",
                                    Ref("eventsPerStream"),
                                    REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturnNoValue();
                }

                var selectExprProcessor = ArrayAtIndex(
                    MEMBER_SELECTEXPRPROCESSOR_ARRAY,
                    ExprDotName(Ref("level"), "LevelNumber"));
                methodNode.Block.ExprDotMethod(
                    Ref("resultEvents"),
                    "Add",
                    ExprDotMethod(
                        selectExprProcessor,
                        "Process",
                        Ref("eventsPerStream"),
                        REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        MEMBER_EXPREVALCONTEXT));

                if (forge.IsSorting) {
                    methodNode.Block.ExprDotMethod(
                        Ref("optSortKeys"),
                        "Add",
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "GetSortKeyRollup",
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            MEMBER_EXPREVALCONTEXT,
                            Ref("level")));
                }
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatched",
                CodegenNamedParam.From(
                    typeof(object),
                    "mk",
                    typeof(AggregationGroupByRollupLevel),
                    "level",
                    typeof(EventBean[]),
                    "eventsPerStream",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "resultEvents",
                    typeof(IList<object>),
                    "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        internal static void GenerateOutputBatchedMapUnsortedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    MEMBER_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotName(MEMBER_EXPREVALCONTEXT, "AgentInstanceId"),
                    Ref("level"));

                if (forge.PerLevelForges.OptionalHavingForges != null) {
                    var having = ArrayAtIndex(
                        MEMBER_HAVINGEVALUATOR_ARRAY,
                        ExprDotName(Ref("level"), "LevelNumber"));
                    methodNode.Block.IfCondition(
                            Not(
                                ExprDotMethod(
                                    having,
                                    "evaluateHaving",
                                    REF_EPS,
                                    REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturnNoValue();
                }

                var selectExprProcessor = ArrayAtIndex(
                    MEMBER_SELECTEXPRPROCESSOR_ARRAY,
                    ExprDotName(Ref("level"), "LevelNumber"));
                methodNode.Block.ExprDotMethod(
                    Ref("resultEvents"),
                    "Put",
                    Ref("mk"),
                    ExprDotMethod(
                        selectExprProcessor,
                        "Process",
                        Ref("eventsPerStream"),
                        REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        MEMBER_EXPREVALCONTEXT));
            };

            instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedMapUnsorted",
                CodegenNamedParam.From(
                    typeof(bool),
                    "join",
                    typeof(object),
                    "mk",
                    typeof(AggregationGroupByRollupLevel),
                    "level",
                    typeof(EventBean[]),
                    "eventsPerStream",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IDictionary<object, EventBean>),
                    "resultEvents"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static void HandleOutputLimitLastViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            InitGroupRepsPerLevelBufCodegen(instance, forge);

            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
            }

            method.Block.ForEach(typeof(IDictionary<object, EventBean[]>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "Clear");

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar(
                        typeof(EventBean[]),
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar(
                        typeof(EventBean[]),
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar(
                        typeof(object[]),
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach<EventBean>("aNewData", Ref("newData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach<EventBean>("anOldData", Ref("oldData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.MethodReturn(
                LocalMethod(generateAndSort, Ref(NAME_GROUPREPSPERLEVELBUF), REF_ISSYNTHESIZE, Ref("count")));
        }

        private static void HandleOutputLimitLastJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            InitGroupRepsPerLevelBufCodegen(instance, forge);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
            }

            method.Block.ForEach(typeof(IDictionary<object, EventBean[]>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "Clear");

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar(
                        EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                        "newData",
                        Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar(
                        EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                        "oldData",
                        Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar(
                        typeof(object[]),
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldData", Ref("oldData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.MethodReturn(
                LocalMethod(generateAndSort, Ref(NAME_GROUPREPSPERLEVELBUF), REF_ISSYNTHESIZE, Ref("count")));
        }

        private static void HandleOutputLimitAllViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
                method.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar(
                        EPTYPE_MAP_OBJECT_EVENTBEANARRAY,
                        "groupGenerators",
                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), ExprDotName(Ref("level"), "LevelNumber")))
                    .ForEachVar("entry", Ref("groupGenerators"))
                    .LocalMethod(
                        generateOutputBatchedGivenArray,
                        ConstantFalse(),
                        ExprDotName(Ref("entry"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                    .IncrementRef("count");
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar(
                        typeof(EventBean[]),
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar(
                        typeof(EventBean[]),
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar(
                        typeof(object[]),
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach<EventBean>("aNewData", Ref("newData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach<EventBean>("anOldData", Ref("oldData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.MethodReturn(
                LocalMethod(generateAndSort, Ref(NAME_GROUPREPSPERLEVELBUF), REF_ISSYNTHESIZE, Ref("count")));
        }

        private static void HandleOutputLimitAllJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
                method.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar(
                        EPTYPE_MAP_OBJECT_EVENTBEANARRAY,
                        "groupGenerators",
                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), ExprDotName(Ref("level"), "LevelNumber")))
                    .ForEachVar("entry", Ref("groupGenerators"))
                    .LocalMethod(
                        generateOutputBatchedGivenArray,
                        ConstantFalse(),
                        ExprDotName(Ref("entry"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                    .IncrementRef("count");
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar(
                        EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                        "newData",
                        Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar(
                        EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                        "oldData",
                        Cast(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar(
                        typeof(object[]),
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "aNewData", Ref("newData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "anOldData", Ref("oldData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                            .DeclareVar<object>("groupKeyComplete",
                                LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>("groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Put",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .IncrementRef("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
            }

            method.Block.MethodReturn(
                LocalMethod(generateAndSort, Ref(NAME_GROUPREPSPERLEVELBUF), REF_ISSYNTHESIZE, Ref("count")));
        }

        private static CodegenMethod GenerateOutputBatchedCollectViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatched = GenerateOutputBatchedCodegen(forge, instance, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]),
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                var forLevels = methodNode.Block.ForEach<AggregationGroupByRollupLevel>("level", Ref("levels"));
                {
                    var forEvents = forLevels.ForEachVar(
                        "pair",
                        ArrayAtIndex(Ref("eventPairs"), ExprDotName(Ref("level"), "LevelNumber")));
                    forEvents.AssignArrayElement(
                            "eventsPerStream",
                            Constant(0),
                            Cast(typeof(EventBean), ExprDotName(Ref("pair"), "Value")))
                        .LocalMethod(
                            generateOutputBatched,
                            ExprDotName(Ref("pair"), "Key"),
                            Ref("level"),
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            Ref("events"),
                            Ref("sortKey"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedCollectView",
                CodegenNamedParam.From(
                    EPTYPE_MAPARRAY_OBJECT_EVENTBEAN,
                    "eventPairs",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "events",
                    typeof(IList<object>),
                    "sortKey",
                    typeof(EventBean[]),
                    "eventsPerStream"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateOutputBatchedCollectJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatched = GenerateOutputBatchedCodegen(forge, instance, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]),
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                var forLevels = methodNode.Block.ForEach<AggregationGroupByRollupLevel>("level", Ref("levels"));
                {
                    var forEvents = forLevels.ForEachVar(
                        "pair",
                        ArrayAtIndex(Ref("eventPairs"), ExprDotName(Ref("level"), "LevelNumber")));
                    forEvents.LocalMethod(
                        generateOutputBatched,
                        ExprDotName(Ref("pair"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Value")),
                        REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        Ref("events"),
                        Ref("sortKey"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedCollectJoin",
                CodegenNamedParam.From(
                    EPTYPE_MAPARRAY_OBJECT_EVENTBEANARRAY,
                    "eventPairs",
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "events",
                    typeof(IList<object>),
                    "sortKey"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        private static CodegenMethod ResetEventPerGroupBufCodegen(
            string memberName,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => methodNode.Block
                .ForEachVar("anEventPerGroupBuf", Ref(memberName))
                .ExprDotMethod(Ref("anEventPerGroupBuf"), "Clear");

            return instance.Methods.AddMethod(
                typeof(void),
                "ResetEventPerGroupBuf",
                Collections.GetEmptyList<CodegenNamedParam>(),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        internal static CodegenMethod GenerateGroupKeysViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNullReturnNull("events")
                    .DeclareVar(
                        typeof(object[][]),
                        "result",
                        NewArrayByLength(typeof(object[]), ArrayLength(Ref("events"))))
                    .DeclareVar(
                        typeof(EventBean[]),
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]),
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));
                {
                    var forLoop = methodNode.Block.ForLoopIntSimple("i", ArrayLength(Ref("events")));
                    forLoop.AssignArrayElement("eventsPerStream", Constant(0), ArrayAtIndex(Ref("events"), Ref("i")))
                        .DeclareVar<object>("groupKeyComplete",
                            LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), REF_ISNEWDATA))
                        .AssignArrayElement(
                            "result",
                            Ref("i"),
                            NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))));
                    {
                        forLoop.ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                            .DeclareVar<object>("subkey",
                                ExprDotMethod(
                                    ArrayAtIndex(Ref("levels"), Ref("j")),
                                    "computeSubkey",
                                    Ref("groupKeyComplete")))
                            .AssignArrayElement2Dim("result", Ref("i"), Ref("j"), Ref("subkey"))
                            .ExprDotMethod(
                                ArrayAtIndex(
                                    Ref("eventPerKey"),
                                    ExprDotName(ArrayAtIndex(Ref("levels"), Ref("j")), "LevelNumber")),
                                "Put",
                                Ref("subkey"),
                                ArrayAtIndex(Ref("events"), Ref("i")));
                    }
                }

                methodNode.Block.MethodReturn(Ref("result"));
            };

            return instance.Methods.AddMethod(
                typeof(object[][]),
                "GenerateGroupKeysView",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "events",
                    typeof(IDictionary<object, EventBean>[]),
                    "eventPerKey",
                    typeof(bool),
                    NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateGroupKeysJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(Or(EqualsNull(Ref("events")), ExprDotMethod(Ref("events"), "IsEmpty")))
                    .BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(object[][]),
                        "result",
                        NewArrayByLength(typeof(object[]), ExprDotName(Ref("events"), "Count")))
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]),
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar(typeof(int), "count", Constant(-1));
                {
                    var forLoop = methodNode.Block.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "eventrow", Ref("events"));
                    forLoop.IncrementRef("count")
                        .DeclareVar(
                            typeof(EventBean[]),
                            "eventsPerStream",
                            Cast(typeof(EventBean[]), ExprDotName(Ref("eventrow"), "Array")))
                        .DeclareVar(
                            typeof(object),
                            "groupKeyComplete",
                            LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), REF_ISNEWDATA))
                        .AssignArrayElement(
                            "result",
                            Ref("count"),
                            NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))));
                    {
                        forLoop.ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                            .DeclareVar(
                                typeof(object),
                                "subkey",
                                ExprDotMethod(
                                    ArrayAtIndex(Ref("levels"), Ref("j")),
                                    "computeSubkey",
                                    Ref("groupKeyComplete")))
                            .AssignArrayElement2Dim("result", Ref("count"), Ref("j"), Ref("subkey"))
                            .ExprDotMethod(
                                ArrayAtIndex(
                                    Ref("eventPerKey"),
                                    ExprDotName(ArrayAtIndex(Ref("levels"), Ref("j")), "LevelNumber")),
                                "Put",
                                Ref("subkey"),
                                Ref("eventsPerStream"));
                    }
                }

                methodNode.Block.MethodReturn(Ref("result"));
            };

            return instance.Methods.AddMethod(
                typeof(object[][]),
                "GenerateGroupKeysJoin",
                CodegenNamedParam.From(
                    EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN,
                    "events",
                    typeof(IDictionary<object, EventBean[]>[]),
                    "eventPerKey",
                    typeof(bool),
                    NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateAndSortCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatched = GenerateOutputBatchedCodegen(forge, instance, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(typeof(EventBean[]), "oldEventsArr", ConstantNull())
                    .DeclareVar(typeof(object[]), "oldEventSortKeys", ConstantNull());

                if (forge.IsSelectRStream) {
                    methodNode.Block.IfCondition(Relational(Ref("oldEventCount"), GT, Constant(0)))
                        .DeclareVar(
                            typeof(EventsAndSortKeysPair),
                            "pair",
                            StaticMethod(
                                typeof(ResultSetProcessorRowPerGroupRollupUtil),
                                METHOD_GETOLDEVENTSSORTKEYS,
                                Ref("oldEventCount"),
                                Ref(NAME_RSTREAMEVENTSORTARRAYBUF),
                                MEMBER_ORDERBYPROCESSOR,
                                ExprDotName(Ref("this"), "GroupByRollupDesc")))
                        .AssignRef("oldEventsArr", ExprDotName(Ref("pair"), "Events"))
                        .AssignRef("oldEventSortKeys", ExprDotName(Ref("pair"), "SortKeys"));
                }

                methodNode.Block.DeclareVar(
                        typeof(IList<EventBean>),
                        "newEvents",
                        NewInstance(typeof(List<EventBean>)))
                    .DeclareVar(
                        typeof(IList<object>),
                        "newEventsSortKey",
                        forge.IsSorting ? NewInstance(typeof(List<object>)) : ConstantNull());

                methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        ExprDotMethodChain(Ref("this")).Add("GetGroupByRollupDesc").Get("Levels"))
                    .DeclareVar(
                        EPTYPE_MAP_OBJECT_EVENTBEANARRAY,
                        "groupGenerators",
                        ArrayAtIndex(
                            Ref("outputLimitGroupRepsPerLevel"),
                            ExprDotName(Ref("level"), "LevelNumber")))
                    .ForEachVar("entry", Ref("groupGenerators"))
                    .LocalMethod(
                        generateOutputBatched,
                        ExprDotName(Ref("entry"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));

                methodNode.Block.DeclareVar(
                    typeof(EventBean[]),
                    "newEventsArr",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));
                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar(
                            typeof(object[]),
                            "sortKeysNew",
                            StaticMethod(
                                typeof(CollectionUtil),
                                METHOD_TOARRAYNULLFOREMPTYOBJECTS,
                                Ref("newEventsSortKey")))
                        .AssignRef(
                            "newEventsArr",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "SortWOrderKeys",
                                Ref("newEventsArr"),
                                Ref("sortKeysNew"),
                                MEMBER_EXPREVALCONTEXT));
                    if (forge.IsSelectRStream) {
                        methodNode.Block.AssignRef(
                            "oldEventsArr",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "SortWOrderKeys",
                                Ref("oldEventsArr"),
                                Ref("oldEventSortKeys"),
                                MEMBER_EXPREVALCONTEXT));
                    }
                }

                methodNode.Block.MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("newEventsArr"),
                        Ref("oldEventsArr")));
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "GenerateAndSort",
                CodegenNamedParam.From(
                    EPTYPE_MAPARRAY_OBJECT_EVENTBEANARRAY,
                    "outputLimitGroupRepsPerLevel",
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(int),
                    "oldEventCount"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        internal static void ApplyViewResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysRow = GenerateGroupKeysRowCodegen(forge, classScope, instance);

            method.Block.DeclareVar(
                typeof(EventBean[]),
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    ifNew.ForEach<EventBean>("aNewData", REF_NEWDATA)
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("aNewData"))
                        .DeclareVar(
                            typeof(object[]),
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            MEMBER_EXPREVALCONTEXT);
                }
            }
            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    ifOld.ForEach<EventBean>("anOldData", REF_OLDDATA)
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("anOldData"))
                        .DeclareVar(
                            typeof(object[]),
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantFalse()))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            MEMBER_EXPREVALCONTEXT);
                }
            }
        }

        public static void ApplyJoinResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysRow = GenerateGroupKeysRowCodegen(forge, classScope, instance);

            method.Block.DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");
            {
                var ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    ifNew.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "mk", REF_NEWDATA)
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotName(Ref("mk"), "Array")))
                        .DeclareVar(
                            typeof(object[]),
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            MEMBER_EXPREVALCONTEXT);
                }
            }
            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    ifOld.ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "mk", REF_OLDDATA)
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotName(Ref("mk"), "Array")))
                        .DeclareVar(
                            typeof(object[]),
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantFalse()))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            MEMBER_EXPREVALCONTEXT);
                }
            }
        }

        private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            string methodName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            CodegenExpression eventTypes = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));

            if (forge.IsOutputAll) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowPerGroupRollupOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerGroupRollupAll",
                        MEMBER_EXPREVALCONTEXT,
                        Ref("this"),
                        Constant(forge.GroupKeyTypes),
                        eventTypes,
                        forge.OutputAllSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTALLHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
            else if (forge.IsOutputLast) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowPerGroupRollupOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerGroupRollupLast",
                        MEMBER_EXPREVALCONTEXT,
                        Ref("this"),
                        Constant(forge.GroupKeyTypes),
                        eventTypes,
                        forge.OutputLastSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTLASTHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessJoin", classScope, method, instance);
        }

        internal static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
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
            ResultSetProcessorRowPerGroupRollupForge forge,
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

        internal static void StopMethodCodegenBound(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                method.Block.ForEach(
                        typeof(ResultSetProcessorGroupedOutputFirstHelper),
                        "helper",
                        Member(NAME_OUTPUTFIRSTHELPERS))
                    .ExprDotMethod(Ref("helper"), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "Destroy");
            }
        }

        private static CodegenMethod GenerateGroupKeysRowCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => methodNode.Block.DeclareVar(
                    typeof(object),
                    "groupKeyComplete",
                    LocalMethod(forge.GenerateGroupKeySingle, Ref("eventsPerStream"), REF_ISNEWDATA))
                .DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]),
                    "levels",
                    ExprDotMethodChain(Ref("this")).Add("GetGroupByRollupDesc").Get("Levels"))
                .DeclareVar(typeof(object[]), "result", NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))))
                .ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                .DeclareVar(
                    typeof(object),
                    "subkey",
                    ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "ComputeSubkey", Ref("groupKeyComplete")))
                .AssignArrayElement("result", Ref("j"), Ref("subkey"))
                .BlockEnd()
                .MethodReturn(Ref("result"));

            return instance.Methods.AddMethod(
                typeof(object[]),
                "GenerateGroupKeysRow",
                CodegenNamedParam.From(typeof(EventBean[]), "eventsPerStream", typeof(bool), NAME_ISNEWDATA),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static void InitGroupRepsPerLevelBufCodegen(
            CodegenInstanceAux instance,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            if (!instance.HasMember(NAME_GROUPREPSPERLEVELBUF)) {
                instance.AddMember(NAME_GROUPREPSPERLEVELBUF, typeof(IDictionary<object, EventBean[]>[]));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_GROUPREPSPERLEVELBUF,
                    StaticMethod(
                        typeof(ResultSetProcessorRowPerGroupRollupUtil),
                        METHOD_MAKEGROUPREPSPERLEVELBUF,
                        Constant(forge.GroupByRollupDesc.Levels.Length)));
            }
        }

        private static void InitRStreamEventsSortArrayBufCodegen(
            CodegenInstanceAux instance,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            if (!instance.HasMember(NAME_RSTREAMEVENTSORTARRAYBUF)) {
                instance.AddMember(NAME_RSTREAMEVENTSORTARRAYBUF, typeof(EventArrayAndSortKeyArray));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_RSTREAMEVENTSORTARRAYBUF,
                    StaticMethod(
                        typeof(ResultSetProcessorRowPerGroupRollupUtil),
                        METHOD_MAKERSTREAMSORTEDARRAYBUF,
                        Constant(forge.GroupByRollupDesc.Levels.Length),
                        Constant(forge.IsSorting)));
            }
        }

        private static void InitOutputFirstHelpers(
            CodegenExpressionInstanceField outputConditionFactory,
            CodegenInstanceAux instance,
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope)
        {
            if (!instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                var factory = classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
                instance.AddMember(NAME_OUTPUTFIRSTHELPERS, typeof(ResultSetProcessorGroupedOutputFirstHelper[]));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTFIRSTHELPERS,
                    StaticMethod(
                        typeof(ResultSetProcessorRowPerGroupRollupUtil),
                        "InitializeOutputFirstHelpers",
                        factory,
                        MEMBER_EXPREVALCONTEXT,
                        Constant(forge.GroupKeyTypes),
                        ExprDotName(Ref("this"), "GroupByRollupDesc"),
                        outputConditionFactory,
                        forge.OutputFirstSettings.ToExpression()));
            }
        }
    }
} // end of namespace