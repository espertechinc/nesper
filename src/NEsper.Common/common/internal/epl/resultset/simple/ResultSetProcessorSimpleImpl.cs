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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.handthru;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.@event.core.EventBeanUtility;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
    /// <summary>
    /// Result set processor for the simplest case: no aggregation functions used in the select clause, and no group-by.
    /// <para />The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
    /// </summary>
    public class ResultSetProcessorSimpleImpl
    {
        private const string NAME_OUTPUTALLHELPER = "outputAllHelper";
        private const string NAME_OUTPUTLASTHELPER = "outputLastHelper";

        public static void ProcessJoinResultCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block
                .DeclareVar<EventBean[]>("selectOldEvents", ConstantNull())
                .DeclareVarNoInit(typeof(EventBean[]), "selectNewEvents");
            ResultSetProcessorUtil.ProcessJoinResultCodegen(
                method,
                classScope,
                instance,
                forge.OptionalHavingNode != null,
                forge.IsSelectRStream,
                forge.IsSorting,
                false);
        }

        public static void ProcessViewResultCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block
                .DeclareVar<EventBean[]>("selectOldEvents", ConstantNull())
                .DeclareVarNoInit(typeof(EventBean[]), "selectNewEvents");
            ResultSetProcessorUtil.ProcessViewResultCodegen(
                method,
                classScope,
                instance,
                forge.OptionalHavingNode != null,
                forge.IsSelectRStream,
                forge.IsSorting,
                false);
        }

        public static void GetEnumeratorViewCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsSorting) {
                // Return an iterator that gives row-by-row a result
                method.Block.MethodReturn(
                    NewInstance(
                        typeof(TransformEventEnumerator),
                        ExprDotMethod(REF_VIEWABLE, "GetEnumerator"),
                        NewInstance(typeof(ResultSetProcessorHandtruTransform), Ref("this"))));
                return;
            }

            // Pull all events, generate order keys
            method.Block.DeclareVar<EventBean[]>(
                    "eventsPerStream",
                    NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar<IList<EventBean>>("events", NewInstance(typeof(List<EventBean>)))
                .DeclareVar<IList<object>>("orderKeys", NewInstance(typeof(List<object>)))
                .DeclareVar<IEnumerator<EventBean>>("parentIterator", ExprDotMethod(REF_VIEWABLE, "GetEnumerator"))
                .IfCondition(EqualsNull(Ref("parentIterator")))
                .BlockReturn(PublicConstValue(typeof(CollectionUtil), "NULL_EVENT_ITERATOR"));

            {
                var loop = method.Block.ForEach<EventBean>("aParent", REF_VIEWABLE);
                loop.AssignArrayElement("eventsPerStream", Constant(0), Ref("aParent"))
                    .DeclareVar<object>(
                        "orderKey",
                        ExprDotMethod(
                            MEMBER_ORDERBYPROCESSOR,
                            "GetSortKey",
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));

                if (forge.OptionalHavingNode == null) {
                    loop.DeclareVar<EventBean[]>(
                        "result",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            METHOD_GETSELECTEVENTSNOHAVING,
                            MEMBER_SELECTEXPRPROCESSOR,
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }
                else {
                    var select = GetSelectEventsHavingCodegen(classScope, instance);
                    loop.DeclareVar<EventBean[]>(
                        "result",
                        LocalMethod(
                            select,
                            MEMBER_SELECTEXPRNONMEMBER,
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            ConstantTrue(),
                            MEMBER_EXPREVALCONTEXT));
                }

                loop.IfCondition(
                        And(NotEqualsNull(Ref("result")), Not(EqualsIdentity(ArrayLength(Ref("result")), Constant(0)))))
                    .ExprDotMethod(Ref("events"), "Add", ArrayAtIndex(Ref("result"), Constant(0)))
                    .ExprDotMethod(Ref("orderKeys"), "Add", Ref("orderKey"));
            }

            method.Block.DeclareVar<EventBean[]>(
                    "outgoingEvents",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYEVENTS, Ref("events")))
                .DeclareVar<object[]>(
                    "orderKeysArr",
                    StaticMethod(typeof(CollectionUtil), METHOD_TOARRAYOBJECTS, Ref("orderKeys")))
                .DeclareVar<EventBean[]>(
                    "orderedEvents",
                    ExprDotMethod(
                        MEMBER_ORDERBYPROCESSOR,
                        "SortWOrderKeys",
                        Ref("outgoingEvents"),
                        Ref("orderKeysArr"),
                        MEMBER_EXPREVALCONTEXT))
                .MethodReturn(
                    ExprDotMethod(
                        StaticMethod(typeof(Arrays), "Enumerate", Ref("orderedEvents")),
                        "GetEnumerator"));
        }

        public static void GetEnumeratorJoinCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar<UniformPair<EventBean[]>>(
                    "result",
                    ExprDotMethod(
                        Ref("this"),
                        "ProcessJoinResult",
                        REF_JOINSET,
                        StaticMethod(typeof(Collections), "GetEmptySet", new[] {typeof(MultiKeyArrayOfKeys<EventBean>)}),
                        ConstantTrue()))
                .IfRefNull("result")
                .BlockReturn(
                    StaticMethod(typeof(Collections), "GetEmptyEnumerator", new[] {typeof(EventBean)}))
                .MethodReturn(
                    ExprDotMethod(
                        StaticMethod(typeof(Arrays), "Enumerate", GetProperty(Ref("result"), "First")),
                        "GetEnumerator"));
        }

        public static void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessView", classScope, method, instance);
        }

        private static void ProcessOutputLimitedLastAllNonBufferedCodegen(
            ResultSetProcessorSimpleForge forge,
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
            if (forge.IsOutputAll) {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorSimpleOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSSimpleOutputAll",
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT,
                        eventTypes,
                        forge.OutputAllHelperSettings.ToExpression()));
                method.Block.ExprDotMethod(Member(NAME_OUTPUTALLHELPER), methodName, REF_NEWDATA, REF_OLDDATA);
            }
            else if (forge.IsOutputLast) {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorSimpleOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory,
                        "MakeRSSimpleOutputLast",
                        Ref("this"),
                        MEMBER_EXPREVALCONTEXT,
                        eventTypes,
                        forge.OutputLastHelperSettings.ToExpression()));
                method.Block.ExprDotMethod(Member(NAME_OUTPUTLASTHELPER), methodName, REF_NEWDATA, REF_OLDDATA);
            }
        }

        public static void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ProcessOutputLimitedLastAllNonBufferedCodegen(forge, "ProcessJoin", classScope, method, instance);
        }

        public static void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
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
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
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

        public static void StopMethodCodegen(
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

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenMethod method)
        {
            if (!forge.IsOutputLast) {
                method.Block
                    .DeclareVar<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>>(
                        "pair",
                        StaticMethod(typeof(EventBeanUtility), METHOD_FLATTENBATCHJOIN, REF_JOINEVENTSSET))
                    .MethodReturn(
                        ExprDotMethod(
                            Ref("this"),
                            "ProcessJoinResult",
                            ExprDotName(Ref("pair"), "First"),
                            ExprDotName(Ref("pair"), "Second"),
                            REF_ISSYNTHESIZE));
                return;
            }

            method.Block.MethodThrowUnsupported();
        }

        public static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenMethod method)
        {
            if (!forge.IsOutputLast) {
                method.Block.DeclareVar<UniformPair<EventBean[]>>(
                        "pair",
                        StaticMethod(typeof(EventBeanUtility), METHOD_FLATTENBATCHSTREAM, REF_VIEWEVENTSLIST))
                    .MethodReturn(
                        ExprDotMethod(
                            Ref("this"),
                            "ProcessViewResult",
                            ExprDotName(Ref("pair"), "First"),
                            ExprDotName(Ref("pair"), "Second"),
                            REF_ISSYNTHESIZE));
                return;
            }

            method.Block.MethodThrowUnsupported();
        }
    }
} // end of namespace