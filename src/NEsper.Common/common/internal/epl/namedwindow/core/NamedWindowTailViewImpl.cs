///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    /// This view is hooked into a named window's view chain as the last view and handles dispatching of named window
    /// insert and remove stream results via <seealso cref="NamedWindowManagementService" /> to consuming statements.
    /// </summary>
    public class NamedWindowTailViewImpl : NamedWindowTailViewBase
    {
        private volatile IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>
            consumersNonContext; // handles as copy-on-write

        public NamedWindowTailViewImpl(
            EventType eventType,
            bool isParentBatchWindow,
            EPStatementInitServices services)
            : base(eventType, isParentBatchWindow, services)
        {
            this.consumersNonContext = NamedWindowUtil.CreateConsumerMap(isPrioritized);
        }

        public IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> GetConsumersNonContext()
        {
            return consumersNonContext;
        }

        public override NamedWindowConsumerView AddConsumerNoContext(NamedWindowConsumerDesc consumerDesc)
        {
            NamedWindowConsumerCallback consumerCallback = new ProxyNamedWindowConsumerCallback() {
                ProcGetEnumerator = () => throw new UnsupportedOperationException(
                    "Iterator not supported on named windows that have a context attached and when that context is not the same as the consuming statement's context"),
                ProcIsParentBatchWindow = () => isParentBatchWindow,
                ProcSnapshot = (
                    queryGraph,
                    annotations) => Collections.GetEmptyList<EventBean>(),
                ProcStopped = RemoveConsumerNoContext,
            };

            // Construct consumer view, allow a callback to this view to remove the consumer
            bool audit = AuditEnum.STREAM.GetAudit(consumerDesc.AgentInstanceContext.StatementContext.Annotations) !=
                         null;
            NamedWindowConsumerView consumerView = new NamedWindowConsumerView(
                consumerDesc.NamedWindowConsumerId,
                consumerDesc.FilterEvaluator,
                consumerDesc.OptPropertyEvaluator,
                eventType,
                consumerCallback,
                consumerDesc.AgentInstanceContext,
                audit);

            // Keep a list of consumer views per statement to accomodate joins and subqueries
            IList<NamedWindowConsumerView> viewsPerStatements =
                consumersNonContext.Get(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle);
            if (viewsPerStatements == null) {
                viewsPerStatements = new CopyOnWriteList<NamedWindowConsumerView>();

                // avoid concurrent modification as a thread may currently iterate over consumers as its dispatching
                // without the runtime lock
                IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> newConsumers =
                    NamedWindowUtil.CreateConsumerMap(isPrioritized);
                newConsumers.PutAll(consumersNonContext);
                newConsumers.Put(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle, viewsPerStatements);
                consumersNonContext = newConsumers;
            }

            viewsPerStatements.Add(consumerView);

            return consumerView;
        }

        public override void RemoveConsumerNoContext(NamedWindowConsumerView namedWindowConsumerView)
        {
            EPStatementAgentInstanceHandle handleRemoved = null;
            // Find the consumer view
            foreach (KeyValuePair<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> entry in
                consumersNonContext) {
                bool foundAndRemoved = entry.Value.Remove(namedWindowConsumerView);
                // Remove the consumer view
                if (foundAndRemoved && (entry.Value.Count == 0)) {
                    // Remove the handle if this list is now empty
                    handleRemoved = entry.Key;
                    break;
                }
            }

            if (handleRemoved != null) {
                IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> newConsumers =
                    NamedWindowUtil.CreateConsumerMap(isPrioritized);
                newConsumers.PutAll(consumersNonContext);
                newConsumers.Remove(handleRemoved);
                consumersNonContext = newConsumers;
            }
        }

        public override void AddDispatches(
            NamedWindowConsumerLatchFactory latchFactory,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumersInContext,
            NamedWindowDeltaData delta,
            AgentInstanceContext agentInstanceContext)
        {
            if (!consumersInContext.IsEmpty()) {
                namedWindowDispatchService.AddDispatch(latchFactory, delta, consumersInContext);
            }

            if (!consumersNonContext.IsEmpty()) {
                namedWindowDispatchService.AddDispatch(latchFactory, delta, consumersNonContext);
            }
        }

        public override NamedWindowConsumerLatchFactory MakeLatchFactory()
        {
            return new NamedWindowConsumerLatchFactory(
                eventType.Name,
                threadingConfig.IsNamedWindowConsumerDispatchPreserveOrder,
                threadingConfig.NamedWindowConsumerDispatchTimeout,
                threadingConfig.NamedWindowConsumerDispatchLocking,
                timeSourceService,
                true);
        }
    }
} // end of namespace