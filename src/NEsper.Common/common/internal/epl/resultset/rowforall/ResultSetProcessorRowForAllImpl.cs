///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    /// <summary>
    /// Result set processor for the case: aggregation functions used in the select clause, and no group-by,
    /// and all properties in the select clause are under an aggregation function.
    /// <para>
    /// This processor does not perform grouping, every event entering and leaving is in the same group.
    /// Produces one old event and one new event row every time either at least one old or new event is received.
    /// Aggregation state is simply one row holding all the state.
    /// </para>
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
                method.Block.Expression(ExprDotMethod(Ref("this"), "Clear"));
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
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGJOINRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    REF_OLDDATA)
                .DeclareVar(
                    typeof(EventBean[]),
                    "selectNewEvents",
                    LocalMethod(selectList, ConstantTrue(), REF_ISSYNTHESIZE, ConstantTrue()))
                .IfCondition(And(EqualsNull(Ref("selectNewEvents")), EqualsNull(Ref("selectOldEvents"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean[]>), Ref("selectNewEvents"), Ref("selectOldEvents")));
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
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGVIEWRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    Ref("eventsPerStream"))
                .DeclareVar(
                    typeof(EventBean[]),
                    "selectNewEvents",
                    LocalMethod(selectList, ConstantTrue(), REF_ISSYNTHESIZE, ConstantFalse()))
                .IfCondition(And(EqualsNull(Ref("selectNewEvents")), EqualsNull(Ref("selectOldEvents"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean[]>), Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        internal static void GetEnumeratorViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var obtainMethod = ObtainEnumeratorCodegen(forge, classScope, method, instance);
            if (!forge.IsHistoricalOnly) {
                method.Block.MethodReturn(LocalMethod(obtainMethod));
                return;
            }

            method.Block
                .StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_CLEARANDAGGREGATEUNGROUPED,
                    MEMBER_EXPREVALCONTEXT,
                    MEMBER_AGGREGATIONSVC,
                    REF_VIEWABLE)
                .DeclareVar(typeof(IEnumerator<EventBean>), "enumerator", LocalMethod(obtainMethod))
                .Expression(ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT))
                .MethodReturn(Ref("enumerator"));
        }

        public static void GetEnumeratorJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var select = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);
            method.Block
                .DeclareVar(
                    typeof(EventBean[]),
                    "result",
                    LocalMethod(select, Constant(true), ConstantTrue(), ConstantTrue()))
                .MethodReturn(NewInstance(typeof(ArrayEventEnumerator), Ref("result")));
        }

        public static void ClearCodegen(CodegenMethod method)
        {
            method.Block.Expression(ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT));
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
                typeof(ResultSetProcessorUtil),
                METHOD_APPLYAGGVIEWRESULT,
                MEMBER_AGGREGATIONSVC,
                MEMBER_EXPREVALCONTEXT,
                REF_NEWDATA,
                REF_OLDDATA,
                NewArrayByLength(typeof(EventBean), Constant(1)));
        }

        public static void ApplyJoinResultCodegen(CodegenMethod method)
        {
            method.Block.StaticMethod(
                typeof(ResultSetProcessorUtil),
                METHOD_APPLYAGGJOINRESULT,
                MEMBER_AGGREGATIONSVC,
                MEMBER_EXPREVALCONTEXT,
                REF_NEWDATA,
                REF_OLDDATA);
        }

        public static void StopCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "Destroy");
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen("ProcessView", forge, classScope, method, instance);
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen("ProcessJoin", forge, classScope, method, instance);
        }

        internal static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            string methodName,
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);

            if (forge.IsOutputAll) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorRowForAllOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowForAllOutputAll",
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT,
                        forge.OutputAllHelperSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTALLHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
            else if (forge.IsOutputLast) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorRowForAllOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowForAllOutputLast",
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT,
                        forge.OutputLastHelperSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTLASTHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenMethod method)
        {
            if (forge.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
            else {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
        }

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLHELPER));
            }
        }

        internal static void ProcessOutputLimitedJoinDefaultCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventAddList = GetSelectListEventsAddListCodegen(forge, classScope, instance);
            var getSelectListEventAsArray = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "Clear");
                }

                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(getSelectListEventAddList, ConstantFalse(), REF_ISSYNTHESIZE, Ref("oldEvents"));
                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("oldEventsSortKey"),
                            "Add",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "GetSortKey",
                                ConstantNull(),
                                ConstantFalse(),
                                MEMBER_EXPREVALCONTEXT));
                    }
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGJOINRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    ExprDotName(Ref("pair"), "First"),
                    ExprDotName(Ref("pair"), "Second"));
                forEach.LocalMethod(getSelectListEventAddList, ConstantTrue(), REF_ISSYNTHESIZE, Ref("newEvents"));
                if (forge.IsSorting) {
                    forEach.ExprDotMethod(
                        Ref("newEventsSortKey"),
                        "Add",
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "GetSortKey",
                            ConstantNull(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }

                forEach.BlockEnd();
            }

            var ifEmpty = method.Block.IfCondition(Not(ExprDotMethod(REF_JOINEVENTSSET, "IsEmpty")));
            FinalizeOutputMaySortMayRStreamCodegen(
                ifEmpty,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);

            method.Block.DeclareVar(
                    typeof(EventBean[]),
                    "newEventsArr",
                    LocalMethod(getSelectListEventAsArray, ConstantTrue(), REF_ISSYNTHESIZE, ConstantFalse()))
                .DeclareVar(
                    typeof(EventBean[]),
                    "oldEventsArr",
                    forge.IsSelectRStream
                        ? LocalMethod(getSelectListEventAsArray, ConstantFalse(), REF_ISSYNTHESIZE, ConstantFalse())
                        : ConstantNull())
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("newEventsArr"),
                        Ref("oldEventsArr")));
        }

        internal static void ProcessOutputLimitedJoinLastCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventSingle = GetSelectListEventSingleCodegen(forge, classScope, instance);

            method.Block
                .DeclareVar<EventBean>("lastOldEvent", ConstantNull())
                .DeclareVar<EventBean>("lastNewEvent", ConstantNull());

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "Clear");
                }

                if (forge.IsSelectRStream) {
                    forEach.IfCondition(EqualsNull(Ref("lastOldEvent")))
                        .AssignRef(
                            "lastOldEvent",
                            LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .BlockEnd();
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGJOINRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    ExprDotName(Ref("pair"), "First"),
                    ExprDotName(Ref("pair"), "Second"));
                forEach.AssignRef(
                    "lastNewEvent",
                    LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
            }

            {
                var ifEmpty = method.Block.IfCondition(ExprDotMethod(REF_JOINEVENTSSET, "IsEmpty"));
                if (forge.IsSelectRStream) {
                    ifEmpty.AssignRef(
                            "lastOldEvent",
                            LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .AssignRef("lastNewEvent", Ref("lastOldEvent"));
                }
                else {
                    ifEmpty.AssignRef(
                        "lastNewEvent",
                        LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
                }
            }

            method.Block
                .DeclareVar(
                    typeof(EventBean[]),
                    "lastNew",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastNewEvent")))
                .DeclareVar(
                    typeof(EventBean[]),
                    "lastOld",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastOldEvent")))
                .IfCondition(And(EqualsNull(Ref("lastNew")), EqualsNull(Ref("lastOld"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean[]>), Ref("lastNew"), Ref("lastOld")));
        }

        internal static void ProcessOutputLimitedViewDefaultCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventAddList = GetSelectListEventsAddListCodegen(forge, classScope, instance);
            var getSelectListEventAsArray = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);

            PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            method.Block.DeclareVar(
                typeof(EventBean[]),
                "eventsPerStream",
                NewArrayByLength(typeof(EventBean), Constant(1)));
            var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
            {
                if (forge.IsSelectRStream) {
                    forEach.LocalMethod(getSelectListEventAddList, ConstantFalse(), REF_ISSYNTHESIZE, Ref("oldEvents"));
                    if (forge.IsSorting) {
                        forEach.ExprDotMethod(
                            Ref("oldEventsSortKey"),
                            "Add",
                            ExprDotMethod(
                                MEMBER_ORDERBYPROCESSOR,
                                "GetSortKey",
                                ConstantNull(),
                                ConstantFalse(),
                                MEMBER_EXPREVALCONTEXT));
                    }
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGVIEWRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    ExprDotName(Ref("pair"), "First"),
                    ExprDotName(Ref("pair"), "Second"),
                    Ref("eventsPerStream"));
                forEach.LocalMethod(getSelectListEventAddList, ConstantTrue(), REF_ISSYNTHESIZE, Ref("newEvents"));
                if (forge.IsSorting) {
                    forEach.ExprDotMethod(
                        Ref("newEventsSortKey"),
                        "Add",
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "GetSortKey",
                            ConstantNull(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }

                forEach.BlockEnd();
            }

            var ifEmpty = method.Block.IfCondition(Not(ExprDotMethod(REF_VIEWEVENTSLIST, "IsEmpty")));
            FinalizeOutputMaySortMayRStreamCodegen(
                ifEmpty,
                Ref("newEvents"),
                Ref("newEventsSortKey"),
                Ref("oldEvents"),
                Ref("oldEventsSortKey"),
                forge.IsSelectRStream,
                forge.IsSorting);

            method.Block.DeclareVar(
                    typeof(EventBean[]),
                    "newEventsArr",
                    LocalMethod(getSelectListEventAsArray, ConstantTrue(), REF_ISSYNTHESIZE, ConstantFalse()))
                .DeclareVar(
                    typeof(EventBean[]),
                    "oldEventsArr",
                    forge.IsSelectRStream
                        ? LocalMethod(getSelectListEventAsArray, ConstantFalse(), REF_ISSYNTHESIZE, ConstantFalse())
                        : ConstantNull())
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("newEventsArr"),
                        Ref("oldEventsArr")));
        }

        internal static void ProcessOutputLimitedViewLastCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var getSelectListEventSingle = GetSelectListEventSingleCodegen(forge, classScope, instance);

            method.Block.DeclareVar<EventBean>("lastOldEvent", ConstantNull())
                .DeclareVar<EventBean>("lastNewEvent", ConstantNull())
                .DeclareVar(typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var forEach = method.Block.ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                if (forge.IsSelectRStream) {
                    forEach.IfCondition(EqualsNull(Ref("lastOldEvent")))
                        .AssignRef(
                            "lastOldEvent",
                            LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .BlockEnd();
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGVIEWRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    ExprDotName(Ref("pair"), "First"),
                    ExprDotName(Ref("pair"), "Second"),
                    Ref("eventsPerStream"));
                forEach.AssignRef(
                    "lastNewEvent",
                    LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
            }

            {
                var ifEmpty = method.Block.IfCondition(ExprDotMethod(REF_VIEWEVENTSLIST, "IsEmpty"));
                if (forge.IsSelectRStream) {
                    ifEmpty.AssignRef(
                            "lastOldEvent",
                            LocalMethod(getSelectListEventSingle, ConstantFalse(), REF_ISSYNTHESIZE))
                        .AssignRef("lastNewEvent", Ref("lastOldEvent"));
                }
                else {
                    ifEmpty.AssignRef(
                        "lastNewEvent",
                        LocalMethod(getSelectListEventSingle, ConstantTrue(), REF_ISSYNTHESIZE));
                }
            }

            method.Block
                .DeclareVar(
                    typeof(EventBean[]),
                    "lastNew",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastNewEvent")))
                .DeclareVar(
                    typeof(EventBean[]),
                    "lastOld",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastOldEvent")))
                .IfCondition(And(EqualsNull(Ref("lastNew")), EqualsNull(Ref("lastOld"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean[]>), Ref("lastNew"), Ref("lastOld")));
        }

        internal static CodegenMethod ObtainEnumeratorCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent,
            CodegenInstanceAux instance)
        {
            var selectList = GetSelectListEventsAsArrayCodegen(forge, classScope, instance);
            var method = parent.MakeChild(
                typeof(IEnumerator<EventBean>),
                typeof(ResultSetProcessorRowForAllImpl),
                classScope);
            method.Block.DeclareVar(
                    typeof(EventBean[]),
                    "events",
                    LocalMethod(selectList, ConstantTrue(), ConstantTrue(), ConstantFalse()))
                .IfRefNull("events")
                .BlockReturn(EnumValue(typeof(CollectionUtil), "NULL_EVENT_ITERATOR"))
                .MethodReturn(NewInstance(typeof(SingleEventEnumerator), ArrayAtIndex(Ref("events"), Constant(0))));
            return method;
        }

        internal static CodegenMethod GetSelectListEventSingleCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                    if (forge.OptionalHavingNode != null) {
                        method.Block.IfCondition(
                                Not(
                                    LocalMethod(
                                        instance.Methods.GetMethod("EvaluateHavingClause"),
                                        ConstantNull(),
                                        REF_ISNEWDATA,
                                        MEMBER_EXPREVALCONTEXT)))
                            .BlockReturn(ConstantNull());
                    }

                    method.Block.MethodReturn(
                        ExprDotMethod(
                            MEMBER_SELECTEXPRPROCESSOR,
                            "Process",
                            EnumValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                };
            
            return instance.Methods.AddMethod(
                typeof(EventBean),
                "GetSelectListEventSingle",
                CodegenNamedParam.From(typeof(bool), NAME_ISNEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowForAllImpl),
                classScope,
                code);
        }

        internal static CodegenMethod GetSelectListEventsAddListCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    ConstantNull(),
                                    REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturnNoValue();
                }

                method.Block.DeclareVar<EventBean>("theEvent",
                        ExprDotMethod(
                            MEMBER_SELECTEXPRPROCESSOR,
                            "Process",
                            EnumValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT))
                    .Expression(ExprDotMethod(Ref("resultEvents"), "Add", Ref("theEvent")));
            };
            return instance.Methods.AddMethod(
                typeof(void),
                "GetSelectListEventsAddList",
                CodegenNamedParam.From(
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(IList<EventBean>),
                    "resultEvents"),
                typeof(ResultSetProcessorRowForAllImpl),
                classScope,
                code);
        }

        public static CodegenMethod GetSelectListEventsAsArrayCodegen(
            ResultSetProcessorRowForAllForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                if (forge.OptionalHavingNode != null) {
                    method.Block.IfCondition(
                            Not(
                                LocalMethod(
                                    instance.Methods.GetMethod("EvaluateHavingClause"),
                                    ConstantNull(),
                                    REF_ISNEWDATA,
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockReturn(ConstantNull());
                }

                method.Block.DeclareVar(
                        typeof(EventBean),
                        "theEvent",
                        ExprDotMethod(
                            MEMBER_SELECTEXPRPROCESSOR,
                            "Process",
                            EnumValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"),
                            REF_ISNEWDATA,
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT))
                    .DeclareVar(typeof(EventBean[]), "result", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .AssignArrayElement("result", Constant(0), Ref("theEvent"))
                    .MethodReturn(Ref("result"));
            };
            return instance.Methods.AddMethod(
                typeof(EventBean[]),
                "GetSelectListEventsAsArray",
                CodegenNamedParam.From(
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(bool),
                    NAME_ISSYNTHESIZE,
                    typeof(bool),
                    "join"),
                typeof(ResultSetProcessorRowForAllImpl),
                classScope,
                code);
        }
    }
} // end of namespace