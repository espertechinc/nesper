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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    ///     This view is hooked into a named window's view chain as the last view and handles dispatching of named window
    ///     insert and remove stream results via <seealso cref="NamedWindowManagementService" /> to consuming statements.
    /// </summary>
    public class NamedWindowTailViewInstance : ViewSupport,
        IEnumerable<EventBean>
    {
        private readonly NamedWindowConsumerLatchFactory _latchFactory;
        private readonly NamedWindow _namedWindow;
        private readonly NamedWindowRootViewInstance _rootViewInstance;
        private readonly NamedWindowTailView _tailView;
        private readonly AgentInstanceContext _agentInstanceContext;

        // handles as copy-on-write
        private volatile IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> _consumersInContext;
        private long _numberOfEvents;

        public NamedWindowTailViewInstance(
            NamedWindowRootViewInstance rootViewInstance,
            NamedWindowTailView tailView,
            NamedWindow namedWindow,
            AgentInstanceContext agentInstanceContext)
        {
            _rootViewInstance = rootViewInstance;
            _tailView = tailView;
            _namedWindow = namedWindow;
            _agentInstanceContext = agentInstanceContext;
            _consumersInContext = NamedWindowUtil.CreateConsumerMap(tailView.IsPrioritized);
            _latchFactory = tailView.MakeLatchFactory();
        }

        /// <summary>
        ///     Returns the number of events held.
        /// </summary>
        /// <returns>number of events</returns>
        public long NumberOfEvents => _numberOfEvents;

        public NamedWindowTailView TailView => _tailView;

        public override EventType EventType => TailView.EventType;

        public AgentInstanceContext AgentInstanceContext => _agentInstanceContext;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireReadLock();
            try {
                using (var it = parent.GetEnumerator()) {
                    if (!it.MoveNext()) {
                        return CollectionUtil.NULL_EVENT_ITERATOR;
                    }

                    var list = new List<EventBean>();
                    while (it.MoveNext()) {
                        list.Add(it.Current);
                    }

                    return list.GetEnumerator();
                }
            }
            finally {
                AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReleaseReadLock();
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // Only old data (remove stream) needs to be removed from indexes (kept by root view), if any
            if (oldData != null) {
                _rootViewInstance.RemoveOldData(oldData);
                _numberOfEvents -= oldData.Length;
            }

            if (newData != null && !TailView.IsParentBatchWindow) {
                _rootViewInstance.AddNewData(newData);
            }

            if (newData != null) {
                _numberOfEvents += newData.Length;
            }

            // Post to child views, only if there are listeners or subscribers
            if (TailView.StatementResultService.IsMakeNatural || TailView.StatementResultService.IsMakeSynthetic) {
                Child.Update(newData, oldData);
            }

            var delta = new NamedWindowDeltaData(newData, oldData);
            TailView.AddDispatches(_latchFactory, _consumersInContext, delta, AgentInstanceContext);
        }

        public NamedWindowConsumerView AddConsumer(
            NamedWindowConsumerDesc consumerDesc,
            bool isSubselect)
        {
            NamedWindowConsumerCallback consumerCallback = new ProxyNamedWindowConsumerCallback {
                ProcGetEnumerator = () => {
                    var instance = _namedWindow.GetNamedWindowInstance(AgentInstanceContext);
                    if (instance == null) {
                        // this can happen on context-partition "output when terminated"
                        return GetEnumerator();
                    }

                    return instance.TailViewInstance.GetEnumerator();
                },
                ProcStopped = namedWindowConsumerView
                    => RemoveConsumer(namedWindowConsumerView),
                ProcIsParentBatchWindow = ()
                    => _rootViewInstance.IsParentBatchWindow,
                ProcSnapshot = (
                        queryGraph,
                        annotations)
                    => Snapshot(queryGraph, annotations)
            };

            // Construct consumer view, allow a callback to this view to remove the consumer
            var audit = AuditEnum.STREAM.GetAudit(consumerDesc.AgentInstanceContext.StatementContext.Annotations) !=
                        null;
            var consumerView = new NamedWindowConsumerView(
                consumerDesc.NamedWindowConsumerId, consumerDesc.FilterEvaluator, consumerDesc.OptPropertyEvaluator,
                TailView.EventType, consumerCallback, consumerDesc.AgentInstanceContext, audit);

            // indicate to virtual data window that a consumer was added
            var virtualDWView = _rootViewInstance.VirtualDataWindow;
            if (virtualDWView != null) {
                virtualDWView.VirtualDataWindow.HandleEvent(
                    new VirtualDataWindowEventConsumerAdd(
                        TailView.EventType.Name, consumerView, consumerDesc.AgentInstanceContext.StatementName,
                        consumerDesc.AgentInstanceContext.AgentInstanceId, consumerDesc.FilterEvaluator,
                        AgentInstanceContext));
            }

            // Keep a list of consumer views per statement to accommodate joins and subqueries
            var viewsPerStatements =
                _consumersInContext.Get(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle);
            if (viewsPerStatements == null) {
                viewsPerStatements = new CopyOnWriteList<NamedWindowConsumerView>();

                // avoid concurrent modification as a thread may currently iterate over consumers as its dispatching
                // without the runtimelock
                var newConsumers = NamedWindowUtil.CreateConsumerMap(TailView.IsPrioritized);
                newConsumers.PutAll(_consumersInContext);
                newConsumers.Put(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle, viewsPerStatements);
                _consumersInContext = newConsumers;
            }

            if (isSubselect) {
                viewsPerStatements.Insert(0, consumerView);
            }
            else {
                viewsPerStatements.Add(consumerView);
            }

            return consumerView;
        }

        /// <summary>
        ///     Called by the consumer view to indicate it was stopped or destroyed, such that the
        ///     consumer can be deregistered and further dispatches disregard this consumer.
        /// </summary>
        public void RemoveConsumer(NamedWindowConsumerView namedWindowConsumerView)
        {
            EPStatementAgentInstanceHandle handleRemoved = null;
            // Find the consumer view
            foreach (var entry in _consumersInContext) {
                var foundAndRemoved = entry.Value.Remove(namedWindowConsumerView);
                // Remove the consumer view
                if (foundAndRemoved && entry.Value.IsEmpty()) {
                    // Remove the handle if this list is now empty
                    handleRemoved = entry.Key;
                    break;
                }
            }

            if (handleRemoved != null) {
                var newConsumers = NamedWindowUtil.CreateConsumerMap(TailView.IsPrioritized);
                newConsumers.PutAll(_consumersInContext);
                newConsumers.Remove(handleRemoved);
                _consumersInContext = newConsumers;
            }

            // indicate to virtual data window that a consumer was added
            var virtualDWView = _rootViewInstance.VirtualDataWindow;
            if (virtualDWView != null && handleRemoved != null) {
                virtualDWView.VirtualDataWindow.HandleEvent(
                    new VirtualDataWindowEventConsumerRemove(
                        TailView.EventType.Name,
                        namedWindowConsumerView,
                        handleRemoved.StatementHandle.StatementName,
                        handleRemoved.AgentInstanceId));
            }
        }

        public ICollection<EventBean> Snapshot(
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireReadLock();
            try {
                return SnapshotNoLock(queryGraph, annotations);
            }
            finally {
                ReleaseTableLocks(AgentInstanceContext);
                AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReleaseReadLock();
            }
        }

        public EventBean[] SnapshotUpdate(
            QueryGraph filterQueryGraph,
            ExprEvaluator optionalWhereClause,
            EventBeanUpdateHelperWCopy updateHelper,
            Attribute[] annotations)
        {
            AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireReadLock();
            try {
                var events = SnapshotNoLockWithFilter(
                    filterQueryGraph, annotations, optionalWhereClause, AgentInstanceContext);
                if (events.IsEmpty()) {
                    return CollectionUtil.EVENTBEANARRAY_EMPTY;
                }

                var eventsPerStream = new EventBean[3];
                var updated = new EventBean[events.Count];
                var count = 0;
                foreach (var @event in events) {
                    updated[count++] = updateHelper.UpdateWCopy(@event, eventsPerStream, AgentInstanceContext);
                }

                var deleted = events.ToArray();
                _rootViewInstance.Update(updated, deleted);
                return updated;
            }
            finally {
                ReleaseTableLocks(AgentInstanceContext);
                AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReleaseReadLock();
            }
        }

        public EventBean[] SnapshotDelete(
            QueryGraph filterQueryGraph,
            ExprEvaluator filterExpr,
            Attribute[] annotations)
        {
            AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireReadLock();
            try {
                var events = SnapshotNoLockWithFilter(filterQueryGraph, annotations, filterExpr, AgentInstanceContext);
                if (events.IsEmpty()) {
                    return CollectionUtil.EVENTBEANARRAY_EMPTY;
                }

                var eventsDeleted = events.ToArray();
                _rootViewInstance.Update(null, eventsDeleted);
                return eventsDeleted;
            }
            finally {
                ReleaseTableLocks(AgentInstanceContext);
                AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReleaseReadLock();
            }
        }

        public ICollection<EventBean> SnapshotNoLock(
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            var indexedResult = _rootViewInstance.Snapshot(queryGraph, annotations);
            if (indexedResult != null) {
                return indexedResult;
            }

            return parent.ToList();

#if DEPRECATED
            var it = parent.GetEnumerator();
            if (!it.MoveNext()) {
                return Collections.GetEmptyList<EventBean>();
            }

            var list = new ArrayDeque<EventBean>();
            while (it.MoveNext()) {
                list.Add(it.Current);
            }

            return list;
#endif
        }

        public ICollection<EventBean> SnapshotNoLockWithFilter(
            QueryGraph filterQueryGraph,
            Attribute[] annotations,
            ExprEvaluator filterExpr,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var indexedResult = _rootViewInstance.Snapshot(filterQueryGraph, annotations);
            if (indexedResult != null) {
                if (indexedResult.IsEmpty()) {
                    return indexedResult;
                }

                if (filterExpr == null) {
                    return indexedResult;
                }

                var deque = new ArrayDeque<EventBean>(Math.Min(indexedResult.Count, 16));
                ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                    indexedResult.GetEnumerator(), filterExpr, exprEvaluatorContext, deque);
                return deque;
            }

            // fall back to window operator if snapshot doesn't resolve successfully
            using (var it = parent.GetEnumerator()) {
                if (!it.MoveNext()) {
                    return Collections.GetEmptyList<EventBean>();
                }

                var list = new ArrayDeque<EventBean>();
                if (filterExpr != null) {
                    ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(it, filterExpr, AgentInstanceContext, list);
                }
                else {
                    while (it.MoveNext()) {
                        list.Add(it.Current);
                    }
                }

                return list;
            }
        }

        public void Destroy()
        {
            _consumersInContext = NamedWindowUtil.CreateConsumerMap(TailView.IsPrioritized);
        }

        private void ReleaseTableLocks(AgentInstanceContext agentInstanceContext)
        {
            agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
        }

        public void Stop()
        {
            // no action
        }
    }
} // end of namespace