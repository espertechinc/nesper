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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// A data window view that holds events in a stream and only removes events from a stream 
    /// (rstream) if no more events arrive for a given time interval, also handling the remove 
    /// stream by keeping set-like semantics. See <seealso cref="TimeAccumView"/> for the same 
    /// behavior without remove stream handling.
    /// </summary>
    public class TimeAccumViewRStream
        : ViewSupport
        , CloneableView
        , DataWindowView
        , StoppableView
        , StopCallback
    {
        // View parameters
        private readonly TimeAccumViewFactory _factory;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly long _scheduleSlot;

        // Current running parameters
        private readonly LinkedHashMap<EventBean, long> _currentBatch = new LinkedHashMap<EventBean, long>();
        private EventBean _lastEvent;
        private long _callbackScheduledTime;
        private readonly EPStatementHandleCallback _handle;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeBatchViewFactory">for copying this view in a group-by</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="timeDeltaComputation">The time delta computation.</param>
        public TimeAccumViewRStream(
            TimeAccumViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation)
        {
            _agentInstanceContext = agentInstanceContext;
            _factory = timeBatchViewFactory;
            _timeDeltaComputation = timeDeltaComputation;

            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext =>
                {
                    using (Instrument.With(
                        i => i.QViewScheduledEval(this, _factory.ViewName),
                        i => i.AViewScheduledEval()))
                    {
                        SendRemoveStream();
                    }
                }
            };

            _handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            agentInstanceContext.AddTerminationCallback(Stop);
        }

        public View CloneView()
        {
            return _factory.MakeView(_agentInstanceContext);
        }

        /// <summary>Returns the interval size</summary>
        /// <value>batch size</value>
        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _factory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                if ((newData != null) && (newData.Length > 0))
                {
                    // If we have an empty window about to be filled for the first time, add a callback
                    bool removeSchedule = false;
                    bool addSchedule = false;
                    long timestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;

                    // if the window is already filled, then we may need to reschedule
                    if (_currentBatch.IsNotEmpty())
                    {
                        // check if we need to reschedule
                        long callbackTime = timestamp + _timeDeltaComputation.DeltaMillisecondsAdd(timestamp);
                        if (callbackTime != _callbackScheduledTime)
                        {
                            removeSchedule = true;
                            addSchedule = true;
                        }
                    }
                    else
                    {
                        addSchedule = true;
                    }

                    if (removeSchedule)
                    {
                        _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                        _callbackScheduledTime = -1;
                    }
                    if (addSchedule)
                    {
                        long msecIntervalSize = _timeDeltaComputation.DeltaMillisecondsAdd(timestamp);
                        _agentInstanceContext.StatementContext.SchedulingService.Add(
                            msecIntervalSize, _handle, _scheduleSlot);
                        _callbackScheduledTime = msecIntervalSize + timestamp;
                    }

                    // add data points to the window
                    for (int i = 0; i < newData.Length; i++)
                    {
                        _currentBatch.Put(newData[i], timestamp);
                        InternalHandleAdded(newData[i], timestamp);
                        _lastEvent = newData[i];
                    }
                }

                if ((oldData != null) && (oldData.Length > 0))
                {
                    bool removedLastEvent = false;
                    foreach (EventBean anOldData in oldData)
                    {
                        _currentBatch.Remove(anOldData);
                        InternalHandleRemoved(anOldData);
                        if (anOldData == _lastEvent)
                        {
                            removedLastEvent = true;
                        }
                    }

                    // we may need to reschedule as the newest event may have been deleted
                    if (_currentBatch.Count == 0)
                    {
                        _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                        _callbackScheduledTime = -1;
                        _lastEvent = null;
                    }
                    else
                    {
                        // reschedule if the last event was removed
                        if (removedLastEvent)
                        {
                            ICollection<EventBean> keyset = _currentBatch.Keys;
                            EventBean[] events = keyset.ToArray();
                            _lastEvent = events[events.Length - 1];
                            long lastTimestamp = _currentBatch.Get(_lastEvent);

                            // reschedule, newest event deleted
                            long timestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;
                            long callbackTime = lastTimestamp + _timeDeltaComputation.DeltaMillisecondsAdd(lastTimestamp);
                            long deltaFromNow = callbackTime - timestamp;
                            if (callbackTime != _callbackScheduledTime)
                            {
                                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                                _agentInstanceContext.StatementContext.SchedulingService.Add(
                                    deltaFromNow, _handle, _scheduleSlot);
                                _callbackScheduledTime = callbackTime;
                            }
                        }
                    }
                }

                // Update child views
                if (HasViews)
                {
                    using (Instrument.With(
                        i => i.QViewIndicate(this, _factory.ViewName, newData, oldData),
                        i => i.AViewIndicate()))
                    {
                        UpdateChildren(newData, oldData);
                    }
                }
            }
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_currentBatch, true, _factory.ViewName, _currentBatch.Count, null);
        }

        /// <summary>This method sends the remove stream for all accumulated events. </summary>
        protected void SendRemoveStream()
        {
            _callbackScheduledTime = -1;

            // If there are child views and the batch was filled, fireStatementStopped Update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] oldData = null;
                if (_currentBatch.IsNotEmpty())
                {
                    oldData = _currentBatch.Keys.ToArray();
                }

                if (oldData != null)
                {
                    using (Instrument.With(
                        i => i.QViewIndicate(this, _factory.ViewName, null, oldData),
                        i => i.AViewIndicate()))
                    {
                        UpdateChildren(null, oldData);
                    }
                }
            }

            _currentBatch.Clear();
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _currentBatch.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _currentBatch.Keys.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        public void StopView()
        {
            StopSchedule();
            _agentInstanceContext.RemoveTerminationCallback(Stop);
        }

        public void Stop()
        {
            StopSchedule();
        }

        public void StopSchedule()
        {
            if (_handle != null)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
        }

        public void InternalHandleRemoved(EventBean anOldData)
        {
            // no action required
        }

        public void InternalHandleAdded(EventBean eventBean, long timestamp)
        {
            // no action required
        }

        public ViewFactory ViewFactory
        {
            get { return _factory; }
        }
    }
}
