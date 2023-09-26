///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Antlr4.Runtime.Misc;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
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
        private volatile IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>>
            _consumersInContext;

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
        /// <value>number of events</value>
        public long NumberOfEvents => _numberOfEvents;

        public NamedWindowTailView TailView => _tailView;

        public override EventType EventType => _tailView.EventType;

        public AgentInstanceContext AgentInstanceContext => _agentInstanceContext;

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
                child.Update(newData, oldData);
            }

            var delta = new NamedWindowDeltaData(newData, oldData);
            TailView.AddDispatches(_latchFactory, _consumersInContext, delta, _agentInstanceContext);
        }

        public NamedWindowConsumerView AddConsumer(
            NamedWindowConsumerDesc consumerDesc,
            bool isSubselect)
        {
            NamedWindowConsumerCallback consumerCallback = new ProxyNamedWindowConsumerCallback {
                ProcGetEnumerator = () => {
                    var instance = _namedWindow.GetNamedWindowInstance(_agentInstanceContext);
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
                consumerDesc.NamedWindowConsumerId,
                consumerDesc.FilterEvaluator,
                consumerDesc.OptPropertyEvaluator,
                TailView.EventType,
                consumerCallback,
                consumerDesc.AgentInstanceContext,
                audit);

            // indicate to virtual data window that a consumer was added
            var virtualDWView = _rootViewInstance.VirtualDataWindow;
            virtualDWView?.VirtualDataWindow.HandleEvent(
                new VirtualDataWindowEventConsumerAdd(
                    TailView.EventType.Name,
                    consumerView,
                    consumerDesc.AgentInstanceContext.StatementName,
                    consumerDesc.AgentInstanceContext.AgentInstanceId,
                    consumerDesc.FilterEvaluator,
                    _agentInstanceContext));

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
        /// <param name="namedWindowConsumerView">is the consumer representative view</param>
        public void RemoveConsumer(NamedWindowConsumerView namedWindowConsumerView) {
            EPStatementAgentInstanceHandle handleRemoved = null;
            // Find the consumer view
            foreach (var entry in _consumersInContext) {
                var foundAndRemoved = entry.Value.Remove(namedWindowConsumerView);
                // Remove the consumer view
                if (foundAndRemoved && (entry.Value.Count == 0)) {
                    // Remove the handle if this list is now empty
                    handleRemoved = entry.Key;
                    break;
                }
            }
            if (handleRemoved != null) {
                var newConsumers = NamedWindowUtil.CreateConsumerMap(_tailView.IsPrioritized);
                newConsumers.PutAll(_consumersInContext);
                newConsumers.Remove(handleRemoved);
                _consumersInContext = newConsumers;
            }

            // indicate to virtual data window that a consumer was added
            var virtualDWView = _rootViewInstance.VirtualDataWindow;
            if (virtualDWView != null && handleRemoved != null) {
                virtualDWView.VirtualDataWindow.HandleEvent(
                    new VirtualDataWindowEventConsumerRemove(
                        _tailView.EventType.Name,
                        namedWindowConsumerView,
                        handleRemoved.StatementHandle.StatementName,
                        handleRemoved.AgentInstanceId));
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            using (_agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireReadLock()) {
                var enumerator = parent.GetEnumerator();
                if (!enumerator.MoveNext()) {
                    return CollectionUtil.NULL_EVENT_ITERATOR;
                }

                var list = new List<EventBean>();
                do {
                    list.Add(enumerator.Current);
                } while (enumerator.MoveNext());

                return new ArrayEventEnumerator(list.ToArray());
            }
        }

        public ICollection<EventBean> Snapshot(
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            using (_agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireReadLock()) {
                try {
                    return SnapshotNoLock(queryGraph, annotations);
                }
                finally {
                    ReleaseTableLocks(_agentInstanceContext);
                }
            }
        }

        public EventBean[] SnapshotUpdate(
            QueryGraph filterQueryGraph,
            ExprEvaluator optionalWhereClause,
            EventBeanUpdateHelperWCopy updateHelper,
            Attribute[] annotations)
        {
            _agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                var events = SnapshotNoLockWithFilter(
                    filterQueryGraph,
                    annotations,
                    optionalWhereClause,
                    _agentInstanceContext);
                if (events.IsEmpty()) {
                    return CollectionUtil.EVENTBEANARRAY_EMPTY;
                }

                var eventsPerStream = new EventBean[3];
                var updated = new EventBean[events.Count];
                var count = 0;
                foreach (var @event in events) {
                    updated[count++] = updateHelper.Invoke(@event, eventsPerStream, _agentInstanceContext);
                }

                var deleted = events.ToArray();
                _rootViewInstance.Update(updated, deleted);
                return updated;
            }
            finally {
                ReleaseTableLocks(_agentInstanceContext);
                _agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReleaseWriteLock();
            }
        }

        public EventBean[] SnapshotDelete(
            QueryGraph filterQueryGraph,
            ExprEvaluator filterExpr,
            Attribute[] annotations)
        {
            _agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                var events = SnapshotNoLockWithFilter(filterQueryGraph, annotations, filterExpr, _agentInstanceContext);
                if (events.IsEmpty()) {
                    return CollectionUtil.EVENTBEANARRAY_EMPTY;
                }

                var eventsDeleted = events.ToArray();
                _rootViewInstance.Update(null, eventsDeleted);
                return eventsDeleted;
            }
            finally {
                ReleaseTableLocks(_agentInstanceContext);
                _agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReleaseWriteLock();
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

            var enumerator = parent.GetEnumerator();
            if (!enumerator.MoveNext()) {
                return EmptyList<EventBean>.Instance;
            }

            var list = new ArrayDeque<EventBean>();
            do {
                list.Add(enumerator.Current);
            } while (enumerator.MoveNext());

            return list;
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
                    indexedResult.GetEnumerator(),
                    filterExpr,
                    exprEvaluatorContext,
                    deque);
                return deque;
            }

            // fall back to window operator if snapshot doesn't resolve successfully
            var enumerator = parent.GetEnumerator();
            if (!enumerator.MoveNext()) {
                return EmptyList<EventBean>.Instance;
            }

            var list = new ArrayDeque<EventBean>();
            if (filterExpr != null) {
                ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(enumerator, filterExpr, _agentInstanceContext, list);
            }
            else {
                do {
                    list.Add(enumerator.Current);
                } while (enumerator.MoveNext());
            }

            return list;
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