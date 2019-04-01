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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    /// <summary>
    ///     Result set processor for the case: aggregation functions used in the select clause, and no group-by,
    ///     and all properties in the select clause are under an aggregation function.
    ///     <para />
    ///     This processor does not perform grouping, every event entering and leaving is in the same group.
    ///     Produces one old event and one new event row every time either at least one old or new event is received.
    ///     Aggregation state is simply one row holding all the state.
    /// </summary>
    public class ResultSetProcessorRowForAllImpl
    {
        private const string NAME_OUTPUTALLHELPER = "outputAllHelper";
        private const string NAME_OUTPUTLASTHELPER = "outputLastHelper";

        public static void ProcessJoinResultCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instanceMethods)
        {
            var selectList = GetSelectListEventsAsArrayCodegen(forge, classScope, instanceMethods);

            if (forge.IsUnidirectional) {
                method.Block.Expression(ExprDotMethod(Ref("this"), "clear"));
            }

            CodegenExpression selectOld;
            if (forge.IsSelectRStream) {
                selectOld = LocalMethod(selectList, ConstantFalse(), REF_ISSYNTHESIZE, ConstantTrue());
            }
            else {
                selectOld = ConstantNull();
            }

            method.Block
                .DeclareVar(typeof(EventBean[]), "selectOldEvents", selectOld)
                .StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_APPLYAGGJOINRESULT, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, REF_NEWDATA, REF_OLDDATA)
                .DeclareVar(
                    typeof(EventBean[]), "selectNewEvents",
                    LocalMethod(selectList, ConstantTrue(), REF_ISSYNTHESIZE, ConstantTrue()))
                .IfCondition(And(EqualsNull(Ref("selectNewEvents")), EqualsNull(Ref("selectOldEvents"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    NewInstance(typeof(UniformPair<EventBean>), Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        public static void ProcessViewResultCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var selectList = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);

            CodegenExpression selectOld;
            if (forge.IsSelectRStream) {
                selectOld = LocalMethod(selectList, ConstantFalse(), REF_ISSYNTHESIZE, ConstantFalse());
            }
            else {
                selectOld = ConstantNull();
            }

            method.Block
                .DeclareVar(typeof(EventBean[]), "selectOldEvents", selectOld)
                .DeclareVar(typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_APPLYAGGVIEWRESULT, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, REF_NEWDATA, REF_OLDDATA, Ref("eventsPerStream"))
                .DeclareVar(
                    typeof(EventBean[]), "selectNewEvents",
                    LocalMethod(selectList, ConstantTrue(), REF_ISSYNTHESIZE, ConstantFalse()))
                .IfCondition(And(EqualsNull(Ref("selectNewEvents")), EqualsNull(Ref("selectOldEvents"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    NewInstance(typeof(UniformPair<EventBean>), Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        protected internal static void GetIteratorViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var obtainMethod = ObtainIteratorCodegen(forge, classScope, method, instance);
            if (!forge.IsHistoricalOnly) {
                method.Block.MethodReturn(LocalMethod(obtainMethod));
                return;
            }

            method.Block
                .StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_CLEARANDAGGREGATEUNGROUPED, REF_AGENTINSTANCECONTEXT,
                    REF_AGGREGATIONSVC, REF_VIEWABLE)
                .DeclareVar(typeof(IEnumerator<EventBean>), "iterator", LocalMethod(obtainMethod))
                .Expression(ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT))
                .MethodReturn(Ref("iterator"));
        }

        public static void GetIteratorJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var select = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);
            method.Block
                .DeclareVar(
                    typeof(EventBean[]), "result", LocalMethod(select, Constant(true), ConstantTrue(), ConstantTrue()))
                .MethodReturn(NewInstance(typeof(ArrayEventIterator), Ref("result")));
        }

        public static void ClearCodegen(CodegenMethod method)
        {
            method.Block.Expression(ExprDotMethod(REF_AGGREGATIONSVC, "clearResults", REF_AGENTINSTANCECONTEXT));
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                ProcessOutputLimitedJoinLastCodegen(forge, classScope, method, instance);
            }
            else {
                ProcessOutputLimitedJoinDefaultCodegen(forge, classScope, method, instance);
            }
        }

        public static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                ProcessOutputLimitedViewLastCodegen(forge, classScope, method, instance);
            }
            else {
                ProcessOutputLimitedViewDefaultCodegen(forge, classScope, method, instance);
            }
        }

        public static void ApplyViewResultCodegen(CodegenMethod method)
        {
            method.Block.StaticMethod(
                typeof(ResultSetProcessorUtil), METHOD_APPLYAGGVIEWRESULT, REF_AGGREGATIONSVC, REF_AGENTINSTANCECONTEXT,
                REF_NEWDATA, REF_OLDDATA, NewArrayByLength(typeof(EventBean), Constant(1)));
        }

        public static void ApplyJoinResultCodegen(CodegenMethod method)
        {
            method.Block.StaticMethod(
                typeof(ResultSetProcessorUtil), METHOD_APPLYAGGJOINRESULT, REF_AGGREGATIONSVC, REF_AGENTINSTANCECONTEXT,
                REF_NEWDATA, REF_OLDDATA);
        }

        public static void StopCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "destroy");
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen("processView", forge, classScope, method, instance);
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen("processJoin", forge, classScope, method, instance);
        }

        protected internal static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            string methodName,
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);

            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowForAllOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(factory, "makeRSRowForAllOutputAll", Ref("this"), REF_AGENTINSTANCECONTEXT));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTALLHELPER), methodName, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE);
            }
            else if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowForAllOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(factory, "makeRSRowForAllOutputLast", Ref("this"), REF_AGENTINSTANCECONTEXT));
                method.Block.ExprDotMethod(
                    Ref(NAME_OUTPUTLASTHELPER), methodName, REF_NEWDATA, REF_OLDDATA, REF_ISSYNTHESIZE);
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "outputView", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "outputView", REF_ISSYNTHESIZE));
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "outputJoin", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "outputJoin", REF_ISSYNTHESIZE));
            }
        }

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "visit", Ref(NAME_OUTPUTALLHELPER));
            }
        }

        protected internal static void ProcessOutputLimitedJoinDefaultCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventAddList = GetSelectListEventsAddListCodegen(forge, classScope, instance);
            var getSelectListEventAsArray = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_JOINEVENTSSET);
                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "clear");
                }

                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(getSelectListEventAddList, ConstantFalse(), REF_ISSYNTHESIZE, Ref("oldEvents"));
                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("oldEventsSortKey"), "add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR, "getSortKey", ConstantNull(), ConstantFalse(),
                                REF_AGENTINSTANCECONTEXT));
                    }
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_APPLYAGGJOINRESULT, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")),
                    Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")));
                forEach.LocalMethod(getSelectListEventAddList, ConstantTrue(), REF_ISSYNTHESIZE, Ref("newEvents"));
                if (forge.IsSorting) {
                    forEach.ExprDotMethod(
                        Ref("newEventsSortKey"), "add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR, "getSortKey", ConstantNull(), ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT));
                }

                forEach.BlockEnd();
            }

            var ifEmpty = method.Block.IfCondition(Not(ExprDotMethod(REF_JOINEVENTSSET, "isEmpty")));
            FinalizeOutputMaySortMayRStreamCodegen(
                ifEmpty, Ref("newEvents"), Ref("newEventsSortKey"), Ref("oldEvents"), Ref("oldEventsSortKey"),
                forge.IsSelectRStream, forge.IsSorting);

            method.Block.DeclareVar(
                    typeof(EventBean[]), "newEventsArr",
                    LocalMethod(getSelectListEventAsArray, ConstantTrue(), REF_ISSYNTHESIZE, ConstantFalse()))
                .DeclareVar(
                    typeof(EventBean[]), "oldEventsArr",
                    forge.IsSelectRStream
                        ? LocalMethod(getSelectListEventAsArray, ConstantFalse(), REF_ISSYNTHESIZE, ConstantFalse())
                        : ConstantNull())
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("newEventsArr"),
                        Ref("oldEventsArr")));
        }

        protected internal static void ProcessOutputLimitedJoinLastCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventSingle = GetSelectListEventSingleCodegen(forge, classScope, instance);

            method.Block.DeclareVar(typeof(EventBean), "lastOldEvent", ConstantNull())
                .DeclareVar(typeof(EventBean), "lastNewEvent", ConstantNull());

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_JOINEVENTSSET);
                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "clear");
                }

                if (forge.IsSelectRStream) {
                    forEach.IfCondition(EqualsNull(Ref("lastOldEvent")))
                        .AssignRef(
                            "lastOldEvent", LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .BlockEnd();
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_APPLYAGGJOINRESULT, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getFirst")),
                    Cast(typeof(ISet<EventBean>), ExprDotMethod(Ref("pair"), "getSecond")));
                forEach.AssignRef(
                    "lastNewEvent", LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
            }

            {
                var ifEmpty = method.Block.IfCondition(ExprDotMethod(REF_JOINEVENTSSET, "isEmpty"));
                if (forge.IsSelectRStream) {
                    ifEmpty.AssignRef(
                            "lastOldEvent", LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .AssignRef("lastNewEvent", Ref("lastOldEvent"));
                }
                else {
                    ifEmpty.AssignRef(
                        "lastNewEvent", LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
                }
            }

            method.Block
                .DeclareVar(
                    typeof(EventBean[]), "lastNew",
                    StaticMethod(typeof(CollectionUtil), CollectionUtil.METHOD_TOARRAYMAYNULL, Ref("lastNewEvent")))
                .DeclareVar(
                    typeof(EventBean[]), "lastOld",
                    StaticMethod(typeof(CollectionUtil), CollectionUtil.METHOD_TOARRAYMAYNULL, Ref("lastOldEvent")))
                .IfCondition(And(EqualsNull(Ref("lastNew")), EqualsNull(Ref("lastOld"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean>), Ref("lastNew"), Ref("lastOld")));
        }

        protected internal static void ProcessOutputLimitedViewDefaultCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventAddList = GetSelectListEventsAddListCodegen(forge, classScope, instance);
            var getSelectListEventAsArray = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar(
                typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));
            var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_VIEWEVENTSLIST);
            {
                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(getSelectListEventAddList, ConstantFalse(), REF_ISSYNTHESIZE, Ref("oldEvents"));
                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("oldEventsSortKey"), "add",
                            ExprDotMethod(
                                REF_ORDERBYPROCESSOR, "getSortKey", ConstantNull(), ConstantFalse(),
                                REF_AGENTINSTANCECONTEXT));
                    }
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_APPLYAGGVIEWRESULT, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")),
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")), Ref("eventsPerStream"));
                forEach.LocalMethod(getSelectListEventAddList, ConstantTrue(), REF_ISSYNTHESIZE, Ref("newEvents"));
                if (forge.IsSorting) {
                    forEach.ExprDotMethod(
                        Ref("newEventsSortKey"), "add",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR, "getSortKey", ConstantNull(), ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT));
                }

                forEach.BlockEnd();
            }

            var ifEmpty = method.Block.IfCondition(Not(ExprDotMethod(REF_VIEWEVENTSLIST, "isEmpty")));
            FinalizeOutputMaySortMayRStreamCodegen(
                ifEmpty, Ref("newEvents"), Ref("newEventsSortKey"), Ref("oldEvents"), Ref("oldEventsSortKey"),
                forge.IsSelectRStream, forge.IsSorting);

            method.Block.DeclareVar(
                    typeof(EventBean[]), "newEventsArr",
                    LocalMethod(getSelectListEventAsArray, ConstantTrue(), REF_ISSYNTHESIZE, ConstantFalse()))
                .DeclareVar(
                    typeof(EventBean[]), "oldEventsArr",
                    forge.IsSelectRStream
                        ? LocalMethod(getSelectListEventAsArray, ConstantFalse(), REF_ISSYNTHESIZE, ConstantFalse())
                        : ConstantNull())
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, Ref("newEventsArr"),
                        Ref("oldEventsArr")));
        }

        protected internal static void ProcessOutputLimitedViewLastCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventSingle = GetSelectListEventSingleCodegen(forge, classScope, instance);

            method.Block.DeclareVar(typeof(EventBean), "lastOldEvent", ConstantNull())
                .DeclareVar(typeof(EventBean), "lastNewEvent", ConstantNull())
                .DeclareVar(typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean>), "pair", REF_VIEWEVENTSLIST);
                if (forge.IsSelectRStream) {
                    forEach.IfCondition(EqualsNull(Ref("lastOldEvent")))
                        .AssignRef(
                            "lastOldEvent", LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .BlockEnd();
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil), METHOD_APPLYAGGVIEWRESULT, REF_AGGREGATIONSVC,
                    REF_AGENTINSTANCECONTEXT, Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getFirst")),
                    Cast(typeof(EventBean[]), ExprDotMethod(Ref("pair"), "getSecond")), Ref("eventsPerStream"));
                forEach.AssignRef(
                    "lastNewEvent", LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
            }

            {
                var ifEmpty = method.Block.IfCondition(ExprDotMethod(REF_VIEWEVENTSLIST, "isEmpty"));
                if (forge.IsSelectRStream) {
                    ifEmpty.AssignRef(
                            "lastOldEvent", LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .AssignRef("lastNewEvent", Ref("lastOldEvent"));
                }
                else {
                    ifEmpty.AssignRef(
                        "lastNewEvent", LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
                }
            }

            method.Block
                .DeclareVar(
                    typeof(EventBean[]), "lastNew",
                    StaticMethod(typeof(CollectionUtil), CollectionUtil.METHOD_TOARRAYMAYNULL, Ref("lastNewEvent")))
                .DeclareVar(
                    typeof(EventBean[]), "lastOld",
                    StaticMethod(typeof(CollectionUtil), CollectionUtil.METHOD_TOARRAYMAYNULL, Ref("lastOldEvent")))
                .IfCondition(And(EqualsNull(Ref("lastNew")), EqualsNull(Ref("lastOld"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean>), Ref("lastNew"), Ref("lastOld")));
        }

        protected internal static CodegenMethod ObtainIteratorCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent,
            CodegenInstanceAux instance)
        {
            var selectList = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);
            var method = parent.MakeChild(
                typeof(IEnumerator<EventBean>), typeof(ResultSetProcessorRowForAllImpl), classScope);
            method.Block.DeclareVar(
                    typeof(EventBean[]), "events",
                    LocalMethod(selectList, ConstantTrue(), ConstantTrue(), ConstantFalse()))
                .IfRefNull("events")
                .BlockReturn(EnumValue(typeof(CollectionUtil), "NULL_EVENT_ITERATOR"))
                .MethodReturn(NewInstance(typeof(SingleEventIterator), ArrayAtIndex(Ref("events"), Constant(0))));
            return method;
        }

        protected internal static CodegenMethod GetSelectListEventSingleCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"), ConstantNull(), REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                method.Block.MethodReturn(
                    ExprDotMethod(
                        REF_SELECTEXPRPROCESSOR, "process", EnumValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"),
                        REF_ISNEWDATA, REF_ISSYNTHESIZE, REF_AGENTINSTANCECONTEXT));
            };
            return instance.Methods.AddMethod(
                typeof(EventBean), "getSelectListEventSingle",
                CodegenNamedParam.From(typeof(bool), NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowForAllImpl), classScope, code);
        }

        protected internal static CodegenMethod GetSelectListEventsAddListCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"), ConstantNull(), REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturnNoValue();
                }

                method.Block.DeclareVar(
                        typeof(EventBean), "theEvent",
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR, "process",
                            EnumValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"), REF_ISNEWDATA, REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT))
                    .Expression(ExprDotMethod(Ref("resultEvents"), "add", Ref("theEvent")));
            };
            return instance.Methods.AddMethod(
                typeof(void), "getSelectListEventsAddList",
                CodegenNamedParam.From(
                    typeof(bool), NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE, typeof(IList<object>),
                    "resultEvents"),
                typeof(ResultSetProcessorRowForAllImpl), classScope, code);
        }

        protected internal static CodegenMethod GetSelectListEventsAsArrayCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("evaluateHavingClause"), ConstantNull(), REF_ISNEWDATA,
                                    REF_AGENTINSTANCECONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                method.Block.DeclareVar(
                        typeof(EventBean), "theEvent",
                        ExprDotMethod(
                            REF_SELECTEXPRPROCESSOR, "process",
                            EnumValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"), REF_ISNEWDATA, REF_ISSYNTHESIZE,
                            REF_AGENTINSTANCECONTEXT))
                    .DeclareVar(typeof(EventBean[]), "result", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .AssignArrayElement("result", Constant(0), Ref("theEvent"))
                    .MethodReturn(Ref("result"));
            };
            return instance.Methods.AddMethod(
                typeof(EventBean[]), "getSelectListEventsAsArray",
                CodegenNamedParam.From(
                    typeof(bool), NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE, typeof(bool), "join"),
                typeof(ResultSetProcessorRowForAllImpl), classScope, code);
        }
    }
} // end of namespace