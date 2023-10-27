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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    /// <summary>
    /// Result set processor for the case: aggregation functions used in the select clause, and no group-by,
    /// and not all of the properties in the select clause are under an aggregation function.
    /// <para />This processor does not perform grouping, every event entering and leaving is in the same group.
    /// The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
    /// Aggregation state is simply one row holding all the state.
    /// </summary>
    public class ResultSetProcessorRowPerEventImpl
    {
        private const string NAME_OUTPUTALLUNORDHELPER = "outputAllUnordHelper";
        private const string NAME_OUTPUTLASTUNORDHELPER = "outputLastUnordHelper";

        public static void ApplyViewResultCodegen(CodegenMethod method)
        {
            method.Block.DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGVIEWRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    Ref("eventsPerStream"));
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

        public static void ProcessJoinResultCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar<EventBean[]>("selectOldEvents", ConstantNull())
                .DeclareVarNoInit(typeof(EventBean[]), "selectNewEvents");

            if (forge.IsUnidirectional) {
                method.Block.ExprDotMethod(Ref("this"), "Clear");
            }

            method.Block.StaticMethod(
                typeof(ResultSetProcessorUtil),
                METHOD_APPLYAGGJOINRESULT,
                MEMBER_AGGREGATIONSVC,
                MEMBER_EXPREVALCONTEXT,
                REF_NEWDATA,
                REF_OLDDATA);

            ResultSetProcessorUtil.ProcessJoinResultCodegen(
                method,
                classScope,
                instance,
                forge.OptionalHavingNode != null,
                forge.IsSelectRStream,
                forge.IsSorting,
                true);
        }

        public static void ProcessViewResultCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar<EventBean[]>("selectOldEvents", ConstantNull())
                .DeclareVarNoInit(typeof(EventBean[]), "selectNewEvents")
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGVIEWRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    Ref("eventsPerStream"));

            ResultSetProcessorUtil.ProcessViewResultCodegen(
                method,
                classScope,
                instance,
                forge.OptionalHavingNode != null,
                forge.IsSelectRStream,
                forge.IsSorting,
                true);
        }

        public static void GetEnumeratorViewCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            if (!forge.IsHistoricalOnly) {
                method.Block.MethodReturn(LocalMethod(ObtainEnumeratorCodegen(forge, classScope, method), REF_VIEWABLE));
                return;
            }

            method.Block
                .StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_CLEARANDAGGREGATEUNGROUPED,
                    MEMBER_EXPREVALCONTEXT,
                    MEMBER_AGGREGATIONSVC,
                    REF_VIEWABLE)
                .DeclareVar(
                    typeof(IEnumerator<EventBean>),
                    "iterator",
                    LocalMethod(ObtainEnumeratorCodegen(forge, classScope, method), REF_VIEWABLE))
                .DeclareVar(
                    typeof(ArrayDeque<EventBean>),
                    "deque",
                    StaticMethod(typeof(ResultSetProcessorUtil), METHOD_ITERATORTODEQUE, Ref("iterator")))
                .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT)
                .MethodReturn(ExprDotMethod(Ref("deque"), "GetEnumerator"));
        }

        private static CodegenMethod ObtainEnumeratorCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent)
        {
            var iterator = parent.MakeChild(
                    typeof(IEnumerator<EventBean>),
                    typeof(ResultSetProcessorRowPerEventImpl),
                    classScope)
                .AddParam<Viewable>(NAME_VIEWABLE);
            if (!forge.IsSorting) {
                iterator.Block.MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorRowPerEventEnumerator),
                        "For",
                        ExprDotMethod(REF_VIEWABLE, "GetEnumerator"),
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT));
                return iterator;
            }

            iterator.Block
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar(typeof(IList<EventBean>), "outgoingEvents", NewInstance(typeof(List<EventBean>)))
                .DeclareVar(typeof(IList<object>), "orderKeys", NewInstance(typeof(List<object>)));

            {
                var forEach = iterator.Block.ForEach(typeof(EventBean), "candidate", REF_VIEWABLE);
                forEach.AssignArrayElement("eventsPerStream", Constant(0), Ref("candidate"));
                if (forge.OptionalHavingNode != null) {
                    forEach.IfCondition(
                            Not(
                                ExprDotMethod(
                                    Ref("this"),
                                    "EvaluateHavingClause",
                                    Ref("eventsPerStream"),
                                    Constant(true),
                                    MEMBER_EXPREVALCONTEXT)))
                        .BlockContinue();
                }

                forEach.ExprDotMethod(
                        Ref("outgoingEvents"),
                        "Add",
                        ExprDotMethod(
                            MEMBER_SELECTEXPRPROCESSOR,
                            "Process",
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT))
                    .ExprDotMethod(
                        Ref("orderKeys"),
                        "Add",
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "GetSortKey",
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
            }

            iterator.Block.MethodReturn(
                StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_ORDEROUTGOINGGETITERATOR,
                    Ref("outgoingEvents"),
                    Ref("orderKeys"),
                    MEMBER_ORDERBYPROCESSOR,
                    MEMBER_EXPREVALCONTEXT));
            return iterator;
        }

        public static void GetEnumeratorJoinCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.OptionalHavingNode == null) {
                if (!forge.IsSorting) {
                    method.Block.DeclareVar<EventBean[]>(
                        "result",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTJOINEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_JOINSET,
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    method.Block.DeclareVar<EventBean[]>(
                        "result",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTJOINEVENTSNOHAVINGWITHORDERBY,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_JOINSET,
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }
            }
            else {
                if (!forge.IsSorting) {
                    var select = GetSelectJoinEventsHavingCodegen(classScope, instance);
                    method.Block.DeclareVar<EventBean[]>(
                        "result",
                        LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_JOINSET,
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    var select = GetSelectJoinEventsHavingWithOrderByCodegen(classScope, instance);
                    method.Block.DeclareVar<EventBean[]>(
                        "result",
                        LocalMethod(
                            select,
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_JOINSET,
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }
            }

            method.Block.MethodReturn(NewInstance(typeof(ArrayEventEnumerator), Ref("result")));
        }

        public static void ClearMethodCodegen(CodegenMethod method)
        {
            method.Block.ExprDotMethod(MEMBER_AGGREGATIONSVC, "ClearResults", MEMBER_EXPREVALCONTEXT);
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsOutputLast) {
                ProcessOutputLimitedJoinLastCodegen(forge, classScope, method, instance);
            }
            else {
                ProcessOutputLimitedJoinDefaultCodegen(forge, classScope, method, instance);
            }
        }

        public static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsOutputLast) {
                ProcessOutputLimitedViewLastCodegen(forge, classScope, method, instance);
            }
            else {
                ProcessOutputLimitedViewDefaultCodegen(forge, classScope, method, instance);
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessView", classScope, method, instance);
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessJoin", classScope, method, instance);
        }

        private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            ResultSetProcessorRowPerEventForge forge,
            string methodName,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);

            if (forge.IsOutputAll) {
                instance.AddMember(NAME_OUTPUTALLUNORDHELPER, typeof(ResultSetProcessorRowPerEventOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLUNORDHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerEventOutputAll",
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT,
                        forge.OutputAllHelperSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTALLUNORDHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
            else if (forge.IsOutputLast) {
                instance.AddMember(NAME_OUTPUTLASTUNORDHELPER, typeof(ResultSetProcessorRowPerEventOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTUNORDHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSRowPerEventOutputLast",
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT,
                        forge.OutputLastHelperSettings.ToExpression()));
                method.Block.ExprDotMethod(
                    Member(NAME_OUTPUTLASTUNORDHELPER),
                    methodName,
                    REF_NEWDATA,
                    REF_OLDDATA,
                    REF_ISSYNTHESIZE);
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenMethod method)
        {
            if (forge.IsOutputAll) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLUNORDHELPER), "Output"));
            }
            else if (forge.IsOutputLast) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTUNORDHELPER), "Output"));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenMethod method)
        {
            if (forge.IsOutputAll) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTALLUNORDHELPER), "Output"));
            }
            else if (forge.IsOutputLast) {
                method.Block.MethodReturn(ExprDotMethod(Member(NAME_OUTPUTLASTUNORDHELPER), "Output"));
            }
            else {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        public static void StopCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTUNORDHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTUNORDHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLUNORDHELPER)) {
                method.Block.ExprDotMethod(Member(NAME_OUTPUTALLUNORDHELPER), "Destroy");
            }
        }

        private static void ProcessOutputLimitedJoinDefaultCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorUtil.PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block
                    .ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach
                    .DeclareVar(
                        typeof(ISet<MultiKeyArrayOfKeys<EventBean>>),
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar(
                        typeof(ISet<MultiKeyArrayOfKeys<EventBean>>),
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));
                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "Clear");
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGJOINRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    REF_OLDDATA);

                // generate old events using select expressions
                if (forge.IsSelectRStream) {
                    if (forge.OptionalHavingNode == null) {
                        if (!forge.IsSorting) {
                            forEach.StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_POPULATESELECTJOINEVENTSNOHAVING,
                                MEMBER_SELECTEXPRPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                        else {
                            forEach.StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_POPULATESELECTJOINEVENTSNOHAVINGWITHORDERBY,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                    else {
                        // generate old events using having then select
                        if (!forge.IsSorting) {
                            var select = PopulateSelectJoinEventsHavingCodegen(classScope, instance);
                            forEach.LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                        else {
                            var select = PopulateSelectJoinEventsHavingWithOrderByCodegen(classScope, instance);
                            forEach.LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                }

                // generate new events using select expressions
                if (forge.OptionalHavingNode == null) {
                    if (!forge.IsSorting) {
                        forEach.StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_POPULATESELECTJOINEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                    else {
                        forEach.StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_POPULATESELECTJOINEVENTSNOHAVINGWITHORDERBY,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            Ref("newEventsSortKey"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
                else {
                    if (!forge.IsSorting) {
                        var select = PopulateSelectJoinEventsHavingCodegen(classScope, instance);
                        forEach.LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                    else {
                        var select = PopulateSelectJoinEventsHavingWithOrderByCodegen(classScope, instance);
                        forEach.LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            Ref("newEventsSortKey"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
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

        private static void ProcessOutputLimitedJoinLastCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar<EventBean>("lastOldEvent", ConstantNull())
                .DeclareVar<EventBean>("lastNewEvent", ConstantNull());

            {
                var forEach = method.Block
                    .ForEach(typeof(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>), "pair", REF_JOINEVENTSSET);
                forEach
                    .DeclareVar(
                        typeof(ISet<MultiKeyArrayOfKeys<EventBean>>),
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar(
                        typeof(ISet<MultiKeyArrayOfKeys<EventBean>>),
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"));

                if (forge.IsUnidirectional) {
                    forEach.ExprDotMethod(Ref("this"), "Clear");
                }

                forEach.StaticMethod(
                    typeof(ResultSetProcessorUtil),
                    METHOD_APPLYAGGJOINRESULT,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    Ref("newData"),
                    Ref("oldData"));

                if (forge.IsSelectRStream) {
                    if (forge.OptionalHavingNode == null) {
                        forEach.DeclareVar<EventBean[]>(
                            "selectOldEvents",
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_GETSELECTJOINEVENTSNOHAVING,
                                MEMBER_SELECTEXPRPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                    else {
                        var select = GetSelectJoinEventsHavingCodegen(classScope, instance);
                        forEach.DeclareVar<EventBean[]>(
                            "selectOldEvents",
                            LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }

                    forEach.IfCondition(
                            And(
                                NotEqualsNull(Ref("selectOldEvents")),
                                Relational(
                                    ArrayLength(Ref("selectOldEvents")),
                                    CodegenExpressionRelational.CodegenRelational.GT,
                                    Constant(0))))
                        .AssignRef(
                            "lastOldEvent",
                            ArrayAtIndex(
                                Ref("selectOldEvents"),
                                Op(ArrayLength(Ref("selectOldEvents")), "-", Constant(1))))
                        .BlockEnd();
                }

                // generate new events using select expressions
                if (forge.OptionalHavingNode == null) {
                    forEach.DeclareVar<EventBean[]>(
                        "selectNewEvents",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTJOINEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    var select = GetSelectJoinEventsHavingCodegen(classScope, instance);
                    forEach.DeclareVar<EventBean[]>(
                        "selectNewEvents",
                        LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }

                forEach.IfCondition(
                        And(
                            NotEqualsNull(Ref("selectNewEvents")),
                            Relational(
                                ArrayLength(Ref("selectNewEvents")),
                                CodegenExpressionRelational.CodegenRelational.GT,
                                Constant(0))))
                    .AssignRef(
                        "lastNewEvent",
                        ArrayAtIndex(Ref("selectNewEvents"), Op(ArrayLength(Ref("selectNewEvents")), "-", Constant(1))))
                    .BlockEnd();
            }

            method.Block
                .DeclareVar<EventBean[]>(
                    "lastNew",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastNewEvent")))
                .DeclareVar<EventBean[]>(
                    "lastOld",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastOldEvent")))
                .IfCondition(And(EqualsNull(Ref("lastNew")), EqualsNull(Ref("lastOld"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean[]>), Ref("lastNew"), Ref("lastOld")));
        }

        private static void ProcessOutputLimitedViewDefaultCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorUtil.PrefixCodegenNewOldEvents(method.Block, forge.IsSorting, forge.IsSelectRStream);

            {
                var forEach = method.Block
                    .ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "First")))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        Cast(typeof(EventBean[]), ExprDotName(Ref("pair"), "Second")))
                    .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_APPLYAGGVIEWRESULT,
                        MEMBER_AGGREGATIONSVC,
                        MEMBER_EXPREVALCONTEXT,
                        REF_NEWDATA,
                        REF_OLDDATA,
                        Ref("eventsPerStream"));

                // generate old events using select expressions
                if (forge.IsSelectRStream) {
                    if (forge.OptionalHavingNode == null) {
                        if (!forge.IsSorting) {
                            forEach.StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_POPULATESELECTEVENTSNOHAVING,
                                MEMBER_SELECTEXPRPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                        else {
                            forEach.StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_POPULATESELECTEVENTSNOHAVINGWITHORDERBY,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                    }
                    else {
                        // generate old events using having then select
                        if (!forge.IsSorting) {
                            var select = PopulateSelectEventsHavingCodegen(classScope, instance);
                            forEach.LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                MEMBER_EXPREVALCONTEXT);
                        }
                        else {
                            var select = PopulateSelectEventsHavingWithOrderByCodegen(classScope, instance);
                            forEach.LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                MEMBER_ORDERBYPROCESSOR,
                                REF_OLDDATA,
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                Ref("oldEvents"),
                                Ref("oldEventsSortKey"),
                                MEMBER_EXPREVALCONTEXT);
                            throw new UnsupportedOperationException();
                        }
                    }
                }

                // generate new events using select expressions
                if (forge.OptionalHavingNode == null) {
                    if (!forge.IsSorting) {
                        forEach.StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_POPULATESELECTEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                    else {
                        forEach.StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_POPULATESELECTEVENTSNOHAVINGWITHORDERBY,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            Ref("newEventsSortKey"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
                else {
                    if (!forge.IsSorting) {
                        var select = PopulateSelectEventsHavingCodegen(classScope, instance);
                        forEach.LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                    else {
                        var select = PopulateSelectEventsHavingWithOrderByCodegen(classScope, instance);
                        forEach.LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            MEMBER_ORDERBYPROCESSOR,
                            REF_NEWDATA,
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            Ref("newEvents"),
                            Ref("newEventsSortKey"),
                            MEMBER_EXPREVALCONTEXT);
                    }
                }
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

        private static void ProcessOutputLimitedViewLastCodegen(
            ResultSetProcessorRowPerEventForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar<EventBean>("lastOldEvent", ConstantNull())
                .DeclareVar<EventBean>("lastNewEvent", ConstantNull())
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var forEach = method.Block
                    .ForEach(typeof(UniformPair<EventBean[]>), "pair", REF_VIEWEVENTSLIST);
                forEach.DeclareVar<EventBean[]>(
                        "newData",
                        ExprDotName(Ref("pair"), "First"))
                    .DeclareVar<EventBean[]>(
                        "oldData",
                        ExprDotName(Ref("pair"), "Second"))
                    .StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_APPLYAGGVIEWRESULT,
                        MEMBER_AGGREGATIONSVC,
                        MEMBER_EXPREVALCONTEXT,
                        Ref("newData"),
                        Ref("oldData"),
                        Ref("eventsPerStream"));

                if (forge.IsSelectRStream) {
                    if (forge.OptionalHavingNode == null) {
                        forEach.DeclareVar<EventBean[]>(
                            "selectOldEvents",
                            StaticMethod(
                                typeof(ResultSetProcessorUtil),
                                METHOD_GETSELECTEVENTSNOHAVING,
                                MEMBER_SELECTEXPRPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }
                    else {
                        var select = GetSelectEventsHavingCodegen(classScope, instance);
                        forEach.DeclareVar<EventBean[]>(
                            "selectOldEvents",
                            LocalMethod(
                                select,
                                MEMBER_SELECTEXPRPROCESSOR,
                                Ref("oldData"),
                                ConstantFalse(),
                                REF_ISSYNTHESIZE,
                                MEMBER_EXPREVALCONTEXT));
                    }

                    forEach.IfCondition(
                            And(
                                NotEqualsNull(Ref("selectOldEvents")),
                                Relational(
                                    ArrayLength(Ref("selectOldEvents")),
                                    CodegenExpressionRelational.CodegenRelational.GT,
                                    Constant(0))))
                        .AssignRef(
                            "lastOldEvent",
                            ArrayAtIndex(
                                Ref("selectOldEvents"),
                                Op(ArrayLength(Ref("selectOldEvents")), "-", Constant(1))))
                        .BlockEnd();
                }

                // generate new events using select expressions
                if (forge.OptionalHavingNode == null) {
                    forEach.DeclareVar<EventBean[]>(
                        "selectNewEvents",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    var select = GetSelectEventsHavingCodegen(classScope, instance);
                    forEach.DeclareVar<EventBean[]>(
                        "selectNewEvents",
                        LocalMethod(
                            select,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("newData"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE,
                            MEMBER_EXPREVALCONTEXT));
                }

                forEach.IfCondition(
                        And(
                            NotEqualsNull(Ref("selectNewEvents")),
                            Relational(
                                ArrayLength(Ref("selectNewEvents")),
                                CodegenExpressionRelational.CodegenRelational.GT,
                                Constant(0))))
                    .AssignRef(
                        "lastNewEvent",
                        ArrayAtIndex(Ref("selectNewEvents"), Op(ArrayLength(Ref("selectNewEvents")), "-", Constant(1))))
                    .BlockEnd();
            }

            method.Block
                .DeclareVar<EventBean[]>(
                    "lastNew",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastNewEvent")))
                .DeclareVar<EventBean[]>(
                    "lastOld",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYMAYNULL, Ref("lastOldEvent")))
                .IfCondition(And(EqualsNull(Ref("lastNew")), EqualsNull(Ref("lastOld"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(NewInstance(typeof(UniformPair<EventBean[]>), Ref("lastNew"), Ref("lastOld")));
        }

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTUNORDHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTLASTUNORDHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLUNORDHELPER)) {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Member(NAME_OUTPUTALLUNORDHELPER));
            }
        }
    }
} // end of namespace