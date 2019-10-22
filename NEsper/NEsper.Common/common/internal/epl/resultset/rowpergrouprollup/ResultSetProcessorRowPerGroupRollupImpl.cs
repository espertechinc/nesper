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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
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

        protected internal static void ProcessJoinResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            AddEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFJOIN, forge, instance, method, classScope);
            var resetEventPerGroupJoinBuf =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFJOIN, classScope, instance);
            var generateGroupKeysJoin = GenerateGroupKeysJoinCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);

            if (forge.IsUnidirectional) {
                method.Block.ExprDotMethod(Ref("this"), "Clear");
            }

            method.Block.InstanceMethod(resetEventPerGroupJoinBuf)
                .DeclareVar<object[][]>(
                    "newDataMultiKey",
                    LocalMethod(generateGroupKeysJoin, REF_NEWDATA, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue()))
                .DeclareVar<object[][]>(
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
                    REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT,
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

        protected internal static void ProcessViewResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            AddEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFVIEW, forge, instance, method, classScope);
            var resetEventPerGroupBufView =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFVIEW, classScope, instance);
            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            method.Block.InstanceMethod(resetEventPerGroupBufView)
                .DeclareVar<object[][]>(
                    "newDataMultiKey",
                    LocalMethod(generateGroupKeysView, REF_NEWDATA, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue()))
                .DeclareVar<object[][]>(
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
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (!instance.HasMember(memberName)) {
                var init = method.MakeChild(
                    typeof(LinkedHashMap<object, EventBean[]>[]),
                    typeof(ResultSetProcessorRowPerGroupRollupImpl),
                    classScope);
                instance.AddMember(memberName, typeof(LinkedHashMap<object, EventBean[]>[]));
                instance.ServiceCtor.Block.AssignRef(memberName, LocalMethod(init));
                var levelCount = forge.GroupByRollupDesc.Levels.Length;
                init.Block.DeclareVar<LinkedHashMap<object, EventBean[]>[]>(
                        memberName,
                        NewArrayByLength(typeof(LinkedHashMap<object, EventBean[]>), Constant(levelCount)))
                    .ForLoopIntSimple("i", Constant(levelCount))
                    .AssignArrayElement(memberName, Ref("i"), NewInstance(typeof(LinkedHashMap<object, EventBean[]>)))
                    .BlockEnd()
                    .MethodReturn(Ref(memberName));
            }
        }

        protected internal static CodegenMethod GenerateOutputEventsViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<EventBean[]>(
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar<List<EventBean>>("events", NewInstance<List<EventBean>>(Constant(1)))
                    .DeclareVar<IList<GroupByRollupKey>>(
                        "currentGenerators",
                        forge.IsSorting ? NewInstance<List<GroupByRollupKey>>(Constant(1)) : ConstantNull())
                    .DeclareVar<AggregationGroupByRollupLevel[]>(
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                {
                    var forLevels = methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        Ref("levels"));
                    {
                        var forEvents = forLevels.ForEach(
                            typeof(KeyValuePair<object, EventBean[]>),
                            "entry",
                             ArrayAtIndex(Ref("keysAndEvents"), ExprDotName(Ref("level"), "LevelNumber")));
                        forEvents.DeclareVar<object>("groupKey", ExprDotName(Ref("entry"), "Key"))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "SetCurrentAccess",
                                Ref("groupKey"),
                                ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                                Ref("level"))
                            .AssignArrayElement(
                                Ref("eventsPerStream"),
                                Constant(0),
                                Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")));

                        if (forge.PerLevelForges.OptionalHavingForges != null) {
                            var having = ArrayAtIndex(
                                REF_HAVINGEVALUATOR_ARRAY,
                                ExprDotName(Ref("level"), "LevelNumber"));
                            forEvents.IfCondition(
                                    Not(
                                        ExprDotMethod(
                                            having,
                                            "EvaluateHaving",
                                            REF_EPS,
                                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                            REF_AGENTINSTANCECONTEXT)))
                                .BlockContinue();
                        }

                        forEvents.ExprDotMethod(
                            Ref("events"),
                            "Add",
                            ExprDotMethod(
                                ArrayAtIndex(
                                    REF_SELECTEXPRPROCESSOR_ARRAY,
                                    ExprDotName(Ref("level"), "LevelNumber")),
                                "Process",
                                Ref("eventsPerStream"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT));

                        if (forge.IsSorting) {
                            forEvents.DeclareVar<EventBean[]>(
                                    "currentEventsPerStream",
                                    NewArrayWithInit(
                                        typeof(EventBean),
                                        Cast(typeof(EventBean), ExprDotName(Ref("entry"), "Value"))))
                                .ExprDotMethod(
                                    Ref("currentGenerators"),
                                    "Add",
                                    NewInstance<GroupByRollupKey>(
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
                                REF_ORDERBYPROCESSOR,
                                "SortRollup",
                                Ref("outgoing"),
                                Ref("currentGenerators"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT,
                                REF_AGGREGATIONSVC));
                }

                methodNode.Block.MethodReturn(Ref("outgoing"));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GenerateOutputEventsView",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, EventBean[]>[]), "keysAndEvents",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE),
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
                methodNode.Block.DeclareVar<List<EventBean>>(
                        "events",
                        NewInstance<List<EventBean>>(Constant(1)))
                    .DeclareVar<IList<GroupByRollupKey>>(
                        "currentGenerators",
                        forge.IsSorting ? NewInstance<List<GroupByRollupKey>>(Constant(1)) : ConstantNull())
                    .DeclareVar<AggregationGroupByRollupLevel[]>(
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                {
                    var forLevels = methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        Ref("levels"));
                    {
                        var forEvents = forLevels.ForEach(
                            typeof(KeyValuePair<object, EventBean[]>),
                            "entry",
                            ArrayAtIndex(Ref("eventPairs"), ExprDotName(Ref("level"), "LevelNumber")));
                        forEvents.DeclareVar<object>("groupKey", ExprDotName(Ref("entry"), "Key"))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "SetCurrentAccess",
                                Ref("groupKey"),
                                ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                                Ref("level"))
                            .DeclareVar<EventBean[]>(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")));

                        if (forge.PerLevelForges.OptionalHavingForges != null) {
                            var having = ArrayAtIndex(
                                REF_HAVINGEVALUATOR_ARRAY,
                                ExprDotName(Ref("level"), "LevelNumber"));
                            forEvents.IfCondition(
                                    Not(
                                        ExprDotMethod(
                                            having,
                                            "EvaluateHaving",
                                            REF_EPS,
                                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                            REF_AGENTINSTANCECONTEXT)))
                                .BlockContinue();
                        }

                        forEvents.ExprDotMethod(
                            Ref("events"),
                            "Add",
                            ExprDotMethod(
                                ArrayAtIndex(
                                    REF_SELECTEXPRPROCESSOR_ARRAY,
                                    ExprDotName(Ref("level"), "LevelNumber")),
                                "Process",
                                Ref("eventsPerStream"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT));

                        if (forge.IsSorting) {
                            forEvents.ExprDotMethod(
                                Ref("currentGenerators"),
                                "Add",
                                NewInstance<GroupByRollupKey>(
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
                    methodNode.Block
                        .IfCondition(Relational(ArrayLength(Ref("outgoing")), GT, Constant(1)))
                        .BlockReturn(
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "SortRollup",
                                Ref("outgoing"),
                                Ref("currentGenerators"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                REF_AGENTINSTANCECONTEXT,
                                REF_AGGREGATIONSVC));
                }

                methodNode.Block.MethodReturn(Ref("outgoing"));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GenerateOutputEventsJoin",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, EventBean[]>[]), "eventPairs",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA, 
                    typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        protected internal static void GetIteratorViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
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

            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "ClearResults", REF_AGENTINSTANCECONTEXT)
                .DeclareVar<IEnumerator<EventBean>>("it", ExprDotMethod(REF_VIEWABLE, "GetEnumerator"))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar<object[]>(
                    "groupKeys",
                    NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                .DeclareVar<AggregationGroupByRollupLevel[]>(
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));
            {
                method.Block.WhileLoop(ExprDotMethod(Ref("it"), "MoveNext"))
                    .AssignArrayElement(
                        Ref("eventsPerStream"),
                        Constant(0),
                        Cast(typeof(EventBean), ExprDotName(Ref("it"), "Current")))
                    .DeclareVar<object>(
                        "groupKeyComplete",
                        LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                    .ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                    .DeclareVar<object>(
                        "subkey",
                        ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "ComputeSubkey", Ref("groupKeyComplete")))
                    .AssignArrayElement("groupKeys", Ref("j"), Ref("subkey"))
                    .BlockEnd()
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC,
                        "ApplyEnter",
                        Ref("eventsPerStream"),
                        Ref("groupKeys"),
                        REF_AGENTINSTANCECONTEXT)
                    .BlockEnd();
            }

            method.Block.DeclareVar<ArrayDeque<EventBean>>(
                    "deque",
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_ITERATORTODEQUE,
                        LocalMethod(ObtainIteratorCodegen(forge, classScope, method, instance), REF_VIEWABLE)))
                .ExprDotMethod(REF_AGGREGATIONSVC, "ClearResults", REF_AGENTINSTANCECONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "GetEnumerator"));
        }

        private static CodegenMethod ObtainIteratorCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent,
            CodegenInstanceAux instance)
        {
            var resetEventPerGroupBufView =
                ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFVIEW, classScope, instance);
            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            var iterator = parent
                .MakeChild(typeof(IEnumerator<EventBean>), typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope)
                .AddParam(typeof(Viewable), NAME_VIEWABLE);
            iterator.Block.InstanceMethod(resetEventPerGroupBufView)
                .DeclareVar<EventBean[]>(
                    "events",
                    StaticMethod(
                        typeof(CollectionUtil),
                        METHOD_ENUMERATORTOARRAYEVENTS,
                        ExprDotMethod(REF_VIEWABLE, "GetEnumerator")))
                .InstanceMethod(generateGroupKeysView, Ref("events"), Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue())
                .DeclareVar<EventBean[]>(
                    "output",
                    LocalMethod(
                        generateOutputEventsView,
                        Ref(NAME_EVENTPERGROUPBUFVIEW),
                        ConstantTrue(),
                        ConstantTrue()))
                .MethodReturn(
                    ExprDotMethod(
                        StaticMethod(typeof(Arrays), "Enumerate", Ref("output")),
                        "GetEnumerator"));
            return iterator;
        }

        protected internal static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessView", classScope, method, instance);
        }

        protected internal static void GetIteratorJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysJoin = GenerateGroupKeysJoinCodegen(forge, classScope, instance);
            var generateOutputEventsJoin = GenerateOutputEventsJoinCodegen(forge, classScope, instance);
            var resetEventPerGroupBuf = ResetEventPerGroupBufCodegen(NAME_EVENTPERGROUPBUFJOIN, classScope, instance);
            method.Block.InstanceMethod(resetEventPerGroupBuf)
                .InstanceMethod(generateGroupKeysJoin, REF_JOINSET, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue())
                .DeclareVar<EventBean[]>(
                    "output",
                    LocalMethod(
                        generateOutputEventsJoin,
                        Ref(NAME_EVENTPERGROUPBUFJOIN),
                        ConstantTrue(),
                        ConstantTrue()))
                .MethodReturn(
                    ExprDotMethod(
                        StaticMethod(typeof(Arrays), "Enumerate", Ref("output")),
                        "GetEnumerator"));
        }

        protected internal static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "ClearResults", REF_AGENTINSTANCECONTEXT);
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                HandleOutputLimitDefaultJoinCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                HandleOutputLimitAllJoinCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                HandleOutputLimitFirstJoinCodegen(forge, classScope, method, instance);
                return;
            }

            HandleOutputLimitLastJoinCodegen(forge, classScope, method, instance);
        }

        protected internal static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var outputLimitLimitType = forge.OutputLimitSpec.DisplayLimit;
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                HandleOutputLimitDefaultViewCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                HandleOutputLimitAllViewCodegen(forge, classScope, method, instance);
                return;
            }

            if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                HandleOutputLimitFirstViewCodegen(forge, classScope, method, instance);
                return;
            }

            HandleOutputLimitLastViewCodegen(forge, classScope, method, instance);
        }

        protected internal static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Ref(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Ref(NAME_OUTPUTALLHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                method.Block.ForEach(
                        typeof(ResultSetProcessorGroupedOutputFirstHelper),
                        "helper",
                        Ref(NAME_OUTPUTFIRSTHELPERS))
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

            method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "Reset");

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

            method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "Reset");

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
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
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
                    var forEach = methodNode.Block.ForEach(
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
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                        .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifNewFirst = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNewFirst = ifNewFirst.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forNewFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "EvaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantTrue(),
                                                REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }

                        var ifOldFirst = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOldFirst = ifOldFirst.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forOldFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "EvaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantFalse(),
                                                REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "handleOutputLimitFirstViewHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>), "viewEventsList",
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<object>[]), "oldEventsPerLevel",
                    typeof(IList<object>[]), "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        private static CodegenMethod HandleOutputLimitFirstJoinNoHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
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
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<object>>(
                            "newData",
                            Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "First")))
                        .DeclareVar<ISet<object>>(
                            "oldData",
                            Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "Second")))
                        .DeclareVar<object[]>(
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)));

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(MultiKey<object>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var forLvl = forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(MultiKey<object>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                var forLvl = forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "handleOutputLimitFirstJoinNoHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>),
                    NAME_JOINEVENTSSET,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>[]),
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
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
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
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar<ISet<EventBean>>(
                            "newData",
                            ExprDotName(Ref("pair"), "First"))
                        .DeclareVar<ISet<EventBean>>(
                            "oldData",
                            ExprDotName(Ref("pair"), "Second"))
                        .DeclareVar<object[]>(
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)));
                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifNewFirst = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNewFirst = ifNewFirst.ForEach(
                                    typeof(MultiKey<EventBean>),
                                    "aNewData",
                                    Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forNewFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "EvaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantTrue(),
                                                REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }

                        var ifOldFirst = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOldFirst = ifOldFirst.ForEach(
                                    typeof(MultiKey<EventBean>),
                                    "anOldData",
                                    Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forOldFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC,
                                        "SetCurrentAccess",
                                        Ref("groupKey"),
                                        ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                                        Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotName(Ref("level"), "LevelNumber")),
                                                "EvaluateHaving",
                                                Ref("eventsPerStream"),
                                                ConstantFalse(),
                                                REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "handleOutputLimitFirstJoinHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>),
                    NAME_JOINEVENTSSET,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>[]),
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
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
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
                    var forEach = methodNode.Block.ForEach(
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
                            "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                        .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var forLvl = forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantTrue(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyEnter",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar<object>(
                                    "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                var forLvl = forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel),
                                        "level",
                                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                    .DeclareVar<object>(
                                        "groupKey",
                                        ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar<OutputConditionPolled>(
                                        "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber),
                                            "GetOrAllocate",
                                            Ref("groupKey"),
                                            REF_AGENTINSTANCECONTEXT,
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
                                            "Push",
                                            Ref("groupKey"),
                                            Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.InstanceMethod(
                                            generateOutputBatchedGivenArray,
                                            ConstantFalse(),
                                            Ref("groupKey"),
                                            Ref("level"),
                                            Ref("eventsPerStream"),
                                            ConstantFalse(),
                                            REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"),
                                            Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC,
                                "ApplyLeave",
                                Ref("eventsPerStream"),
                                Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int),
                "handleOutputLimitFirstNoViewHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>), "viewEventsList",
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<object>[]), "oldEventsPerLevel",
                    typeof(IList<object>[]), "oldEventsSortKeyPerLevel"),
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

            method.Block.DeclareVar<EventBean[]>(
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"))
                    .InstanceMethod(resetEventPerGroupBufView)
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysView,
                            Ref("newData"),
                            Ref(NAME_EVENTPERGROUPBUFVIEW),
                            ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysView,
                            Ref("oldData"),
                            Ref(NAME_EVENTPERGROUPBUFVIEW),
                            ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
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
                        REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT,
                        Ref("newData"),
                        Ref("newDataMultiKey"),
                        Ref("oldData"),
                        Ref("oldDataMultiKey"),
                        Ref("eventsPerStream"))
                    .InstanceMethod(
                        generateOutputBatchedCollectView,
                        Ref(NAME_EVENTPERGROUPBUFVIEW),
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

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                forEach
                    .DeclareVar<ISet<object>>(
                        "newData",
                        Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar<ISet<object>>(
                        "oldData",
                        Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "Second")))
                    .InstanceMethod(resetEventPerGroupBufJoin)
                    .DeclareVar<object[]>(
                        "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysJoin,
                            Ref("newData"),
                            Ref(NAME_EVENTPERGROUPBUFJOIN),
                            ConstantTrue()))
                    .DeclareVar<object[]>(
                        "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysJoin,
                            Ref("oldData"),
                            Ref(NAME_EVENTPERGROUPBUFJOIN),
                            ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.InstanceMethod(
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
                        REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT,
                        Ref("newData"),
                        Ref("newDataMultiKey"),
                        Ref("oldData"),
                        Ref("oldDataMultiKey"))
                    .InstanceMethod(
                        generateOutputBatchedCollectJoin,
                        Ref(NAME_EVENTPERGROUPBUFJOIN),
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

        protected internal static void RemovedAggregationGroupKeyCodegen(
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
            Consumer<CodegenMethod> code = methodNode => methodNode.Block
                .DeclareVar<IList<EventBean>>("resultList", ArrayAtIndex(Ref("resultEvents"), ExprDotName(Ref("level"), "LevelNumber")))
                .DeclareVarNoInit(typeof(IList<object>), "sortKeys")
                .IfCondition(EqualsNull(Ref("optSortKeys")))
                .AssignRef("sortKeys", ConstantNull())
                .IfElse()
                .AssignRef("sortKeys", ArrayAtIndex(Ref("optSortKeys"), ExprDotName(Ref("level"), "LevelNumber")))
                .BlockEnd()
                .InstanceMethod(
                    generateOutputBatched,
                    Ref("mk"),
                    Ref("level"),
                    Ref("eventsPerStream"),
                    ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    Ref("resultList"),
                    Ref("sortKeys"));
            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedGivenArrayCodegen",
                CodegenNamedParam.From(
                    typeof(bool), "join",
                    typeof(object), "mk",
                    typeof(AggregationGroupByRollupLevel), "level",
                    typeof(EventBean[]), "eventsPerStream",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>[]), "resultEvents",
                    typeof(IList<object>[]), "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                    Ref("level"));

                if (forge.PerLevelForges.OptionalHavingForges != null) {
                    methodNode.Block.IfCondition(
                            Not(
                                ExprDotMethod(
                                    ArrayAtIndex(
                                        REF_HAVINGEVALUATOR_ARRAY,
                                        ExprDotName(Ref("level"), "LevelNumber")),
                                    "EvaluateHaving",
                                    Ref("eventsPerStream"),
                                    ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturnNoValue();
                }

                var selectExprProcessor = ArrayAtIndex(
                    REF_SELECTEXPRPROCESSOR_ARRAY,
                    ExprDotName(Ref("level"), "LevelNumber"));
                methodNode.Block.ExprDotMethod(
                    Ref("resultEvents"),
                    "Add",
                    ExprDotMethod(
                        selectExprProcessor,
                        "Process",
                        Ref("eventsPerStream"),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        REF_AGENTINSTANCECONTEXT));

                if (forge.IsSorting) {
                    methodNode.Block.ExprDotMethod(
                        Ref("optSortKeys"),
                        "Add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR,
                            "GetSortKeyRollup",
                            Ref("eventsPerStream"),
                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                            REF_AGENTINSTANCECONTEXT,
                            Ref("level")));
                }
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatched",
                CodegenNamedParam.From(
                    typeof(object), "mk",
                    typeof(AggregationGroupByRollupLevel), "level",
                    typeof(EventBean[]), "eventsPerStream",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>), "resultEvents",
                    typeof(IList<object>), "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        protected internal static void GenerateOutputBatchedMapUnsortedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC,
                    "SetCurrentAccess",
                    Ref("mk"),
                    ExprDotName(REF_AGENTINSTANCECONTEXT, "AgentInstanceId"),
                    Ref("level"));

                if (forge.PerLevelForges.OptionalHavingForges != null) {
                    var having = ArrayAtIndex(REF_HAVINGEVALUATOR_ARRAY, ExprDotName(Ref("level"), "LevelNumber"));
                    methodNode.Block.IfCondition(
                            Not(
                                ExprDotMethod(
                                    having,
                                    "EvaluateHaving",
                                    REF_EPS,
                                    ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturnNoValue();
                }

                var selectExprProcessor = ArrayAtIndex(
                    REF_SELECTEXPRPROCESSOR_ARRAY,
                    ExprDotName(Ref("level"), "LevelNumber"));
                methodNode.Block.ExprDotMethod(
                    Ref("resultEvents"),
                    "Push",
                    Ref("mk"),
                    ExprDotMethod(
                        selectExprProcessor,
                        "Process",
                        Ref("eventsPerStream"),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        REF_AGENTINSTANCECONTEXT));
            };

            instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedMapUnsorted",
                CodegenNamedParam.From(
                    typeof(bool), "join",
                    typeof(object), "mk",
                    typeof(AggregationGroupByRollupLevel), "level",
                    typeof(EventBean[]), "eventsPerStream",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IDictionary<object, EventBean>), "resultEvents"),
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

            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "Reset");
            }

            method.Block.ForEach(typeof(IDictionary<object, EventBean[]>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "Clear");

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"))
                    .DeclareVar<object[]>(
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
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
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "Reset");
            }

            method.Block.ForEach(typeof(IDictionary<object, EventBean[]>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "Clear");

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<object>>(
                        "newData",
                        Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar<ISet<object>>(
                        "oldData",
                        Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar<object[]>(
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
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
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "Reset");
                method.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar<IDictionary<object, EventBean>>(
                        "groupGenerators",
                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), ExprDotName(Ref("level"), "LevelNumber")))
                    .ForEach(
                        typeof(KeyValuePair<object, EventBean>),
                        "entry",
                        Ref("groupGenerators"))
                    .InstanceMethod(
                        generateOutputBatchedGivenArray,
                        ConstantFalse(),
                        ExprDotName(Ref("entry"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                    .Increment("count");
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"))
                    .DeclareVar<object[]>(
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
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
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            var levelNumber = ExprDotName(Ref("level"), "LevelNumber");
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar<int>("count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "Reset");
                method.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar<IDictionary<object, object>>(
                        "groupGenerators",
                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), ExprDotName(Ref("level"), "LevelNumber")))
                    .ForEach(
                        typeof(KeyValuePair<object, object>),
                        "entry",
                        Ref("groupGenerators"))
                    .InstanceMethod(
                        generateOutputBatchedGivenArray,
                        ConstantFalse(),
                        ExprDotName(Ref("entry"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")),
                        ConstantFalse(),
                        REF_ISSYNTHESIZE,
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                    .Increment("count");
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar<ISet<object>>(
                        "newData",
                        Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar<ISet<object>>(
                        "oldData",
                        Cast(typeof(ISet<object>), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar<object[]>(
                        "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("aNewData"), "Array")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantTrue(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotName(Ref("anOldData"), "Array")))
                            .DeclareVar<object>(
                                "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel),
                                    "level",
                                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                                .DeclareVar<object>(
                                    "groupKey",
                                    ExprDotMethod(Ref("level"), "ComputeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber),
                                        "Push",
                                        Ref("groupKey"),
                                        Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.InstanceMethod(
                                        generateOutputBatchedGivenArray,
                                        ConstantFalse(),
                                        Ref("groupKey"),
                                        Ref("level"),
                                        Ref("eventsPerStream"),
                                        ConstantFalse(),
                                        REF_ISSYNTHESIZE,
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "EventsPerLevel"),
                                        ExprDotName(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "SortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
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
                methodNode.Block.DeclareVar<AggregationGroupByRollupLevel[]>(
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                var forLevels = methodNode.Block.ForEach(typeof(AggregationGroupByRollupLevel), "level", Ref("levels"));
                {
                    var forEvents = forLevels.ForEach(
                        typeof(KeyValuePair<object, object>),
                        "pair",
                        ArrayAtIndex(
                            Ref("eventPairs"),
                            ExprDotName(Ref("level"), "LevelNumber")));
                    forEvents.AssignArrayElement(
                            "eventsPerStream",
                            Constant(0),
                            Cast(typeof(EventBean), ExprDotName(Ref("pair"), "Value")))
                        .InstanceMethod(
                            generateOutputBatched,
                            ExprDotName(Ref("pair"), "Key"),
                            Ref("level"),
                            Ref("eventsPerStream"),
                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            Ref("events"),
                            Ref("sortKey"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedCollectView",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "eventPairs",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE, 
                    typeof(IList<object>), "events",
                    typeof(IList<object>), "sortKey",
                    typeof(EventBean[]), "eventsPerStream"),
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
                methodNode.Block.DeclareVar<AggregationGroupByRollupLevel[]>(
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));

                var forLevels = methodNode.Block.ForEach(typeof(AggregationGroupByRollupLevel), "level", Ref("levels"));
                {
                    var forEvents = forLevels.ForEach(
                        typeof(KeyValuePair<object, object>),
                        "pair",
                        ArrayAtIndex(
                            Ref("eventPairs"),
                            ExprDotName(Ref("level"), "LevelNumber")));
                    forEvents.InstanceMethod(
                        generateOutputBatched,
                        ExprDotName(Ref("pair"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Value")),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE,
                        Ref("events"),
                        Ref("sortKey"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void),
                "GenerateOutputBatchedCollectJoin",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "eventPairs",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<object>), "events",
                    typeof(IList<object>), "sortKey"),
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

        protected internal static CodegenMethod GenerateGroupKeysViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block
                    .IfRefNullReturnNull("events")
                    .DeclareVar<object[][]>(
                        "result",
                        NewArrayByLength(typeof(object[]), ArrayLength(Ref("events"))))
                    .DeclareVar<EventBean[]>(
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar<AggregationGroupByRollupLevel[]>(
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"));
                {
                    var forLoop = methodNode.Block.ForLoopIntSimple("i", ArrayLength(Ref("events")));
                    forLoop.AssignArrayElement("eventsPerStream", Constant(0), ArrayAtIndex(Ref("events"), Ref("i")))
                        .DeclareVar<object>(
                            "groupKeyComplete",
                            LocalMethod(
                                generateGroupKeySingle,
                                Ref("eventsPerStream"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA))
                        .AssignArrayElement(
                            "result",
                            Ref("i"),
                            NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))));
                    {
                        forLoop.ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                            .DeclareVar<object>(
                                "subkey",
                                ExprDotMethod(
                                    ArrayAtIndex(Ref("levels"), Ref("j")),
                                    "ComputeSubkey",
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
                    typeof(EventBean[]), "events",
                    typeof(IDictionary<object, EventBean[]>[]), "eventPerKey",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupRollupImpl),
                classScope,
                code);
        }

        private static CodegenMethod GenerateGroupKeysJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(Or(EqualsNull(Ref("events")), ExprDotMethod(Ref("events"), "IsEmpty")))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<object[][]>(
                        "result",
                        NewArrayByLength(typeof(object[]), ExprDotName(Ref("events"), "Count")))
                    .DeclareVar<AggregationGroupByRollupLevel[]>(
                        "levels",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar<int>("count", Constant(-1));
                {
                    var forLoop = methodNode.Block.ForEach(typeof(MultiKey<EventBean>), "eventrow", Ref("events"));
                    forLoop.Increment("count")
                        .DeclareVar<EventBean[]>(
                            "eventsPerStream",
                            Cast(typeof(EventBean[]), ExprDotName(Ref("eventrow"), "Array")))
                        .DeclareVar<object>(
                            "groupKeyComplete",
                            LocalMethod(
                                generateGroupKeySingle,
                                Ref("eventsPerStream"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA))
                        .AssignArrayElement(
                            "result",
                            Ref("count"),
                            NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))));
                    {
                        forLoop.ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                            .DeclareVar<object>(
                                "subkey",
                                ExprDotMethod(
                                    ArrayAtIndex(Ref("levels"), Ref("j")),
                                    "ComputeSubkey",
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
                    typeof(ISet<MultiKey<EventBean>>), "events",
                    typeof(IDictionary<object, EventBean[]>[]), "eventPerKey",
                    typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA),
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
                methodNode.Block.DeclareVar<EventBean[]>("oldEventsArr", ConstantNull())
                    .DeclareVar<object[]>("oldEventSortKeys", ConstantNull());

                if (forge.IsSelectRStream) {
                    methodNode.Block.IfCondition(Relational(Ref("oldEventCount"), GT, Constant(0)))
                        .DeclareVar<EventsAndSortKeysPair>(
                            "pair",
                            StaticMethod(
                                typeof(ResultSetProcessorRowPerGroupRollupUtil),
                                ResultSetProcessorRowPerGroupRollupUtil.METHOD_GETOLDEVENTSSORTKEYS,
                                Ref("oldEventCount"),
                                Ref(NAME_RSTREAMEVENTSORTARRAYBUF),
                                REF_ORDERBYPROCESSOR,
                                ExprDotName(Ref("this"), "GroupByRollupDesc")))
                        .AssignRef("oldEventsArr", ExprDotName(Ref("pair"), "Events"))
                        .AssignRef("oldEventSortKeys", ExprDotName(Ref("pair"), "SortKeys"));
                }

                methodNode.Block
                    .DeclareVar<IList<EventBean>>("newEvents", NewInstance(typeof(List<EventBean>)))
                    .DeclareVar<IList<object>>(
                        "newEventsSortKey",
                        forge.IsSorting ? NewInstance(typeof(List<object>)) : ConstantNull());

                methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel),
                        "level",
                        ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                    .DeclareVar<IDictionary<object, EventBean[]>>(
                        "groupGenerators",
                        ArrayAtIndex(
                            Ref("outputLimitGroupRepsPerLevel"),
                            ExprDotName(Ref("level"), "LevelNumber")))
                    .ForEach(
                        typeof(KeyValuePair<object, EventBean[]>),
                        "entry",
                        Ref("groupGenerators"))
                    .InstanceMethod(
                        generateOutputBatched,
                        ExprDotName(Ref("entry"), "Key"),
                        Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotName(Ref("entry"), "Value")),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        Ref("newEvents"),
                        Ref("newEventsSortKey"));

                methodNode.Block.DeclareVar<EventBean[]>(
                    "newEventsArr",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));
                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar<object[]>(
                            "sortKeysNew",
                            StaticMethod(
                                typeof(CollectionUtil),
                                METHOD_TOARRAYNULLFOREMPTYOBJECTS,
                                Ref("newEventsSortKey")))
                        .AssignRef(
                            "newEventsArr",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "SortWOrderKeys",
                                Ref("newEventsArr"),
                                Ref("sortKeysNew"),
                                REF_AGENTINSTANCECONTEXT));
                    if (forge.IsSelectRStream) {
                        methodNode.Block.AssignRef(
                            "oldEventsArr",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR,
                                "SortWOrderKeys",
                                Ref("oldEventsArr"),
                                Ref("oldEventSortKeys"),
                                REF_AGENTINSTANCECONTEXT));
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
                    typeof(IDictionary<object, EventBean[]>[]), "outputLimitGroupRepsPerLevel",
                    typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(int), "oldEventCount"),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        protected internal static void ApplyViewResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysRow = GenerateGroupKeysRowCodegen(forge, classScope, instance);

            method.Block.DeclareVar<EventBean[]>(
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    ifNew.ForEach(typeof(EventBean), "aNewData", REF_NEWDATA)
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("aNewData"))
                        .DeclareVar<object[]>(
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            REF_AGENTINSTANCECONTEXT);
                }
            }
            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    ifOld.ForEach(typeof(EventBean), "anOldData", REF_OLDDATA)
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("anOldData"))
                        .DeclareVar<object[]>(
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantFalse()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            REF_AGENTINSTANCECONTEXT);
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
                    ifNew.ForEach(typeof(MultiKey<EventBean>), "mk", REF_NEWDATA)
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotName(Ref("mk"), "Array")))
                        .DeclareVar<object[]>(
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyEnter",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            REF_AGENTINSTANCECONTEXT);
                }
            }
            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    ifOld.ForEach(typeof(MultiKey<EventBean>), "mk", REF_OLDDATA)
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotName(Ref("mk"), "Array")))
                        .DeclareVar<object[]>(
                            "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantFalse()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC,
                            "ApplyLeave",
                            Ref("eventsPerStream"),
                            Ref("keys"),
                            REF_AGENTINSTANCECONTEXT);
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

            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowPerGroupRollupOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerGroupRollupAll",
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
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowPerGroupRollupOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerGroupRollupLast",
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
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessJoin", classScope, method, instance);
        }

        protected internal static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        protected internal static void StopMethodCodegenBound(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                method.Block.ForEach(
                        typeof(ResultSetProcessorGroupedOutputFirstHelper),
                        "helper",
                        Ref(NAME_OUTPUTFIRSTHELPERS))
                    .ExprDotMethod(Ref("helper"), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "Destroy");
            }
        }

        private static CodegenMethod GenerateGroupKeysRowCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions,
                classScope,
                instance);

            Consumer<CodegenMethod> code = methodNode => methodNode.Block.DeclareVar<object>(
                    "groupKeyComplete",
                    LocalMethod(
                        generateGroupKeySingle,
                        Ref("eventsPerStream"),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA))
                .DeclareVar<AggregationGroupByRollupLevel[]>(
                    "levels",
                    ExprDotMethodChain(Ref("this")).Get("GroupByRollupDesc").Get("Levels"))
                .DeclareVar<object[]>("result", NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))))
                .ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                .DeclareVar<object>(
                    "subkey",
                    ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "ComputeSubkey", Ref("groupKeyComplete")))
                .AssignArrayElement("result", Ref("j"), Ref("subkey"))
                .BlockEnd()
                .MethodReturn(Ref("result"));

            return instance.Methods.AddMethod(
                typeof(object[]),
                "GenerateGroupKeysRow",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "eventsPerStream",
                    typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA),
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
                        ResultSetProcessorRowPerGroupRollupUtil.METHOD_MAKEGROUPREPSPERLEVELBUF,
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
                        ResultSetProcessorRowPerGroupRollupUtil.METHOD_MAKERSTREAMSORTEDARRAYBUF,
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
                        REF_AGENTINSTANCECONTEXT,
                        Constant(forge.GroupKeyTypes),
                        ExprDotName(Ref("this"), "GroupByRollupDesc"),
                        outputConditionFactory));
            }
        }
    }
} // end of namespace