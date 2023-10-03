///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public abstract class FAFQueryMethodIUDBase : FAFQueryMethod,
        FAFQueryMethodSessionPrepared,
        FAFQuerySessionUnprepared
    {
        private string contextName;
        private FireAndForgetProcessor processor;
        private InternalEventRouteDest internalEventRouteDest;
        protected QueryGraph queryGraph;
        private Attribute[] annotations;
        private IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses;
        private bool hasTableAccess;
        private IDictionary<int, SubSelectFactory> subselects;
        private IReaderWriterLock eventProcessingRWLock;

        public string ContextName {
            set => contextName = value;
        }

        public FireAndForgetProcessor Processor {
            set => processor = value;
        }

        public InternalEventRouteDest InternalEventRouteDest {
            set => internalEventRouteDest = value;
        }

        public virtual QueryGraph QueryGraph {
            get => queryGraph;
            set => queryGraph = value;
        }

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses {
            set => tableAccesses = value;
        }

        public bool HasTableAccess {
            set => hasTableAccess = value;
        }

        public IDictionary<int, SubSelectFactory> Subselects {
            get => subselects;
            set => subselects = value;
        }

        protected abstract EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance);

        public FAFQuerySessionUnprepared ReadyUnprepared(StatementContextRuntimeServices services)
        {
            Ready(services);
            return this;
        }

        public FAFQueryMethodSessionPrepared ReadyPrepared(StatementContextRuntimeServices services)
        {
            Ready(services);
            return this;
        }

        public FAFQueryMethodSessionPrepared Prepared()
        {
            return this;
        }

        public FAFQuerySessionUnprepared Unprepared()
        {
            return this;
        }

        public void Init()
        {
            // no action required
        }

        public void Close()
        {
            // no action required
        }

        public EPPreparedQueryResult Execute(
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextPartitionSelector[] contextPartitionSelectors,
            ContextManagementService contextManagementService)
        {
            if (!serviceStatusProvider.Get()) {
                throw RuntimeDestroyed();
            }

            try {
                using (eventProcessingRWLock.AcquireReadLock()) {
                    if (contextPartitionSelectors != null && contextPartitionSelectors.Length != 1) {
                        throw new ArgumentException("Number of context partition selectors must be one");
                    }

                    var optionalSingleSelector =
                        contextPartitionSelectors != null && contextPartitionSelectors.Length > 0
                            ? contextPartitionSelectors[0]
                            : null;

                    // validate context
                    if (processor.ContextName != null &&
                        contextName != null &&
                        !processor.ContextName.Equals(contextName)) {
                        throw new EPException(
                            "Context for named window is '" +
                            processor.ContextName +
                            "' and query specifies context '" +
                            contextName +
                            "'");
                    }

                    // handle non-specified context
                    if (contextName == null) {
                        var processorInstance = processor.ProcessorInstanceNoContext;
                        if (processorInstance != null) {
                            Assign(processorInstance.AgentInstanceContext, assignerSetter);
                            var rows = Execute(processorInstance);
                            if (rows != null && rows.Length > 0) {
                                Dispatch();
                            }

                            return new EPPreparedQueryResult(processor.EventTypePublic, rows);
                        }
                    }

                    // context partition runtime query
                    var agentInstanceIds = AgentInstanceIds(
                        processor,
                        optionalSingleSelector,
                        contextManagementService);

                    // collect events and agent instances
                    if (agentInstanceIds.IsEmpty()) {
                        return new EPPreparedQueryResult(
                            processor.EventTypeResultSetProcessor,
                            CollectionUtil.EVENTBEANARRAY_EMPTY);
                    }

                    if (agentInstanceIds.Count == 1) {
                        var agentInstanceId = agentInstanceIds.First();
                        var processorInstance =
                            processor.GetProcessorInstanceContextById(agentInstanceId);
                        Assign(processorInstance.AgentInstanceContext, assignerSetter);
                        var rows = Execute(processorInstance);
                        if (rows.Length > 0) {
                            Dispatch();
                        }

                        return new EPPreparedQueryResult(processor.EventTypeResultSetProcessor, rows);
                    }

                    var allRows = new ArrayDeque<EventBean>();
                    foreach (var agentInstanceId in agentInstanceIds) {
                        var processorInstance =
                            processor.GetProcessorInstanceContextById(agentInstanceId);
                        if (processorInstance != null) {
                            Assign(processorInstance.AgentInstanceContext, assignerSetter);
                            var rows = Execute(processorInstance);
                            allRows.AddAll(Arrays.AsList(rows));
                        }
                    }

                    if (allRows.Count > 0) {
                        Dispatch();
                    }

                    return new EPPreparedQueryResult(processor.EventTypeResultSetProcessor, allRows.ToArray());
                }
            }
            finally {
                if (hasTableAccess) {
                    processor.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }

        /// <summary>
        /// Returns the event type of the prepared statement.
        /// </summary>
        /// <value>event type</value>
        public EventType EventType => processor.EventTypeResultSetProcessor;

        public Attribute[] Annotations {
            get => annotations;
            set => annotations = value;
        }

        protected void Dispatch()
        {
            internalEventRouteDest.ProcessThreadWorkQueue();
        }

        private void Assign(
            AgentInstanceContext agentInstanceContext,
            FAFQueryMethodAssignerSetter assignerSetter)
        {
            // start table-access
            var tableAccessEvals =
                ExprTableEvalHelperStart.StartTableAccess(tableAccesses, agentInstanceContext);

            // start subselects
            IList<AgentInstanceMgmtCallback> subselectStopCallbacks = new List<AgentInstanceMgmtCallback>(2);
            var subselectActivations = SubSelectHelperStart.StartSubselects(
                subselects,
                agentInstanceContext,
                agentInstanceContext,
                subselectStopCallbacks,
                false);

            // assign
            assignerSetter.Assign(
                new StatementAIFactoryAssignmentsImpl(null, null, null, subselectActivations, tableAccessEvals, null));
        }

        private void Ready(StatementContextRuntimeServices services)
        {
            if (!subselects.IsEmpty()) {
                InitializeSubselects(services, annotations, subselects);
            }

            eventProcessingRWLock = services.EventProcessingRWLock;
        }
    }
} // end of namespace