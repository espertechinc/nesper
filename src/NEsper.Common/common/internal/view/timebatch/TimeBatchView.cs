///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.view.timebatch
{
    /// <summary>
    ///     A data view that aggregates events in a stream and releases them in one batch at every specified time interval.
    ///     The view works similar to a time_window but in not continuous.
    ///     The view releases the batched events after the interval as new data to child views. The prior batch if
    ///     not empty is released as old data to child view. The view doesn't release intervals with no old or new data.
    ///     It also does not collect old data published by a parent view.
    ///     <para />
    ///     For example, we want to calculate the average of IBM stock every hour, for the last hour.
    ///     The view accepts 2 parameter combinations.
    ///     (1) A time interval is supplied with a reference point - based on this point the intervals are set.
    ///     (1) A time interval is supplied but no reference point - the reference point is set when the first event arrives.
    ///     <para />
    ///     If there are no events in the current and prior batch, the view will not invoke the update method of child views.
    ///     In that case also, no next callback is scheduled with the scheduling service until the next event arrives.
    /// </summary>
    public class TimeBatchView : ViewSupport,
        AgentInstanceMgmtCallback,
        DataWindowView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext _agentInstanceContext;

        // View parameters
        private readonly TimeBatchViewFactory _factory;
        private readonly long _scheduleSlot;
        private readonly TimePeriodProvide _timePeriodProvide;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        private ArrayDeque<EventBean> _currentBatch = new ArrayDeque<EventBean>();
        private long? _currentReferencePoint;

        // Current running parameters
        private EPStatementHandleCallbackSchedule _handle;
        private bool _isCallbackScheduled;
        private ArrayDeque<EventBean> _lastBatch;

        public TimeBatchView(
            TimeBatchViewFactory factory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ViewUpdatedCollection viewUpdatedCollection,
            TimePeriodProvide timePeriodProvide)
        {
            _agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            _factory = factory;
            _viewUpdatedCollection = viewUpdatedCollection;
            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            _timePeriodProvide = timePeriodProvide;

            // schedule the first callback
            if (factory.isStartEager) {
                if (_currentReferencePoint == null) {
                    _currentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                }

                ScheduleCallback();
                _isCallbackScheduled = true;
            }
        }

        public ViewFactory ViewFactory => _factory;

        public void Stop(AgentInstanceStopServices services)
        {
            if (_handle != null) {
                _agentInstanceContext.AuditProvider.ScheduleRemove(
                    _agentInstanceContext,
                    _handle,
                    ScheduleObjectType.view,
                    _factory.ViewName);
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
        }

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _agentInstanceContext.AuditProvider.View(newData, oldData, _agentInstanceContext, _factory);
            _agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(_factory, newData, oldData);

            // we don't care about removed data from a prior view
            if (newData == null || newData.Length == 0) {
                _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
                return;
            }

            // If we have an empty window about to be filled for the first time, schedule a callback
            if (_currentBatch.IsEmpty()) {
                if (_currentReferencePoint == null) {
                    _currentReferencePoint = _factory.optionalReferencePoint;
                    if (_currentReferencePoint == null) {
                        _currentReferencePoint = _agentInstanceContext.StatementContext.SchedulingService.Time;
                    }
                }

                // Schedule the next callback if there is none currently scheduled
                if (!_isCallbackScheduled) {
                    ScheduleCallback();
                    _isCallbackScheduled = true;
                }
            }

            // add data points to the timeWindow
            foreach (var newEvent in newData) {
                _currentBatch.Add(newEvent);
            }

            // We do not update child views, since we batch the events.
            _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _currentBatch.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_currentBatch, true, _factory.ViewName, null);
            viewDataVisitor.VisitPrimary(_lastBatch, true, _factory.ViewName, null);
        }

        /// <summary>
        ///     This method updates child views and clears the batch of events.
        ///     We schedule a new callback at this time if there were events in the batch.
        /// </summary>
        private void SendBatch()
        {
            _isCallbackScheduled = false;

            // If there are child views and the batch was filled, fireStatementStopped update method
            if (Child != null) {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (!_currentBatch.IsEmpty()) {
                    newData = _currentBatch.ToArray();
                }

                if (_lastBatch != null && !_lastBatch.IsEmpty()) {
                    oldData = _lastBatch.ToArray();
                }

                // Post new data (current batch) and old data (prior batch)
                _viewUpdatedCollection?.Update(newData, oldData);

                if (newData != null || oldData != null || _factory.isForceUpdate) {
                    _agentInstanceContext.InstrumentationProvider.QViewIndicate(_factory, newData, oldData);
                    Child.Update(newData, oldData);
                    _agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            // Only if forceOutput is enabled or
            // there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if (!_currentBatch.IsEmpty() ||
                (_lastBatch != null && !_lastBatch.IsEmpty()) ||
                _factory.isForceUpdate) {
                ScheduleCallback();
                _isCallbackScheduled = true;
            }

            _lastBatch = _currentBatch;
            _currentBatch = new ArrayDeque<EventBean>();
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (_lastBatch != null) {
                if (!_lastBatch.IsEmpty()) {
                    return false;
                }
            }

            return _currentBatch.IsEmpty();
        }

        public override string ToString()
        {
            return GetType().Name +
                   " initialReferencePoint=" +
                   _factory.optionalReferencePoint;
        }

        private void ScheduleCallback()
        {
            var current = _agentInstanceContext.StatementContext.SchedulingService.Time;
            var deltaWReference = _timePeriodProvide.DeltaAddWReference(
                current,
                _currentReferencePoint.Value,
                null,
                true,
                _agentInstanceContext);
            var afterTime = deltaWReference.Delta;
            _currentReferencePoint = deltaWReference.LastReference;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    _agentInstanceContext.AuditProvider.ScheduleFire(
                        _agentInstanceContext,
                        ScheduleObjectType.view,
                        _factory.ViewName);
                    _agentInstanceContext.InstrumentationProvider.QViewScheduledEval(_factory);
                    SendBatch();
                    _agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            _handle = new EPStatementHandleCallbackSchedule(
                _agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
            _agentInstanceContext.AuditProvider.ScheduleAdd(
                afterTime,
                _agentInstanceContext,
                _handle,
                ScheduleObjectType.view,
                _factory.ViewName);
            _agentInstanceContext.StatementContext.SchedulingService.Add(afterTime, _handle, _scheduleSlot);
        }
    }
} // end of namespace