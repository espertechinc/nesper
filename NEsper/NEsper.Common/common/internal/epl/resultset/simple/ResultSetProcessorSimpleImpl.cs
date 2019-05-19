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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.handthru;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
    /// <summary>
    ///     Result set processor for the simplest case: no aggregation functions used in the select clause, and no group-by.
    ///     <para />
    ///     The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
    /// </summary>
    public class ResultSetProcessorSimpleImpl
    {
        private const string NAME_OUTPUTALLHELPER = "OutputAllHelper";
        private const string NAME_OUTPUTLASTHELPER = "OutputLastHelper";

        public static void ProcessJoinResultCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar(typeof(EventBean[]), "SelectOldEvents", ConstantNull())
                .DeclareVarNoInit(typeof(EventBean[]), "SelectNewEvents");
            ResultSetProcessorUtil.ProcessJoinResultCodegen(
                method, classScope, instance,
                forge.OptionalHavingNode != null, forge.IsSelectRStream,
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
                .DeclareVar(typeof(EventBean[]), "SelectOldEvents", ConstantNull())
                .DeclareVarNoInit(typeof(EventBean[]), "SelectNewEvents");
            ResultSetProcessorUtil.ProcessViewResultCodegen(
                method, classScope, instance, forge.OptionalHavingNode != null, forge.IsSelectRStream, forge.IsSorting,
                false);
        }

        public static void GetIteratorViewCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsSorting)
            {
                // Return an iterator that gives row-by-row a result
                method.Block.MethodReturn(
                    NewInstance(
                        typeof(TransformEventIterator), ExprDotMethod(REF_VIEWABLE, "iterator"),
                        NewInstance(typeof(ResultSetProcessorHandtruTransform), Ref("this"))
                        ));
                return;
            }

            // Pull all events, generate order keys
            method.Block
                .DeclareVar(typeof(EventBean[]), "eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar(typeof(IList<object>), "events", NewInstance(typeof(List<object>)))
                .DeclareVar(typeof(IList<object>), "orderKeys", NewInstance(typeof(List<object>)))
                .DeclareVar(typeof(IEnumerator<object>), "parentIterator", ExprDotMethod(REF_VIEWABLE, "iterator"))
                .IfCondition(EqualsNull(Ref("parentIterator")))
                .BlockReturn(PublicConstValue(typeof(CollectionUtil), "NULL_EVENT_ITERATOR"));

            {
                var loop = method.Block.ForEach(typeof(EventBean), "aParent", REF_VIEWABLE);
                loop.AssignArrayElement("eventsPerStream", Constant(0), Ref("aParent"))
                    .DeclareVar(
                        typeof(object), "orderKey",
                        ExprDotMethod(
                            REF_ORDERBYPROCESSOR, "getSortKey", Ref("eventsPerStream"), ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT));

                if (forge.OptionalHavingNode == null)
                {
                    loop.DeclareVar(
                        typeof(EventBean[]), "result",
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            ResultSetProcessorUtil.METHOD_GETSELECTEVENTSNOHAVING,
                            REF_SELECTEXPRPROCESSOR,
                            Ref("eventsPerStream"),
                            ConstantTrue(),
                            ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT));
                }
                else
                {
                    var select = ResultSetProcessorUtil.GetSelectEventsHavingCodegen(classScope, instance);
                    loop.DeclareVar(
                        typeof(EventBean[]), "result",
                        LocalMethod(
                            select, REF_SELECTEXPRNONMEMBER, Ref("eventsPerStream"), ConstantTrue(), ConstantTrue(),
                            REF_AGENTINSTANCECONTEXT));
                }

                loop.IfCondition(
                        And(NotEqualsNull(Ref("result")), Not(EqualsIdentity(ArrayLength(Ref("result")), Constant(0)))))
                    .ExprDotMethod(Ref("events"), "add", ArrayAtIndex(Ref("result"), Constant(0)))
                    .ExprDotMethod(Ref("orderKeys"), "add", Ref("orderKey"));
            }

            method.Block.DeclareVar(
                    typeof(EventBean[]), "outgoingEvents",
                    StaticMethod(typeof(CollectionUtil), CollectionUtil.METHOD_TOARRAYEVENTS, Ref("events")))
                .DeclareVar(
                    typeof(object[]), "orderKeysArr",
                    StaticMethod(typeof(CollectionUtil), CollectionUtil.METHOD_TOARRAYOBJECTS, Ref("orderKeys")))
                .DeclareVar(
                    typeof(EventBean[]), "orderedEvents",
                    ExprDotMethod(
                        REF_ORDERBYPROCESSOR, "SortWOrderKeys", Ref("outgoingEvents"), Ref("orderKeysArr"),
                        REF_AGENTINSTANCECONTEXT))
                .MethodReturn(
                    StaticMethod(
                        typeof(ArrayHelper), "Iterate",
                        Ref("orderedEvents")));
        }

        public static void GetIteratorJoinCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            method.Block.DeclareVar(
                    typeof(UniformPair<EventBean>), "result",
                    ExprDotMethod(
                        Ref("this"), "ProcessJoinResult",
                        REF_JOINSET,
                        StaticMethod(typeof(Collections), "emptySet"),
                        ConstantTrue()))
                .IfRefNull("result")
                .BlockReturn(StaticMethod(typeof(Collections), "emptyIterator"))
                .MethodReturn(
                    StaticMethod(
                        typeof(ArrayHelper), "Iterate",
                        Cast(typeof(EventBean[]), GetProperty(Ref("result"), "First"))));
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
            var factory = classScope.AddOrGetFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            CodegenExpression eventTypes = classScope.AddFieldUnshared(
                true, typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            if (forge.IsOutputAll)
            {
                instance.AddMember(NAME_OUTPUTALLHELPER, typeof(ResultSetProcessorSimpleOutputAllHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTALLHELPER,
                    ExprDotMethod(factory, "MakeRSSimpleOutputAll", Ref("this"), REF_AGENTINSTANCECONTEXT, eventTypes));
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), methodName, REF_NEWDATA, REF_OLDDATA);
            }
            else if (forge.IsOutputLast)
            {
                instance.AddMember(NAME_OUTPUTLASTHELPER, typeof(ResultSetProcessorSimpleOutputLastHelper));
                instance.ServiceCtor.Block.AssignRef(
                    NAME_OUTPUTLASTHELPER,
                    ExprDotMethod(
                        factory, "MakeRSSimpleOutputLast", Ref("this"), REF_AGENTINSTANCECONTEXT, eventTypes));
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), methodName, REF_NEWDATA, REF_OLDDATA);
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
            if (forge.IsOutputAll)
            {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
            else if (forge.IsOutputLast)
            {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "OutputView", REF_ISSYNTHESIZE));
            }
            else
            {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        public static void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (forge.IsOutputAll)
            {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
            else if (forge.IsOutputLast)
            {
                method.Block.MethodReturn(ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "OutputJoin", REF_ISSYNTHESIZE));
            }
            else
            {
                method.Block.MethodReturn(ConstantNull());
            }
        }

        public static void StopMethodCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER))
            {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTLASTHELPER), "Destroy");
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER))
            {
                method.Block.ExprDotMethod(Ref(NAME_OUTPUTALLHELPER), "Destroy");
            }
        }

        public static void AcceptHelperVisitorCodegen(
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (instance.HasMember(NAME_OUTPUTLASTHELPER))
            {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Ref(NAME_OUTPUTLASTHELPER));
            }

            if (instance.HasMember(NAME_OUTPUTALLHELPER))
            {
                method.Block.ExprDotMethod(REF_RESULTSETVISITOR, "Visit", Ref(NAME_OUTPUTALLHELPER));
            }
        }

        public static void ProcessOutputLimitedJoinCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenMethod method)
        {
            if (!forge.IsOutputLast)
            {
                method.Block.DeclareVar(
                        typeof(UniformPair<EventBean>), "pair",
                        StaticMethod(typeof(EventBeanUtility), EventBeanUtility.METHOD_FLATTENBATCHJOIN, REF_JOINEVENTSSET))
                    .MethodReturn(
                        ExprDotMethod(
                            Ref("this"), "ProcessJoinResult",
                            Cast(typeof(ISet<EventBean>), GetProperty(Ref("pair"), "First")),
                            Cast(typeof(ISet<EventBean>), GetProperty(Ref("pair"), "Second")),
                            REF_ISSYNTHESIZE));
                return;
            }

            method.Block.MethodThrowUnsupported();
        }

        public static void ProcessOutputLimitedViewCodegen(
            ResultSetProcessorSimpleForge forge,
            CodegenMethod method)
        {
            if (!forge.IsOutputLast)
            {
                method.Block.DeclareVar(
                        typeof(UniformPair<EventBean>), "pair",
                        StaticMethod(typeof(EventBeanUtility), EventBeanUtility.METHOD_FLATTENBATCHSTREAM, REF_VIEWEVENTSLIST))
                    .MethodReturn(
                        ExprDotMethod(
                            Ref("this"), "ProcessViewResult",
                            Cast(typeof(EventBean[]), GetProperty(Ref("pair"), "First")),
                            Cast(typeof(EventBean[]), GetProperty(Ref("pair"), "Second")),
                            REF_ISSYNTHESIZE));
                return;
            }

            method.Block.MethodThrowUnsupported();
        }
    }
} // end of namespace