///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.timer;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// This view is hooked into a named window's view chain as the last view and handles 
    /// dispatching of named window insert and remove stream results via 
    /// <seealso cref="named.NamedWindowMgmtService" /> to consuming statements. 
    /// </summary>
    public class NamedWindowTailView
    {
        private volatile IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> _consumersNonContext;  // handles as copy-on-write

        public NamedWindowTailView(
            EventType eventType,
            NamedWindowMgmtService namedWindowMgmtService,
            NamedWindowDispatchService namedWindowDispatchService,
            StatementResultService statementResultService,
            ValueAddEventProcessor revisionProcessor,
            bool prioritized,
            bool parentBatchWindow,
            TimeSourceService timeSourceService,
            ConfigurationEngineDefaults.ThreadingConfig threadingConfig)
        {
            EventType = eventType;
            NamedWindowMgmtService = namedWindowMgmtService;
            NamedWindowDispatchService = namedWindowDispatchService;
            StatementResultService = statementResultService;
            RevisionProcessor = revisionProcessor;
            IsPrioritized = prioritized;
            IsParentBatchWindow = parentBatchWindow;
            _consumersNonContext = NamedWindowUtil.CreateConsumerMap(IsPrioritized);
            ThreadingConfig = threadingConfig;
            TimeSourceService = timeSourceService;
        }

        /// <summary>Returns true to indicate that the data window view is a batch view. </summary>
        /// <value>true if batch view</value>
        public bool IsParentBatchWindow { get; protected internal set; }

        public EventType EventType { get; protected internal set; }

        public StatementResultService StatementResultService { get; protected internal set; }

        public NamedWindowMgmtService NamedWindowMgmtService { get; protected internal set; }

        public NamedWindowDispatchService NamedWindowDispatchService { get; protected internal set; }

        public bool IsPrioritized { get; protected internal set; }

        public ValueAddEventProcessor RevisionProcessor { get; protected internal set; }

        public IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> ConsumersNonContext
        {
            get => _consumersNonContext;
            protected internal set { _consumersNonContext = value; }
        }

        public TimeSourceService TimeSourceService { get; protected internal set; }

        public ConfigurationEngineDefaults.ThreadingConfig ThreadingConfig { get; protected internal set; }

        public NamedWindowConsumerView AddConsumer(NamedWindowConsumerDesc consumerDesc)
        {
            NamedWindowConsumerCallback consumerCallback = new ProxyNamedWindowConsumerCallback(
                () =>
                {
                    throw new UnsupportedOperationException(
                        "GetEnumerator not supported on named windows that have a context attached and when that context is not the same as the consuming statement's context");
                },
                RemoveConsumer);

            // Construct consumer view, allow a callback to this view to remove the consumer
            var audit = AuditEnum.STREAM.GetAudit(consumerDesc.AgentInstanceContext.StatementContext.Annotations) != null;
            var consumerView = new NamedWindowConsumerView(
                ExprNodeUtility.GetEvaluators(consumerDesc.FilterList),
                consumerDesc.OptPropertyEvaluator,
                EventType,
                consumerCallback,
                consumerDesc.AgentInstanceContext,
                audit);

            // Keep a list of consumer views per statement to accomodate joins and subqueries
            IList<NamedWindowConsumerView> viewsPerStatements = _consumersNonContext.Get(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle);
            if (viewsPerStatements == null)
            {
                viewsPerStatements = new CopyOnWriteList<NamedWindowConsumerView>();

                // avoid concurrent modification as a thread may currently iterate over consumers as its dispatching
                // without the engine lock
                var newConsumers = NamedWindowUtil.CreateConsumerMap(IsPrioritized);
                newConsumers.PutAll(_consumersNonContext);
                newConsumers.Put(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle, viewsPerStatements);
                _consumersNonContext = newConsumers;
            }
            viewsPerStatements.Add(consumerView);

            return consumerView;
        }

        /// <summary>Called by the consumer view to indicate it was stopped or destroyed, such that the consumer can be deregistered and further dispatches disregard this consumer. </summary>
        /// <param name="namedWindowConsumerView">is the consumer representative view</param>
        public void RemoveConsumer(NamedWindowConsumerView namedWindowConsumerView)
        {
            EPStatementAgentInstanceHandle handleRemoved = null;
            // Find the consumer view
            foreach (var entry in _consumersNonContext)
            {
                var foundAndRemoved = entry.Value.Remove(namedWindowConsumerView);
                // Remove the consumer view
                if ((foundAndRemoved) && (entry.Value.Count == 0))
                {
                    // Remove the handle if this list is now empty
                    handleRemoved = entry.Key;
                    break;
                }
            }
            if (handleRemoved != null)
            {
                var newConsumers = NamedWindowUtil.CreateConsumerMap(IsPrioritized);
                newConsumers.PutAll(_consumersNonContext);
                newConsumers.Remove(handleRemoved);
                _consumersNonContext = newConsumers;
            }
        }

        public void AddDispatches(
            NamedWindowConsumerLatchFactory latchFactory,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumersInContext,
            NamedWindowDeltaData delta,
            AgentInstanceContext agentInstanceContext)
        {
            if (!consumersInContext.IsEmpty())
            {
                NamedWindowDispatchService.AddDispatch(latchFactory, delta, consumersInContext);
            }
            if (!_consumersNonContext.IsEmpty())
            {
                NamedWindowDispatchService.AddDispatch(latchFactory, delta, _consumersNonContext);
            }
        }

        public NamedWindowConsumerLatchFactory MakeLatchFactory()
        {
            return new NamedWindowConsumerLatchFactory(
                EventType.Name,
                ThreadingConfig.IsNamedWindowConsumerDispatchPreserveOrder,
                ThreadingConfig.NamedWindowConsumerDispatchTimeout,
                ThreadingConfig.NamedWindowConsumerDispatchLocking,
                TimeSourceService, true
                );
        }
    }
}
