///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.time_accum
{
    /// <summary>
    /// A data window view that holds events in a stream and only removes events from a stream (rstream) if
    /// no more events arrive for a given time interval, also handling the remove stream
    /// by keeping set-like semantics. See <seealso cref="TimeAccumView" /> for the same behavior without
    /// remove stream handling.
    /// </summary>
    public class TimeAccumViewRStream : ViewSupport,
        DataWindowView,
        AgentInstanceMgmtCallback
    {
        private readonly TimeAccumViewFactory _factory;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly long _scheduleSlot;
        private readonly TimePeriodProvide _timePeriodProvide;

        // Current running parameters
        private LinkedHashMap<EventBean, long> _currentBatch = new LinkedHashMap<EventBean, long>();

        private EventBean _lastEvent;
        private long _callbackScheduledTime;
        private EPStatementHandleCallbackSchedule _handle;

        public ViewFactory ViewFactory => _factory;

        public TimeAccumViewRStream(
            TimeAccumViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimePeriodProvide timePeriodProvide)
        {
            _agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            _factory = timeBatchViewFactory;
            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            _timePeriodProvide = timePeriodProvide;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback() {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext.AgentInstanceContext,
                        ScheduleObjectType.view,
                        _factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(_factory);
                    SendRemoveStream();
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                },
            };
            _handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
        }

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _agentInstanceContext.AuditProvider.View(newData, oldData, _agentInstanceContext, _factory);
            _agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(_factory, newData, oldData);

            if ((newData != null) && (newData.Length > 0)) {
                // If we have an empty window about to be filled for the first time, add a callback
                bool removeSchedule = false;
                bool addSchedule = false;
                long timestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;

                // if the window is already filled, then we may need to reschedule
                if (!_currentBatch.IsEmpty()) {
                    // check if we need to reschedule
                    long callbackTime =
                        timestamp + _timePeriodProvide.DeltaAdd(timestamp, null, true, _agentInstanceContext);
                    if (callbackTime != _callbackScheduledTime) {
                        removeSchedule = true;
                        addSchedule = true;
                    }
                }
                else {
                    addSchedule = true;
                }

                if (removeSchedule) {
                    _agentInstanceContext.AuditProvider.ScheduleRemove(
                        _agentInstanceContext,
                        _handle,
                        ScheduleObjectType.view,
                        _factory.ViewName);
                    _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                    _callbackScheduledTime = -1;
                }

                if (addSchedule) {
                    long timeIntervalSize = _timePeriodProvide.DeltaAdd(timestamp, null, true, _agentInstanceContext);
                    _agentInstanceContext.AuditProvider.ScheduleAdd(
                        timeIntervalSize,
                        _agentInstanceContext,
                        _handle,
                        ScheduleObjectType.view,
                        _factory.ViewName);
                    _agentInstanceContext.StatementContext.SchedulingService.Add(
                        timeIntervalSize,
                        _handle,
                        _scheduleSlot);
                    _callbackScheduledTime = timeIntervalSize + timestamp;
                }

                // add data points to the window
                for (int i = 0; i < newData.Length; i++) {
                    _currentBatch.Put(newData[i], timestamp);
                    _lastEvent = newData[i];
                }
            }

            if ((oldData != null) && (oldData.Length > 0)) {
                bool removedLastEvent = false;
                foreach (EventBean anOldData in oldData) {
                    _currentBatch.Remove(anOldData);
                    if (anOldData == _lastEvent) {
                        removedLastEvent = true;
                    }
                }

                // we may need to reschedule as the newest event may have been deleted
                if (_currentBatch.Count == 0) {
                    _agentInstanceContext.AuditProvider.ScheduleRemove(
                        _agentInstanceContext,
                        _handle,
                        ScheduleObjectType.view,
                        _factory.ViewName);
                    _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                    _callbackScheduledTime = -1;
                    _lastEvent = null;
                }
                else {
                    // reschedule if the last event was removed
                    if (removedLastEvent) {
                        EventBean[] events = _currentBatch.Keys.ToArray();
                        _lastEvent = events[events.Length - 1];
                        long lastTimestamp = _currentBatch.Get(_lastEvent);

                        // reschedule, newest event deleted
                        long timestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;
                        long callbackTime = lastTimestamp +
                                            _timePeriodProvide.DeltaAdd(
                                                lastTimestamp,
                                                null,
                                                true,
                                                _agentInstanceContext);
                        long deltaFromNow = callbackTime - timestamp;
                        if (callbackTime != _callbackScheduledTime) {
                            _agentInstanceContext.AuditProvider.ScheduleRemove(
                                _agentInstanceContext,
                                _handle,
                                ScheduleObjectType.view,
                                _factory.ViewName);
                            _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                            _agentInstanceContext.AuditProvider.ScheduleAdd(
                                deltaFromNow,
                                _agentInstanceContext,
                                _handle,
                                ScheduleObjectType.view,
                                _factory.ViewName);
                            _agentInstanceContext.StatementContext.SchedulingService.Add(
                                deltaFromNow,
                                _handle,
                                _scheduleSlot);
                            _callbackScheduledTime = callbackTime;
                        }
                    }
                }
            }

            // update child views
            var child = Child;
            if (child != null) {
                _agentInstanceContext.InstrumentationProvider.QViewIndicate(_factory, newData, oldData);
                child.Update(newData, oldData);
                _agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_currentBatch, true, _factory.ViewName, _currentBatch.Count, null);
        }

        /// <summary>
        /// This method sends the remove stream for all accumulated events.
        /// </summary>
        private void SendRemoveStream()
        {
            _callbackScheduledTime = -1;

            // If there are child views and the batch was filled, fireStatementStopped update method
            var child = Child;
            if (child != null) {
                // Convert to object arrays
                EventBean[] oldData = null;
                if (!_currentBatch.IsEmpty()) {
                    oldData = _currentBatch.Keys.ToArray();
                }

                if (oldData != null) {
                    _agentInstanceContext.InstrumentationProvider.QViewIndicate(_factory, null, oldData);
                    child.Update(null, oldData);
                    _agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            _currentBatch.Clear();
        }

        /// <summary>
        /// Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => _currentBatch.IsEmpty();

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _currentBatch.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name;
        }

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
    }
} // end of namespace