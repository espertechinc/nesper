///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public abstract class FAFQueryMethodIUDBase : FAFQueryMethod
    {
        private string _contextName;
        private bool _hasTableAccess;
        private InternalEventRouteDest _internalEventRouteDest;
        private FireAndForgetProcessor _processor;
        private IDictionary<int, ExprTableEvalStrategyFactory> _tableAccesses;
        private QueryGraph _queryGraph;

        public string ContextName {
            get => throw new NotImplementedException();
            set => _contextName = value;
        }

        public FireAndForgetProcessor Processor {
            get => throw new NotImplementedException();
            set => _processor = value;
        }

        public InternalEventRouteDest InternalEventRouteDest {
            get => throw new NotImplementedException();
            set => _internalEventRouteDest = value;
        }

        public virtual QueryGraph QueryGraph {
            get => throw new NotImplementedException();
            set => _queryGraph = value;
        }

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses {
            get => throw new NotImplementedException();
            set => _tableAccesses = value;
        }

        public bool HasTableAccess {
            get => throw new NotImplementedException();
            set => _hasTableAccess = value;
        }

        public Attribute[] Annotations { get; set; }

        public void Ready()
        {
            // no action
        }

        public EPPreparedQueryResult Execute(
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextPartitionSelector[] contextPartitionSelectors,
            ContextManagementService contextManagementService)
        {
            if (!serviceStatusProvider.Get()) {
                throw FAFQueryMethodUtil.RuntimeDestroyed();
            }

            try {
                if (contextPartitionSelectors != null && contextPartitionSelectors.Length != 1) {
                    throw new ArgumentException("Number of context partition selectors must be one");
                }

                var optionalSingleSelector = contextPartitionSelectors != null && contextPartitionSelectors.Length > 0
                    ? contextPartitionSelectors[0]
                    : null;

                // validate context
                if (_processor.ContextName != null &&
                    _contextName != null &&
                    !_processor.ContextName.Equals(_contextName)) {
                    throw new EPException(
                        "Context for named window is '" +
                        _processor.ContextName +
                        "' and query specifies context '" +
                        _contextName +
                        "'");
                }

                // handle non-specified context
                if (_contextName == null) {
                    var processorInstance = _processor.ProcessorInstanceNoContext;
                    if (processorInstance != null) {
                        Assign(processorInstance.AgentInstanceContext, assignerSetter);
                        var rows = Execute(processorInstance);
                        if (rows != null && rows.Length > 0) {
                            Dispatch();
                        }

                        return new EPPreparedQueryResult(_processor.EventTypePublic, rows);
                    }
                }

                // context partition runtime query
                var agentInstanceIds = FAFQueryMethodUtil.AgentInstanceIds(
                    _processor,
                    optionalSingleSelector,
                    contextManagementService);

                // collect events and agent instances
                if (agentInstanceIds.IsEmpty()) {
                    return new EPPreparedQueryResult(
                        _processor.EventTypeResultSetProcessor,
                        CollectionUtil.EVENTBEANARRAY_EMPTY);
                }

                if (agentInstanceIds.Count == 1) {
                    var agentInstanceId = agentInstanceIds.First();
                    var processorInstance = _processor.GetProcessorInstanceContextById(agentInstanceId);
                    Assign(processorInstance.AgentInstanceContext, assignerSetter);
                    var rows = Execute(processorInstance);
                    if (rows.Length > 0) {
                        Dispatch();
                    }

                    return new EPPreparedQueryResult(_processor.EventTypeResultSetProcessor, rows);
                }

                var allRows = new ArrayDeque<EventBean>();
                foreach (var agentInstanceId in agentInstanceIds) {
                    var processorInstance = _processor.GetProcessorInstanceContextById(agentInstanceId);
                    if (processorInstance != null) {
                        Assign(processorInstance.AgentInstanceContext, assignerSetter);
                        var rows = Execute(processorInstance);
                        allRows.AddAll(rows);
                    }
                }

                if (allRows.Count > 0) {
                    Dispatch();
                }

                return new EPPreparedQueryResult(_processor.EventTypeResultSetProcessor, allRows.ToArray());
            }
            finally {
                if (_hasTableAccess) {
                    _processor.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }

        /// <summary>
        ///     Returns the event type of the prepared statement.
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType => _processor.EventTypeResultSetProcessor;

        protected abstract EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance);

        protected void Dispatch()
        {
            _internalEventRouteDest.ProcessThreadWorkQueue();
        }

        private void Assign(
            AgentInstanceContext agentInstanceContext,
            FAFQueryMethodAssignerSetter assignerSetter)
        {
            // start table-access
            var tableAccessEvals = ExprTableEvalHelperStart.StartTableAccess(_tableAccesses, agentInstanceContext);

            // assign
            assignerSetter.Assign(
                new StatementAIFactoryAssignmentsImpl(
                    null,
                    null,
                    null,
                    Collections.GetEmptyMap<int, SubSelectFactoryResult>(),
                    tableAccessEvals,
                    null));
        }
    }
} // end of namespace