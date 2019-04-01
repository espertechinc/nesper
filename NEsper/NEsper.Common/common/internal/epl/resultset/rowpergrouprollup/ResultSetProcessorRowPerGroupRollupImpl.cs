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
                method.Block.ExprDotMethod(Ref("this"), "clear");
            }

            method.Block.LocalMethod(resetEventPerGroupJoinBuf)
                .DeclareVar(
                    typeof(object[][]), "newDataMultiKey",
                    LocalMethod(generateGroupKeysJoin, REF_NEWDATA, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue()))
                .DeclareVar(
                    typeof(object[][]), "oldDataMultiKey",
                    LocalMethod(generateGroupKeysJoin, REF_OLDDATA, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantFalse()))
                .DeclareVar(
                    typeof(EventBean[]), "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsJoin, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantFalse(), REF_ISSYNTHESIZE)
                        : ConstantNull())
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil), METHOD_APPLYAGGJOINRESULTKEYEDJOIN, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, REF_NEWDATA, Ref("newDataMultiKey"), REF_OLDDATA, Ref("oldDataMultiKey"))
                .DeclareVar(
                    typeof(EventBean[]), "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsJoin, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue(), REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("selectNewEvents"),
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

            method.Block.LocalMethod(resetEventPerGroupBufView)
                .DeclareVar(
                    typeof(object[][]), "newDataMultiKey",
                    LocalMethod(generateGroupKeysView, REF_NEWDATA, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue()))
                .DeclareVar(
                    typeof(object[][]), "oldDataMultiKey",
                    LocalMethod(generateGroupKeysView, REF_OLDDATA, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantFalse()))
                .DeclareVar(
                    typeof(EventBean[]), "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantFalse(), REF_ISSYNTHESIZE)
                        : ConstantNull())
                .DeclareVar(typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil), METHOD_APPLYAGGVIEWRESULTKEYEDVIEW, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, REF_NEWDATA, Ref("newDataMultiKey"), REF_OLDDATA, Ref("oldDataMultiKey"),
                    Ref("eventsPerStream"))
                .DeclareVar(
                    typeof(EventBean[]), "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsView, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue(), REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("selectNewEvents"),
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
                    typeof(LinkedHashMap<object, object>[]), typeof(ResultSetProcessorRowPerGroupRollupImpl),
                    classScope);
                instance.AddMember(memberName, typeof(LinkedHashMap<object, object>[]));
                instance.ServiceCtor.Block.AssignRef(memberName, LocalMethod(init));
                var levelCount = forge.GroupByRollupDesc.Levels.Length;
                init.Block.DeclareVar(
                        typeof(LinkedHashMap<object, object>[]), memberName,
                        NewArrayByLength(typeof(LinkedHashMap<object, object>), Constant(levelCount)))
                    .ForLoopIntSimple("i", Constant(levelCount))
                    .AssignArrayElement(memberName, Ref("i"), NewInstance(typeof(LinkedHashMap<object, object>)))
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
                methodNode.Block.DeclareVar(
                        typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar(typeof(List<>), "events", NewInstance(typeof(List<object>), Constant(1)))
                    .DeclareVar(
                        typeof(IList<object>), "currentGenerators",
                        forge.IsSorting ? NewInstance(typeof(List<object>), Constant(1)) : ConstantNull())
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]), "levels",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"));

                {
                    var forLevels = methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel), "level", Ref("levels"));
                    {
                        var forEvents = forLevels.ForEach(
                            typeof(KeyValuePair<object, object>), "entry",
                            ExprDotMethod(
                                ArrayAtIndex(Ref("keysAndEvents"), ExprDotMethod(Ref("level"), "getLevelNumber")),
                                "entrySet"));
                        forEvents.DeclareVar(typeof(object), "groupKey", ExprDotMethod(Ref("entry"), "getKey"))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC, "setCurrentAccess", Ref("groupKey"),
                                ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"))
                            .AssignArrayElement(
                                Ref("eventsPerStream"), Constant(0),
                                Cast(typeof(EventBean), ExprDotMethod(Ref("entry"), "getValue")));

                        if (forge.PerLevelForges.OptionalHavingForges != null) {
                            var having = ArrayAtIndex(
                                REF_HAVINGEVALUATOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber"));
                            forEvents.IfCondition(
                                    Not(
                                        ExprDotMethod(
                                            having, "evaluateHaving", REF_EPS,
                                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                            REF_AGENTINSTANCECONTEXT)))
                                .BlockContinue();
                        }

                        forEvents.ExprDotMethod(
                            Ref("events"), "add",
                            ExprDotMethod(
                                ArrayAtIndex(
                                    REF_SELECTEXPRPROCESSOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber")),
                                "process", Ref("eventsPerStream"), ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT));

                        if (forge.IsSorting) {
                            forEvents.DeclareVar(
                                    typeof(EventBean[]), "currentEventsPerStream",
                                    NewArrayWithInit(
                                        typeof(EventBean),
                                        Cast(typeof(EventBean), ExprDotMethod(Ref("entry"), "getValue"))))
                                .ExprDotMethod(
                                    Ref("currentGenerators"), "add",
                                    NewInstance(
                                        typeof(GroupByRollupKey), Ref("currentEventsPerStream"), Ref("level"),
                                        Ref("groupKey")));
                        }
                    }
                }

                methodNode.Block.IfCondition(ExprDotMethod(Ref("events"), "isEmpty")).BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(EventBean[]), "outgoing",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")));
                if (forge.IsSorting) {
                    methodNode.Block.IfCondition(Relational(ArrayLength(Ref("outgoing")), GT, Constant(1)))
                        .BlockReturn(
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR, "sortRollup", Ref("outgoing"), Ref("currentGenerators"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA, REF_AGENTINSTANCECONTEXT,
                                REF_AGGREGATIONSVC));
                }

                methodNode.Block.MethodReturn(Ref("outgoing"));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]), "generateOutputEventsView",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "keysAndEvents", typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA, typeof(bool),
                    NAME_ISSYNTHESIZE), typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        private static CodegenMethod GenerateOutputEventsJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(
                        typeof(List<object>), "events", NewInstance(typeof(List<object>), Constant(1)))
                    .DeclareVar(
                        typeof(IList<object>), "currentGenerators",
                        forge.IsSorting ? NewInstance(typeof(List<object>), Constant(1)) : ConstantNull())
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]), "levels",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"));

                {
                    var forLevels = methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel), "level", Ref("levels"));
                    {
                        var forEvents = forLevels.ForEach(
                            typeof(KeyValuePair<object, object>), "entry",
                            ExprDotMethod(
                                ArrayAtIndex(Ref("eventPairs"), ExprDotMethod(Ref("level"), "getLevelNumber")),
                                "entrySet"));
                        forEvents.DeclareVar(typeof(object), "groupKey", ExprDotMethod(Ref("entry"), "getKey"))
                            .ExprDotMethod(
                                REF_AGGREGATIONSVC, "setCurrentAccess", Ref("groupKey"),
                                ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"))
                            .DeclareVar(
                                typeof(EventBean[]), "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")));

                        if (forge.PerLevelForges.OptionalHavingForges != null) {
                            var having = ArrayAtIndex(
                                REF_HAVINGEVALUATOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber"));
                            forEvents.IfCondition(
                                    Not(
                                        ExprDotMethod(
                                            having, "evaluateHaving", REF_EPS,
                                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                            REF_AGENTINSTANCECONTEXT)))
                                .BlockContinue();
                        }

                        forEvents.ExprDotMethod(
                            Ref("events"), "add",
                            ExprDotMethod(
                                ArrayAtIndex(
                                    REF_SELECTEXPRPROCESSOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber")),
                                "process", Ref("eventsPerStream"), ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                REF_ISSYNTHESIZE,
                                REF_AGENTINSTANCECONTEXT));

                        if (forge.IsSorting) {
                            forEvents.ExprDotMethod(
                                Ref("currentGenerators"), "add",
                                NewInstance(
                                    typeof(GroupByRollupKey), Ref("eventsPerStream"), Ref("level"), Ref("groupKey")));
                        }
                    }
                }

                methodNode.Block.IfCondition(ExprDotMethod(Ref("events"), "isEmpty")).BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(EventBean[]), "outgoing",
                        StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")));
                if (forge.IsSorting) {
                    methodNode.Block.IfCondition(Relational(ArrayLength(Ref("outgoing")), GT, Constant(1)))
                        .BlockReturn(
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR, "sortRollup", Ref("outgoing"), Ref("currentGenerators"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA, REF_AGENTINSTANCECONTEXT,
                                REF_AGGREGATIONSVC));
                }

                methodNode.Block.MethodReturn(Ref("outgoing"));
            };

            return instance.Methods.AddMethod(
                typeof(EventBean[]), "generateOutputEventsJoin",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "eventPairs", typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
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
                forge.GroupKeyNodeExpressions, classScope, instance);

            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT)
                .DeclareVar(typeof(IEnumerator<EventBean>), "it", ExprDotMethod(REF_VIEWABLE, "iterator"))
                .DeclareVar(typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar(
                    typeof(object[]), "groupKeys",
                    NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                .DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]), "levels",
                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"));
            {
                method.Block.WhileLoop(ExprDotMethod(Ref("it"), "hasNext"))
                    .AssignArrayElement(
                        Ref("eventsPerStream"), Constant(0), Cast(typeof(EventBean), ExprDotMethod(Ref("it"), "next")))
                    .DeclareVar(
                        typeof(object), "groupKeyComplete",
                        LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()))
                    .ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                    .DeclareVar(
                        typeof(object), "subkey",
                        ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "computeSubkey", Ref("groupKeyComplete")))
                    .AssignArrayElement("groupKeys", Ref("j"), Ref("subkey"))
                    .BlockEnd()
                    .ExprDotMethod(
                        REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeys"),
                        REF_AGENTINSTANCECONTEXT)
                    .BlockEnd();
            }

            method.Block.DeclareVar(
                    typeof(ArrayDeque<EventBean>), "deque",
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_ITERATORTODEQUE,
                        LocalMethod(ObtainIteratorCodegen(forge, classScope, method, instance), REF_VIEWABLE)))
                .ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "iterator"));
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
            iterator.Block.LocalMethod(resetEventPerGroupBufView)
                .DeclareVar(
                    typeof(EventBean[]), "events",
                    StaticMethod(
                        typeof(CollectionUtil), METHOD_ITERATORTOARRAYEVENTS, ExprDotMethod(REF_VIEWABLE, "iterator")))
                .LocalMethod(generateGroupKeysView, Ref("events"), Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue())
                .DeclareVar(
                    typeof(EventBean[]), "output",
                    LocalMethod(
                        generateOutputEventsView, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue(), ConstantTrue()))
                .MethodReturn(StaticMethod(typeof(EnumerationHelper), "Singleton", Ref("output")));
            return iterator;
        }

        protected internal static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "processView", classScope, method, instance);
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
            method.Block.LocalMethod(resetEventPerGroupBuf)
                .LocalMethod(generateGroupKeysJoin, REF_JOINSET, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue())
                .DeclareVar(
                    typeof(EventBean[]), "output",
                    LocalMethod(
                        generateOutputEventsJoin, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue(), ConstantTrue()))
                .MethodReturn(StaticMethod(typeof(EnumerationHelper), "Singleton", Ref("output")));
        }

        protected internal static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT);
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
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTALLHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                method.Block.ForEach(
                        typeof(ResultSetProcessorGroupedOutputFirstHelper), "helper", Ref(NAME_OUTPUTFIRSTHELPERS))
                    .ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref("helper"));
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

            method.Block.ForEach(typeof(IDictionary<object, object>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "clear");

            method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");

            method.Block.DeclareVarNoInit(typeof(int), "count");
            if (forge.PerLevelForges.OptionalHavingForges == null) {
                var handleOutputLimitFirstViewNoHaving =
                    HandleOutputLimitFirstViewNoHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstViewNoHaving, REF_VIEWEVENTSLIST, REF_ISSYNTHESIZE,
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel")));
            }
            else {
                var handleOutputLimitFirstViewHaving =
                    HandleOutputLimitFirstViewHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstViewHaving, REF_VIEWEVENTSLIST, REF_ISSYNTHESIZE,
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel")));
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

            method.Block.ForEach(typeof(IDictionary<object, object>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "clear");

            method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");

            method.Block.DeclareVarNoInit(typeof(int), "count");
            if (forge.PerLevelForges.OptionalHavingForges == null) {
                var handleOutputLimitFirstJoinNoHaving =
                    HandleOutputLimitFirstJoinNoHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstJoinNoHaving, REF_JOINEVENTSSET, REF_ISSYNTHESIZE,
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel")));
            }
            else {
                var handleOutputLimitFirstJoinHaving =
                    HandleOutputLimitFirstJoinHavingCodegen(forge, classScope, instance);
                method.Block.AssignRef(
                    "count",
                    LocalMethod(
                        handleOutputLimitFirstJoinHaving, REF_JOINEVENTSSET, REF_ISSYNTHESIZE,
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel")));
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
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);
            var outputFactory = classScope.AddFieldUnshared(
                true, typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.PackageScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(typeof(int), "count", Constant(0));
                {
                    var forEach = methodNode.Block.ForEach(
                        typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                    forEach.DeclareVar(
                            typeof(EventBean[]), "newData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar(
                            typeof(EventBean[]), "oldData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar(
                            typeof(object[]), "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                        .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifNewFirst = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNewFirst = ifNewFirst.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forNewFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC, "setCurrentAccess", Ref("groupKey"),
                                        ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotMethod(Ref("level"), "getLevelNumber")), "evaluateHaving",
                                                Ref("eventsPerStream"), ConstantTrue(), REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(1),
                                            Constant(0)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }

                        var ifOldFirst = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOldFirst = ifOldFirst.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forOldFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC, "setCurrentAccess", Ref("groupKey"),
                                        ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotMethod(Ref("level"), "getLevelNumber")), "evaluateHaving",
                                                Ref("eventsPerStream"), ConstantFalse(), REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(0),
                                            Constant(1)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int), "handleOutputLimitFirstViewHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>), "viewEventsList", typeof(bool), NAME_ISSYNTHESIZE, typeof(IList<object>[]),
                    "oldEventsPerLevel", typeof(IList<object>[]), "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil), classScope, code);
        }

        private static CodegenMethod HandleOutputLimitFirstJoinNoHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);

            var outputFactory = classScope.AddFieldUnshared(
                true, typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.PackageScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(typeof(int), "count", Constant(0))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar(
                            typeof(ISet<object>), "newData",
                            Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar(
                            typeof(ISet<object>), "oldData",
                            Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar(
                            typeof(object[]), "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)));

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(MultiKey<object>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var forLvl = forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(1),
                                            Constant(0)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(MultiKey<object>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                var forLvl = forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(0),
                                            Constant(1)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int), "handleOutputLimitFirstJoinNoHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>), NAME_JOINEVENTSSET, typeof(bool), NAME_ISSYNTHESIZE, typeof(IList<object>[]),
                    "oldEventsPerLevel", typeof(IList<object>[]), "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil), classScope, code);
        }

        private static CodegenMethod HandleOutputLimitFirstJoinHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);

            var outputFactory = classScope.AddFieldUnshared(
                true, typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.PackageScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(typeof(int), "count", Constant(0))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var forEach = methodNode.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                    forEach.DeclareVar(
                            typeof(ISet<EventBean>), "newData",
                            Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar(
                            typeof(ISet<EventBean>), "oldData",
                            Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar(
                            typeof(object[]), "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)));
                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifNewFirst = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNewFirst = ifNewFirst.ForEach(
                                    typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forNewFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC, "setCurrentAccess", Ref("groupKey"),
                                        ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotMethod(Ref("level"), "getLevelNumber")), "evaluateHaving",
                                                Ref("eventsPerStream"), ConstantTrue(), REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(1),
                                            Constant(0)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }

                        var ifOldFirst = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOldFirst = ifOldFirst.ForEach(
                                    typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                                .AssignRef(
                                    "eventsPerStream",
                                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var eachlvl = forOldFirst.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .ExprDotMethod(
                                        REF_AGGREGATIONSVC, "setCurrentAccess", Ref("groupKey"),
                                        ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"))
                                    .IfCondition(
                                        Not(
                                            ExprDotMethod(
                                                ArrayAtIndex(
                                                    REF_HAVINGEVALUATOR_ARRAY,
                                                    ExprDotMethod(Ref("level"), "getLevelNumber")), "evaluateHaving",
                                                Ref("eventsPerStream"), ConstantFalse(), REF_AGENTINSTANCECONTEXT)))
                                    .BlockContinue()
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(0),
                                            Constant(1)));
                                var passBlock = eachlvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int), "handleOutputLimitFirstJoinHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>), NAME_JOINEVENTSSET, typeof(bool), NAME_ISSYNTHESIZE, typeof(IList<object>[]),
                    "oldEventsPerLevel", typeof(IList<object>[]), "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil), classScope, code);
        }

        private static CodegenMethod HandleOutputLimitFirstViewNoHavingCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            InitRStreamEventsSortArrayBufCodegen(instance, forge);

            var outputFactory = classScope.AddFieldUnshared(
                true, typeof(OutputConditionPolledFactory),
                forge.OptionalOutputFirstConditionFactory.Make(classScope.PackageScope.InitMethod, classScope));
            InitOutputFirstHelpers(outputFactory, instance, forge, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(typeof(int), "count", Constant(0));

                {
                    var forEach = methodNode.Block.ForEach(
                        typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                    forEach.DeclareVar(
                            typeof(EventBean[]), "newData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                        .DeclareVar(
                            typeof(EventBean[]), "oldData",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                        .DeclareVar(
                            typeof(object[]), "groupKeysPerLevel",
                            NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                        .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                    {
                        var ifNewApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                        {
                            var forNew = ifNewApplyAgg.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                            {
                                var forLvl = forNew.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(1),
                                            Constant(0)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forNew.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }

                        var ifOldApplyAgg = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                        {
                            var forOld = ifOldApplyAgg.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                                .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                                .DeclareVar(
                                    typeof(object), "groupKeyComplete",
                                    LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                            {
                                var forLvl = forOld.ForEach(
                                        typeof(AggregationGroupByRollupLevel), "level",
                                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                    .DeclareVar(
                                        typeof(object), "groupKey",
                                        ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                    .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"))
                                    .DeclareVar(
                                        typeof(OutputConditionPolled), "outputStateGroup",
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_OUTPUTFIRSTHELPERS), levelNumber), "getOrAllocate",
                                            Ref("groupKey"), REF_AGENTINSTANCECONTEXT, outputFactory))
                                    .DeclareVar(
                                        typeof(bool), "pass",
                                        ExprDotMethod(
                                            Ref("outputStateGroup"), "updateOutputCondition", Constant(0),
                                            Constant(1)));
                                var passBlock = forLvl.IfCondition(Ref("pass"));
                                var putBlock = passBlock.IfCondition(
                                    EqualsNull(
                                        ExprDotMethod(
                                            ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                            Ref("groupKey"), Ref("eventsPerStream"))));
                                if (forge.IsSelectRStream) {
                                    putBlock.LocalMethod(
                                            generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"),
                                            Ref("level"), Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                            Ref("oldEventsPerLevel"), Ref("oldEventsSortKeyPerLevel"))
                                        .Increment("count");
                                }
                            }
                            forOld.ExprDotMethod(
                                REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                                REF_AGENTINSTANCECONTEXT);
                        }
                    }
                }

                methodNode.Block.MethodReturn(Ref("count"));
            };

            return instance.Methods.AddMethod(
                typeof(int), "handleOutputLimitFirstNoViewHaving",
                CodegenNamedParam.From(
                    typeof(IList<object>), "viewEventsList", typeof(bool), NAME_ISSYNTHESIZE, typeof(IList<object>[]),
                    "oldEventsPerLevel", typeof(IList<object>[]), "oldEventsSortKeyPerLevel"),
                typeof(ResultSetProcessorUtil), classScope, code);
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
                typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar(
                        typeof(EventBean[]), "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar(
                        typeof(EventBean[]), "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .LocalMethod(resetEventPerGroupBufView)
                    .DeclareVar(
                        typeof(object[]), "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysView, Ref("newData"), Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue()))
                    .DeclareVar(
                        typeof(object[]), "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysView, Ref("oldData"), Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(
                        generateOutputBatchedCollectView, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantFalse(),
                        REF_ISSYNTHESIZE, Ref("oldEvents"), Ref("oldEventsSortKey"), Ref("eventsPerStream"));
                }

                forEach.StaticMethod(
                        typeof(ResultSetProcessorGroupedUtil), METHOD_APPLYAGGVIEWRESULTKEYEDVIEW, REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT, Ref("newData"), Ref("newDataMultiKey"), Ref("oldData"),
                        Ref("oldDataMultiKey"), Ref("eventsPerStream"))
                    .LocalMethod(
                        generateOutputBatchedCollectView, Ref(NAME_EVENTPERGROUPBUFVIEW), ConstantTrue(),
                        REF_ISSYNTHESIZE, Ref("newEvents"), Ref("newEventsSortKey"), Ref("eventsPerStream"));
            }

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block, Ref("newEvents"), Ref("newEventsSortKey"), Ref("oldEvents"), Ref("oldEventsSortKey"),
                forge.IsSelectRStream, forge.IsSorting);
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
                    .DeclareVar(
                        typeof(ISet<object>), "newData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar(
                        typeof(ISet<object>), "oldData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                    .LocalMethod(resetEventPerGroupBufJoin)
                    .DeclareVar(
                        typeof(object[]), "newDataMultiKey",
                        LocalMethod(
                            generateGroupKeysJoin, Ref("newData"), Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue()))
                    .DeclareVar(
                        typeof(object[]), "oldDataMultiKey",
                        LocalMethod(
                            generateGroupKeysJoin, Ref("oldData"), Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantFalse()));

                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(
                        generateOutputBatchedCollectJoin, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantFalse(),
                        REF_ISSYNTHESIZE, Ref("oldEvents"), Ref("oldEventsSortKey"));
                }

                forEach.StaticMethod(
                        typeof(ResultSetProcessorGroupedUtil), METHOD_APPLYAGGJOINRESULTKEYEDJOIN, REF_AGGREGATIONSVC,
                        REF_AGENTINSTANCECONTEXT, Ref("newData"), Ref("newDataMultiKey"), Ref("oldData"),
                        Ref("oldDataMultiKey"))
                    .LocalMethod(
                        generateOutputBatchedCollectJoin, Ref(NAME_EVENTPERGROUPBUFJOIN), ConstantTrue(),
                        REF_ISSYNTHESIZE, Ref("newEvents"), Ref("newEventsSortKey"));
            }

            FinalizeOutputMaySortMayRStreamCodegen(
                method.Block, Ref("newEvents"), Ref("newEventsSortKey"), Ref("oldEvents"), Ref("oldEventsSortKey"),
                forge.IsSelectRStream, forge.IsSorting);
        }

        protected internal static void RemovedAggregationGroupKeyCodegen(
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => method.Block.MethodThrowUnsupported();
            instance.Methods.AddMethod(
                typeof(void), "removedAggregationGroupKey", CodegenNamedParam.From(typeof(object), "key"),
                typeof(ResultSetProcessorRowPerGroupImpl), classScope, code);
        }

        private static CodegenMethod GenerateOutputBatchedGivenArrayCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatched = GenerateOutputBatchedCodegen(forge, instance, classScope);
            Consumer<CodegenMethod> code = methodNode => methodNode.Block.DeclareVar(
                    typeof(IList<object>), "resultList",
                    ArrayAtIndex(Ref("resultEvents"), ExprDotMethod(Ref("level"), "getLevelNumber")))
                .DeclareVarNoInit(typeof(IList<object>), "sortKeys")
                .IfCondition(EqualsNull(Ref("optSortKeys")))
                .AssignRef("sortKeys", ConstantNull())
                .IfElse()
                .AssignRef("sortKeys", ArrayAtIndex(Ref("optSortKeys"), ExprDotMethod(Ref("level"), "getLevelNumber")))
                .BlockEnd()
                .LocalMethod(
                    generateOutputBatched, Ref("mk"), Ref("level"), Ref("eventsPerStream"),
                    ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                    REF_ISSYNTHESIZE, Ref("resultList"), Ref("sortKeys"));
            return instance.Methods.AddMethod(
                typeof(void), "generateOutputBatchedGivenArrayCodegen",
                CodegenNamedParam.From(
                    typeof(bool), "join", typeof(object), "mk", typeof(AggregationGroupByRollupLevel), "level",
                    typeof(EventBean[]), "eventsPerStream", typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA, typeof(bool),
                    NAME_ISSYNTHESIZE, typeof(IList<object>[]), "resultEvents", typeof(IList<object>[]), "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        protected internal static CodegenMethod GenerateOutputBatchedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC, "setCurrentAccess", Ref("mk"),
                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"));

                if (forge.PerLevelForges.OptionalHavingForges != null) {
                    methodNode.Block.IfCondition(
                            Not(
                                ExprDotMethod(
                                    ArrayAtIndex(
                                        REF_HAVINGEVALUATOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber")),
                                    "evaluateHaving", Ref("eventsPerStream"),
                                    ResultSetProcessorCodegenNames.REF_ISNEWDATA, REF_AGENTINSTANCECONTEXT)))
                        .BlockReturnNoValue();
                }

                var selectExprProcessor = ArrayAtIndex(
                    REF_SELECTEXPRPROCESSOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber"));
                methodNode.Block.ExprDotMethod(
                    Ref("resultEvents"), "add",
                    ExprDotMethod(
                        selectExprProcessor, "process", Ref("eventsPerStream"),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA, REF_ISSYNTHESIZE,
                        REF_AGENTINSTANCECONTEXT));

                if (forge.IsSorting) {
                    methodNode.Block.ExprDotMethod(
                        Ref("optSortKeys"), "add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR, "getSortKeyRollup", Ref("eventsPerStream"),
                            ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                            REF_AGENTINSTANCECONTEXT, Ref("level")));
                }
            };
            return instance.Methods.AddMethod(
                typeof(void), "generateOutputBatched",
                CodegenNamedParam.From(
                    typeof(object), "mk", typeof(AggregationGroupByRollupLevel), "level", typeof(EventBean[]),
                    "eventsPerStream", typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<object>),
                    "resultEvents", typeof(IList<object>), "optSortKeys"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        protected internal static void GenerateOutputBatchedMapUnsortedCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenInstanceAux instance,
            CodegenClassScope classScope)
        {
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.ExprDotMethod(
                    REF_AGGREGATIONSVC, "setCurrentAccess", Ref("mk"),
                    ExprDotMethod(REF_AGENTINSTANCECONTEXT, "getAgentInstanceId"), Ref("level"));

                if (forge.PerLevelForges.OptionalHavingForges != null) {
                    var having = ArrayAtIndex(REF_HAVINGEVALUATOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber"));
                    methodNode.Block.IfCondition(
                            Not(
                                ExprDotMethod(
                                    having, "evaluateHaving", REF_EPS, ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturnNoValue();
                }

                var selectExprProcessor = ArrayAtIndex(
                    REF_SELECTEXPRPROCESSOR_ARRAY, ExprDotMethod(Ref("level"), "getLevelNumber"));
                methodNode.Block.ExprDotMethod(
                    Ref("resultEvents"), "put", Ref("mk"),
                    ExprDotMethod(
                        selectExprProcessor, "process", Ref("eventsPerStream"),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA, REF_ISSYNTHESIZE,
                        REF_AGENTINSTANCECONTEXT));
            };

            instance.Methods.AddMethod(
                typeof(void), "generateOutputBatchedMapUnsorted",
                CodegenNamedParam.From(
                    typeof(bool), "join", typeof(object), "mk", typeof(AggregationGroupByRollupLevel), "level",
                    typeof(EventBean[]), "eventsPerStream", typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE, typeof(IDictionary<object, object>), "resultEvents"),
                typeof(ResultSetProcessorUtil), classScope, code);
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
                forge.GroupKeyNodeExpressions, classScope, instance);
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");

            method.Block.DeclareVar(typeof(int), "count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
            }

            method.Block.ForEach(typeof(IDictionary<object, object>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "clear");

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar(
                        typeof(EventBean[]), "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar(
                        typeof(EventBean[]), "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar(
                        typeof(object[]), "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
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
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            var generateOutputBatchedGivenArray = GenerateOutputBatchedGivenArrayCodegen(forge, classScope, instance);
            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar(typeof(int), "count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
            }

            method.Block.ForEach(typeof(IDictionary<object, object>), "aGroupRepsView", Ref(NAME_GROUPREPSPERLEVELBUF))
                .ExprDotMethod(Ref("aGroupRepsView"), "clear");

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar(
                        typeof(ISet<object>), "newData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar(
                        typeof(ISet<object>), "oldData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar(
                        typeof(object[]), "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
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
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar(typeof(int), "count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
                method.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel), "level",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                    .DeclareVar(
                        typeof(IDictionary<object, object>), "groupGenerators",
                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), ExprDotMethod(Ref("level"), "getLevelNumber")))
                    .ForEach(
                        typeof(KeyValuePair<object, object>), "entry",
                        ExprDotMethod(Ref("groupGenerators"), "entrySet"))
                    .LocalMethod(
                        generateOutputBatchedGivenArray, ConstantFalse(), ExprDotMethod(Ref("entry"), "getKey"),
                        Ref("level"), Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")),
                        ConstantFalse(), REF_ISSYNTHESIZE,
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                    .Increment("count");
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar(
                        typeof(EventBean[]), "newData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar(
                        typeof(EventBean[]), "oldData",
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar(
                        typeof(object[]), "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(EventBean), "aNewData", Ref("newData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("aNewData")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(EventBean), "anOldData", Ref("oldData"))
                            .AssignRef("eventsPerStream", NewArrayWithInit(typeof(EventBean), Ref("anOldData")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
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
                forge.GroupKeyNodeExpressions, classScope, instance);
            var levelNumber = ExprDotMethod(Ref("level"), "getLevelNumber");
            InitGroupRepsPerLevelBufCodegen(instance, forge);
            if (forge.IsSelectRStream) {
                InitRStreamEventsSortArrayBufCodegen(instance, forge);
            }

            var generateAndSort = GenerateAndSortCodegen(forge, classScope, instance);

            method.Block.DeclareVar(typeof(int), "count", Constant(0));
            if (forge.IsSelectRStream) {
                method.Block.ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "reset");
                method.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel), "level",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                    .DeclareVar(
                        typeof(IDictionary<object, object>), "groupGenerators",
                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), ExprDotMethod(Ref("level"), "getLevelNumber")))
                    .ForEach(
                        typeof(KeyValuePair<object, object>), "entry",
                        ExprDotMethod(Ref("groupGenerators"), "entrySet"))
                    .LocalMethod(
                        generateOutputBatchedGivenArray, ConstantFalse(), ExprDotMethod(Ref("entry"), "getKey"),
                        Ref("level"), Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")),
                        ConstantFalse(), REF_ISSYNTHESIZE,
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                    .Increment("count");
            }

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_JOINEVENTSSET);
                forEach.DeclareVar(
                        typeof(ISet<object>), "newData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getFirst")))
                    .DeclareVar(
                        typeof(ISet<object>), "oldData",
                        Cast(typeof(ISet<object>), ExprDotMethod(Ref("pair"), "getSecond")))
                    .DeclareVar(
                        typeof(object[]), "groupKeysPerLevel",
                        NewArrayByLength(typeof(object), Constant(forge.GroupByRollupDesc.Levels.Length)))
                    .DeclareVarNoInit(typeof(EventBean[]), "eventsPerStream");

                {
                    var ifNew = forEach.IfCondition(NotEqualsNull(Ref("newData")));
                    {
                        var forNew = ifNew.ForEach(typeof(MultiKey<EventBean>), "aNewData", Ref("newData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("aNewData"), "getArray")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantTrue()));
                        {
                            var forLevel = forNew.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantTrue(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forNew.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
                            REF_AGENTINSTANCECONTEXT);
                    }

                    var ifOld = forEach.IfCondition(NotEqualsNull(Ref("oldData")));
                    {
                        var forOld = ifOld.ForEach(typeof(MultiKey<EventBean>), "anOldData", Ref("oldData"))
                            .AssignRef(
                                "eventsPerStream",
                                Cast(typeof(EventBean[]), ExprDotMethod(Ref("anOldData"), "getArray")))
                            .DeclareVar(
                                typeof(object), "groupKeyComplete",
                                LocalMethod(generateGroupKeySingle, Ref("eventsPerStream"), ConstantFalse()));
                        {
                            var forLevel = forOld.ForEach(
                                    typeof(AggregationGroupByRollupLevel), "level",
                                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                                .DeclareVar(
                                    typeof(object), "groupKey",
                                    ExprDotMethod(Ref("level"), "computeSubkey", Ref("groupKeyComplete")))
                                .AssignArrayElement(Ref("groupKeysPerLevel"), levelNumber, Ref("groupKey"));
                            var ifNullPut = forLevel.IfCondition(
                                EqualsNull(
                                    ExprDotMethod(
                                        ArrayAtIndex(Ref(NAME_GROUPREPSPERLEVELBUF), levelNumber), "put",
                                        Ref("groupKey"), Ref("eventsPerStream"))));
                            if (forge.IsSelectRStream) {
                                ifNullPut.LocalMethod(
                                        generateOutputBatchedGivenArray, ConstantFalse(), Ref("groupKey"), Ref("level"),
                                        Ref("eventsPerStream"), ConstantFalse(), REF_ISSYNTHESIZE,
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getEventsPerLevel"),
                                        ExprDotMethod(Ref(NAME_RSTREAMEVENTSORTARRAYBUF), "getSortKeyPerLevel"))
                                    .Increment("count");
                            }
                        }
                        forOld.ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("groupKeysPerLevel"),
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
                methodNode.Block.DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]), "levels",
                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"));

                var forLevels = methodNode.Block.ForEach(typeof(AggregationGroupByRollupLevel), "level", Ref("levels"));
                {
                    var forEvents = forLevels.ForEach(
                        typeof(KeyValuePair<object, object>), "pair",
                        ExprDotMethod(
                            ArrayAtIndex(
                                Ref("eventPairs"), ExprDotMethod(Ref("level"), "getLevelNumber")), "entrySet"));
                    forEvents.AssignArrayElement(
                            "eventsPerStream", Constant(0),
                            Cast(typeof(EventBean), ExprDotMethod(Ref("pair"), "getValue")))
                        .LocalMethod(
                            generateOutputBatched, ExprDotMethod(Ref("pair"), "getKey"), Ref("level"),
                            Ref("eventsPerStream"), ResultSetProcessorCodegenNames.REF_ISNEWDATA, REF_ISSYNTHESIZE,
                            Ref("events"), Ref("sortKey"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void), "generateOutputBatchedCollectView",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "eventPairs", typeof(bool), ResultSetProcessorCodegenNames.NAME_ISNEWDATA, typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<object>), "events", typeof(IList<object>), "sortKey", typeof(EventBean[]),
                    "eventsPerStream"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        private static CodegenMethod GenerateOutputBatchedCollectJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateOutputBatched = GenerateOutputBatchedCodegen(forge, instance, classScope);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]), "levels",
                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"));

                var forLevels = methodNode.Block.ForEach(typeof(AggregationGroupByRollupLevel), "level", Ref("levels"));
                {
                    var forEvents = forLevels.ForEach(
                        typeof(KeyValuePair<object,object>), "pair",
                        ExprDotMethod(
                            ArrayAtIndex(
                                Ref("eventPairs"), ExprDotMethod(Ref("level"), "getLevelNumber")), "entrySet"));
                    forEvents.LocalMethod(
                        generateOutputBatched, ExprDotMethod(Ref("pair"), "getKey"), Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getValue")),
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                        REF_ISSYNTHESIZE, Ref("events"), Ref("sortKey"));
                }
            };

            return instance.Methods.AddMethod(
                typeof(void), "generateOutputBatchedCollectJoin",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "eventPairs", typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE,
                    typeof(IList<object>), "events", typeof(IList<object>), "sortKey"),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        private static CodegenMethod ResetEventPerGroupBufCodegen(
            string memberName,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => methodNode.Block
                .ForEach(typeof(LinkedHashMap<object, object>), "anEventPerGroupBuf", Ref(memberName))
                .ExprDotMethod(Ref("anEventPerGroupBuf"), "clear");

            return instance.Methods.AddMethod(
                typeof(void), "resetEventPerGroupBuf", Collections.GetEmptyList<CodegenNamedParam>(),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        protected internal static CodegenMethod GenerateGroupKeysViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfRefNullReturnNull("events")
                    .DeclareVar(
                        typeof(object[][]), "result", NewArrayByLength(typeof(object[]), ArrayLength(Ref("events"))))
                    .DeclareVar(
                        typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]), "levels",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"));
                {
                    var forLoop = methodNode.Block.ForLoopIntSimple("i", ArrayLength(Ref("events")));
                    forLoop.AssignArrayElement("eventsPerStream", Constant(0), ArrayAtIndex(Ref("events"), Ref("i")))
                        .DeclareVar(
                            typeof(object), "groupKeyComplete",
                            LocalMethod(
                                generateGroupKeySingle, Ref("eventsPerStream"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA))
                        .AssignArrayElement(
                            "result", Ref("i"), NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))));
                    {
                        forLoop.ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                            .DeclareVar(
                                typeof(object), "subkey",
                                ExprDotMethod(
                                    ArrayAtIndex(Ref("levels"), Ref("j")), "computeSubkey", Ref("groupKeyComplete")))
                            .AssignArrayElement2Dim("result", Ref("i"), Ref("j"), Ref("subkey"))
                            .ExprDotMethod(
                                ArrayAtIndex(
                                    Ref("eventPerKey"),
                                    ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "getLevelNumber")), "put",
                                Ref("subkey"), ArrayAtIndex(Ref("events"), Ref("i")));
                    }
                }

                methodNode.Block.MethodReturn(Ref("result"));
            };

            return instance.Methods.AddMethod(
                typeof(object[][]), "generateGroupKeysView",
                CodegenNamedParam.From(
                    typeof(EventBean[]), "events", typeof(IDictionary<object, object>[]), "eventPerKey", typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
        }

        private static CodegenMethod GenerateGroupKeysJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);
            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.IfCondition(Or(EqualsNull(Ref("events")), ExprDotMethod(Ref("events"), "isEmpty")))
                    .BlockReturn(ConstantNull())
                    .DeclareVar(
                        typeof(object[][]), "result",
                        NewArrayByLength(typeof(object[]), ExprDotMethod(Ref("events"), "size")))
                    .DeclareVar(
                        typeof(AggregationGroupByRollupLevel[]), "levels",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                    .DeclareVar(typeof(int), "count", Constant(-1));
                {
                    var forLoop = methodNode.Block.ForEach(typeof(MultiKey<EventBean>), "eventrow", Ref("events"));
                    forLoop.Increment("count")
                        .DeclareVar(
                            typeof(EventBean[]), "eventsPerStream",
                            Cast(typeof(EventBean[]), ExprDotMethod(Ref("eventrow"), "getArray")))
                        .DeclareVar(
                            typeof(object), "groupKeyComplete",
                            LocalMethod(
                                generateGroupKeySingle, Ref("eventsPerStream"),
                                ResultSetProcessorCodegenNames.REF_ISNEWDATA))
                        .AssignArrayElement(
                            "result", Ref("count"), NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))));
                    {
                        forLoop.ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                            .DeclareVar(
                                typeof(object), "subkey",
                                ExprDotMethod(
                                    ArrayAtIndex(Ref("levels"), Ref("j")), "computeSubkey", Ref("groupKeyComplete")))
                            .AssignArrayElement2Dim("result", Ref("count"), Ref("j"), Ref("subkey"))
                            .ExprDotMethod(
                                ArrayAtIndex(
                                    Ref("eventPerKey"),
                                    ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "getLevelNumber")), "put",
                                Ref("subkey"), Ref("eventsPerStream"));
                    }
                }

                methodNode.Block.MethodReturn(Ref("result"));
            };

            return instance.Methods.AddMethod(
                typeof(object[][]), "generateGroupKeysJoin",
                CodegenNamedParam.From(
                    typeof(ISet<object>), "events", typeof(IDictionary<object, object>[]), "eventPerKey", typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroupRollupImpl), classScope, code);
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
                            typeof(EventsAndSortKeysPair), "pair",
                            StaticMethod(
                                typeof(ResultSetProcessorRowPerGroupRollupUtil),
                                ResultSetProcessorRowPerGroupRollupUtil.METHOD_GETOLDEVENTSSORTKEYS,
                                Ref("oldEventCount"), Ref(NAME_RSTREAMEVENTSORTARRAYBUF), REF_ORDERBYPROCESSOR,
                                ExprDotMethod(Ref("this"), "getGroupByRollupDesc")))
                        .AssignRef("oldEventsArr", ExprDotMethod(Ref("pair"), "getEvents"))
                        .AssignRef("oldEventSortKeys", ExprDotMethod(Ref("pair"), "getSortKeys"));
                }

                methodNode.Block.DeclareVar(typeof(IList<object>), "newEvents", NewInstance(typeof(List<object>)))
                    .DeclareVar(
                        typeof(IList<object>), "newEventsSortKey",
                        forge.IsSorting ? NewInstance(typeof(List<object>)) : ConstantNull());

                methodNode.Block.ForEach(
                        typeof(AggregationGroupByRollupLevel), "level",
                        ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                    .DeclareVar(
                        typeof(IDictionary<object, object>), "groupGenerators",
                        ArrayAtIndex(
                            Ref("outputLimitGroupRepsPerLevel"), ExprDotMethod(Ref("level"), "getLevelNumber")))
                    .ForEach(typeof(KeyValuePair<object, object>), "entry", ExprDotMethod(Ref("groupGenerators"), "entrySet"))
                    .LocalMethod(
                        generateOutputBatched, ExprDotMethod(Ref("entry"), "getKey"), Ref("level"),
                        Cast(typeof(EventBean[]), ExprDotMethod(Ref("entry"), "getValue")), ConstantTrue(),
                        REF_ISSYNTHESIZE, Ref("newEvents"), Ref("newEventsSortKey"));

                methodNode.Block.DeclareVar(
                    typeof(EventBean[]), "newEventsArr",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYEVENTS, Ref("newEvents")));
                if (forge.IsSorting) {
                    methodNode.Block.DeclareVar(
                            typeof(object[]), "sortKeysNew",
                            StaticMethod(
                                typeof(CollectionUtil), METHOD_TOARRAYNULLFOREMPTYOBJECTS, Ref("newEventsSortKey")))
                        .AssignRef(
                            "newEventsArr",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR, "sortWOrderKeys", Ref("newEventsArr"), Ref("sortKeysNew"),
                                REF_AGENTINSTANCECONTEXT));
                    if (forge.IsSelectRStream) {
                        methodNode.Block.AssignRef(
                            "oldEventsArr",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR, "sortWOrderKeys", Ref("oldEventsArr"), Ref("oldEventSortKeys"),
                                REF_AGENTINSTANCECONTEXT));
                    }
                }

                methodNode.Block.MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("newEventsArr"),
                        Ref("oldEventsArr")));
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>), "generateAndSort",
                CodegenNamedParam.From(
                    typeof(IDictionary<object, object>[]), "outputLimitGroupRepsPerLevel", typeof(bool),
                    NAME_ISSYNTHESIZE, typeof(int),
                    "oldEventCount"),
                typeof(ResultSetProcessorUtil), classScope, code);
        }

        protected internal static void ApplyViewResultCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysRow = GenerateGroupKeysRowCodegen(forge, classScope, instance);

            method.Block.DeclareVar(
                typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));
            {
                var ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    ifNew.ForEach(typeof(EventBean), "aNewData", REF_NEWDATA)
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("aNewData"))
                        .DeclareVar(
                            typeof(object[]), "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("keys"),
                            REF_AGENTINSTANCECONTEXT);
                }
            }
            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    ifOld.ForEach(typeof(EventBean), "anOldData", REF_OLDDATA)
                        .AssignArrayElement("eventsPerStream", Constant(0), Ref("anOldData"))
                        .DeclareVar(
                            typeof(object[]), "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantFalse()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("keys"),
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
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotMethod(Ref("mk"), "getArray")))
                        .DeclareVar(
                            typeof(object[]), "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantTrue()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", Ref("eventsPerStream"), Ref("keys"),
                            REF_AGENTINSTANCECONTEXT);
                }
            }
            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    ifOld.ForEach(typeof(MultiKey<EventBean>), "mk", REF_OLDDATA)
                        .AssignRef("eventsPerStream", Cast(typeof(EventBean[]), ExprDotMethod(Ref("mk"), "getArray")))
                        .DeclareVar(
                            typeof(object[]), "keys",
                            LocalMethod(generateGroupKeysRow, Ref("eventsPerStream"), ConstantFalse()))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", Ref("eventsPerStream"), Ref("keys"),
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
            var factory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            CodegenExpression eventTypes = classScope.AddFieldUnshared(
                true, typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));

            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowPerGroupRollupOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory, "makeRSRowPerGroupRollupAll", REF_AGENTINSTANCECONTEXT, Ref("this"),
                        Constant(forge.GroupKeyTypes), eventTypes));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTALLHELPER), methodName, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE);
            }
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowPerGroupRollupOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory, "makeRSRowPerGroupRollupLast", REF_AGENTINSTANCECONTEXT, Ref("this"),
                        Constant(forge.GroupKeyTypes), eventTypes));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTLASTHELPER), methodName, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE);
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "processJoin", classScope, method, instance);
        }

        protected internal static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "outputView", REF_ISSYNTHESIZE));
            }
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "outputView", REF_ISSYNTHESIZE));
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
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "outputJoin", REF_ISSYNTHESIZE));
            }
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "outputJoin", REF_ISSYNTHESIZE));
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
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "destroy");
            }

            if (instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                method.Block.ForEach(
                        typeof(ResultSetProcessorGroupedOutputFirstHelper), "helper", Ref(NAME_OUTPUTFIRSTHELPERS))
                    .ExprDotMethod(Ref("helper"), "destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "destroy");
            }
        }

        private static CodegenMethod GenerateGroupKeysRowCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var generateGroupKeySingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);

            Consumer<CodegenMethod> code = methodNode => methodNode.Block.DeclareVar(
                    typeof(object), "groupKeyComplete",
                    LocalMethod(
                        generateGroupKeySingle, Ref("eventsPerStream"), ResultSetProcessorCodegenNames.REF_ISNEWDATA))
                .DeclareVar(
                    typeof(AggregationGroupByRollupLevel[]), "levels",
                    ExprDotMethodChain(Ref("this")).Add("getGroupByRollupDesc").Add("getLevels"))
                .DeclareVar(typeof(object[]), "result", NewArrayByLength(typeof(object), ArrayLength(Ref("levels"))))
                .ForLoopIntSimple("j", ArrayLength(Ref("levels")))
                .DeclareVar(
                    typeof(object), "subkey",
                    ExprDotMethod(ArrayAtIndex(Ref("levels"), Ref("j")), "computeSubkey", Ref("groupKeyComplete")))
                .AssignArrayElement("result", Ref("j"), Ref("subkey"))
                .BlockEnd()
                .MethodReturn(Ref("result"));

            return instance.Methods.AddMethod(
                typeof(object[]), "generateGroupKeysRow",
                CodegenNamedParam.From(
                    typeof(EventBean[]), "eventsPerStream", typeof(bool),
                    ResultSetProcessorCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorUtil), classScope, code);
        }

        private static void InitGroupRepsPerLevelBufCodegen(
            CodegenInstanceAux instance,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            if (!instance.HasMember(NAME_GROUPREPSPERLEVELBUF)) {
                instance.AddMember(NAME_GROUPREPSPERLEVELBUF, typeof(IDictionary<object, object>[]));
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
                        Constant(forge.GroupByRollupDesc.Levels.Length), Constant(forge.IsSorting)));
            }
        }

        private static void InitOutputFirstHelpers(
            CodegenExpressionField outputConditionFactory,
            CodegenInstanceAux instance,
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope)
        {
            if (!instance.HasMember(NAME_OUTPUTFIRSTHELPERS)) {
                var factory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
                instance.AddMember(NAME_OUTPUTFIRSTHELPERS, typeof(ResultSetProcessorGroupedOutputFirstHelper[]));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTFIRSTHELPERS, StaticMethod(
                        typeof(ResultSetProcessorRowPerGroupRollupUtil), "initializeOutputFirstHelpers", factory,
                        REF_AGENTINSTANCECONTEXT, Constant(forge.GroupKeyTypes),
                        ExprDotMethod(Ref("this"), "getGroupByRollupDesc"), outputConditionFactory));
            }
        }
    }
} // end of namespace