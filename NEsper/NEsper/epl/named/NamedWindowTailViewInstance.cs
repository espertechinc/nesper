///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// This view is hooked into a named window's view chain as the last view and handles dispatching of named window
    /// insert and remove stream results via <seealso cref="NamedWindowMgmtService" /> to consuming statements.
    /// </summary>
    public class NamedWindowTailViewInstance : ViewSupport, IEnumerable<EventBean>
    {
        private readonly NamedWindowRootViewInstance _rootViewInstance;
        private readonly NamedWindowTailView _tailView;
        private readonly NamedWindowProcessor _namedWindowProcessor;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly NamedWindowConsumerLatchFactory _latchFactory;
    
        private volatile IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> _consumersInContext;  // handles as copy-on-write
        private long _numberOfEvents;
    
        public NamedWindowTailViewInstance(
            NamedWindowRootViewInstance rootViewInstance, 
            NamedWindowTailView tailView, 
            NamedWindowProcessor namedWindowProcessor, 
            AgentInstanceContext agentInstanceContext)
        {
            this._rootViewInstance = rootViewInstance;
            this._tailView = tailView;
            this._namedWindowProcessor = namedWindowProcessor;
            this._agentInstanceContext = agentInstanceContext;
            this._consumersInContext = NamedWindowUtil.CreateConsumerMap(tailView.IsPrioritized);
            this._latchFactory = tailView.MakeLatchFactory();
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData) {
            // Only old data (remove stream) needs to be removed from indexes (kept by root view), if any
            if (oldData != null) {
                _rootViewInstance.RemoveOldData(oldData);
                _numberOfEvents -= oldData.Length;
            }
    
            if ((newData != null) && (!_tailView.IsParentBatchWindow)) {
                _rootViewInstance.AddNewData(newData);
            }
    
            if (newData != null) {
                _numberOfEvents += newData.Length;
            }
    
            // Post to child views, only if there are listeners or subscribers
            if (_tailView.StatementResultService.IsMakeNatural || _tailView.StatementResultService.IsMakeSynthetic) {
                UpdateChildren(newData, oldData);
            }
    
            var delta = new NamedWindowDeltaData(newData, oldData);
            _tailView.AddDispatches(_latchFactory, _consumersInContext, delta, _agentInstanceContext);
        }
    
        public NamedWindowConsumerView AddConsumer(NamedWindowConsumerDesc consumerDesc, bool isSubselect) {
            var consumerCallback = new ProxyNamedWindowConsumerCallback() {
                ProcGetEnumerator = () => {
                    NamedWindowProcessorInstance instance = _namedWindowProcessor.GetProcessorInstance(_agentInstanceContext);
                    if (instance == null) {
                        // this can happen on context-partition "output when terminated"
                        return GetEnumerator();
                    }
                    return instance.TailViewInstance.GetEnumerator();
                },    
                ProcStopped = (namedWindowConsumerView) => {
                    RemoveConsumer(namedWindowConsumerView);
                }
            };
    
            // Construct consumer view, allow a callback to this view to remove the consumer
            bool audit = AuditEnum.STREAM.GetAudit(consumerDesc.AgentInstanceContext.StatementContext.Annotations) != null;
            var consumerView = new NamedWindowConsumerView(ExprNodeUtility.GetEvaluators(consumerDesc.FilterList), consumerDesc.OptPropertyEvaluator, _tailView.EventType, consumerCallback, consumerDesc.AgentInstanceContext, audit);
    
            // indicate to virtual data window that a consumer was added
            VirtualDWView virtualDWView = _rootViewInstance.VirtualDataWindow;
            if (virtualDWView != null) {
                virtualDWView.VirtualDataWindow.HandleEvent(
                        new VirtualDataWindowEventConsumerAdd(_tailView.EventType.Name, consumerView, consumerDesc.AgentInstanceContext.StatementName, consumerDesc.AgentInstanceContext.AgentInstanceId, ExprNodeUtility.ToArray(consumerDesc.FilterList), _agentInstanceContext));
            }
    
            // Keep a list of consumer views per statement to accommodate joins and subqueries
            var viewsPerStatements = _consumersInContext.Get(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle);
            if (viewsPerStatements == null) {
                viewsPerStatements = new CopyOnWriteList<NamedWindowConsumerView>();
    
                // avoid concurrent modification as a thread may currently iterate over consumers as its dispatching
                // without the engine lock
                var newConsumers = NamedWindowUtil.CreateConsumerMap(_tailView.IsPrioritized);
                newConsumers.PutAll(_consumersInContext);
                newConsumers.Put(consumerDesc.AgentInstanceContext.EpStatementAgentInstanceHandle, viewsPerStatements);
                _consumersInContext = newConsumers;
            }
            if (isSubselect) {
                viewsPerStatements.Insert(0, consumerView);
            } else {
                viewsPerStatements.Add(consumerView);
            }
    
            return consumerView;
        }
    
        /// <summary>
        /// Called by the consumer view to indicate it was stopped or destroyed, such that the
        /// consumer can be deregistered and further dispatches disregard this consumer.
        /// </summary>
        /// <param name="namedWindowConsumerView">is the consumer representative view</param>
        public void RemoveConsumer(NamedWindowConsumerView namedWindowConsumerView) {
            EPStatementAgentInstanceHandle handleRemoved = null;
            // Find the consumer view
            foreach (var entry in _consumersInContext) {
                bool foundAndRemoved = entry.Value.Remove(namedWindowConsumerView);
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
            VirtualDWView virtualDWView = _rootViewInstance.VirtualDataWindow;
            if (virtualDWView != null && handleRemoved != null) {
                virtualDWView.VirtualDataWindow.HandleEvent(new VirtualDataWindowEventConsumerRemove(_tailView.EventType.Name, namedWindowConsumerView, handleRemoved.StatementHandle.StatementName, handleRemoved.AgentInstanceId));
            }
        }

        public override EventType EventType => _tailView.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_tailView.RevisionProcessor != null)
            {
                var coll = _tailView.RevisionProcessor.GetSnapshot(_agentInstanceContext.EpStatementAgentInstanceHandle, Parent);
                return coll.GetEnumerator();
            }

            using (_agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReadLock.Acquire())
            {
                var en = Parent.GetEnumerator();
                if (!en.MoveNext())
                {
                    return CollectionUtil.NULL_EVENT_ITERATOR;
                }
                var list = new List<EventBean>();
                do {
                    list.Add(en.Current);
                } while (en.MoveNext());

                return list.GetEnumerator();
            }
        }
    
        /// <summary>
        /// Returns a snapshot of window contents, thread-safely
        /// </summary>
        /// <param name="queryGraph">query graph</param>
        /// <param name="annotations">annotations</param>
        /// <returns>window contents</returns>
        public ICollection<EventBean> Snapshot(QueryGraph queryGraph, Attribute[] annotations)
        {
            if (_tailView.RevisionProcessor != null) {
                return _tailView.RevisionProcessor.GetSnapshot(_agentInstanceContext.EpStatementAgentInstanceHandle, Parent);
            }

            using (_agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReadLock.Acquire())
            {
                try
                {
                    return SnapshotNoLock(queryGraph, annotations);
                }
                finally
                {
                    ReleaseTableLocks(_agentInstanceContext);
                }
            }
        }
    
        public EventBean[] SnapshotUpdate(
            QueryGraph queryGraph, 
            ExprNode optionalWhereClause, 
            EventBeanUpdateHelper updateHelper, 
            Attribute[] annotations)
        {
            using (_agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReadLock.Acquire())
            {
                try
                {
                    var events = SnapshotNoLockWithFilter(queryGraph, annotations, optionalWhereClause, _agentInstanceContext);
                    if (events.IsEmpty())
                    {
                        return CollectionUtil.EVENTBEANARRAY_EMPTY;
                    }

                    var eventsPerStream = new EventBean[3];
                    var updated = new EventBean[events.Count];
                    int count = 0;
                    foreach (EventBean @event in events)
                    {
                        updated[count++] = updateHelper.UpdateWCopy(@event, eventsPerStream, _agentInstanceContext);
                    }

                    var deleted = events.ToArray();
                    _rootViewInstance.Update(updated, deleted);
                    return updated;
                }
                finally
                {
                    ReleaseTableLocks(_agentInstanceContext);
                }
            }
        }
    
        public EventBean[] SnapshotDelete(QueryGraph queryGraph, ExprNode filterExpr, Attribute[] annotations)
        {
            using (_agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.ReadLock.Acquire())
            {
                try
                {
                    var events = SnapshotNoLockWithFilter(queryGraph, annotations, filterExpr, _agentInstanceContext);
                    if (events.IsEmpty())
                    {
                        return CollectionUtil.EVENTBEANARRAY_EMPTY;
                    }
                    var eventsDeleted = events.ToArray();
                    _rootViewInstance.Update(null, eventsDeleted);
                    return eventsDeleted;
                }
                finally
                {
                    ReleaseTableLocks(_agentInstanceContext);
                }
            }
        }
    
        public ICollection<EventBean> SnapshotNoLock(QueryGraph queryGraph, Attribute[] annotations)
        {
            if (_tailView.RevisionProcessor != null) {
                return TailView.RevisionProcessor.GetSnapshot(_agentInstanceContext.EpStatementAgentInstanceHandle, Parent);
            }
    
            var indexedResult = _rootViewInstance.Snapshot(queryGraph, annotations);
            if (indexedResult != null) {
                return indexedResult;
            }
            var en = Parent.GetEnumerator();
            if (!en.MoveNext()) {
                return Collections.GetEmptyList<EventBean>();
            }

            var list = new ArrayDeque<EventBean>(1024);
            do {
                list.Add(en.Current);
            } while (en.MoveNext());

            return list;
        }
    
        public ICollection<EventBean> SnapshotNoLockWithFilter(
            QueryGraph queryGraph, 
            Attribute[] annotations, 
            ExprNode filterExpr, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_tailView.RevisionProcessor != null) {
                return TailView.RevisionProcessor.GetSnapshot(_agentInstanceContext.EpStatementAgentInstanceHandle, Parent);
            }

            var indexedResult = _rootViewInstance.Snapshot(queryGraph, annotations);
            if (indexedResult != null) {
                if (indexedResult.IsEmpty()) {
                    return indexedResult;
                }
                if (filterExpr == null) {
                    return indexedResult;
                }
                var deque = new ArrayDeque<EventBean>(Math.Min(indexedResult.Count, 16));
                ExprNodeUtility.ApplyFilterExpressionIterable(indexedResult.GetEnumerator(), filterExpr.ExprEvaluator, exprEvaluatorContext, deque);
                return deque;
            }
    
            // fall back to window operator if snapshot doesn't resolve successfully
            var en = Parent.GetEnumerator();
            if (!en.MoveNext()) {
                return Collections.GetEmptyList<EventBean>();
            }

            var list = new ArrayDeque<EventBean>();
            if (filterExpr != null) {
                // rewind the enumerator by one value
                en = en.Prepend(en.Current);
                ExprNodeUtility.ApplyFilterExpressionIterable(en, filterExpr.ExprEvaluator, _agentInstanceContext, list);
            } else {
                do {
                    list.Add(en.Current);
                } while (en.MoveNext());
            }
            return list;
        }

        public AgentInstanceContext AgentInstanceContext => _agentInstanceContext;

        /// <summary>Destroy the view.</summary>
        public void Destroy() {
            _consumersInContext = NamedWindowUtil.CreateConsumerMap(_tailView.IsPrioritized);
        }

        /// <summary>
        /// Returns the number of events held.
        /// </summary>
        /// <returns>number of events</returns>
        public long NumberOfEvents => _numberOfEvents;

        public NamedWindowTailView TailView => _tailView;

        private void ReleaseTableLocks(AgentInstanceContext agentInstanceContext) {
            agentInstanceContext.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
        }
    
        public void Stop() {
            // no action
        }
    }
} // end of namespace
