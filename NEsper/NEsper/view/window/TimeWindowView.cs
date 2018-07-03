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
using com.espertech.esper.collection;
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
    /// This view is a moving timeWindow extending the specified amount of milliseconds into the 
    /// past. The view bases the timeWindow on the time obtained from the scheduling service. All 
    /// incoming events receive a timestamp and are placed in a sorted map by timestamp. The view 
    /// does not care about old data published by the parent view to this view. Events leave or 
    /// expire from the time timeWindow by means of a scheduled callback registered with the 
    /// scheduling service. Thus child views receive updates containing old data only asynchronously 
    /// as the system-time-based timeWindow moves on. However child views receive updates containing 
    /// new data as soon as the new data arrives.
    /// </summary>
    public class TimeWindowView
        : ViewSupport
        , CloneableView
        , DataWindowView
        , ScheduleAdjustmentCallback
        , StoppableView
        , StopCallback
    {
        private readonly TimeWindowViewFactory _timeWindowViewFactory;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly TimeWindow _timeWindow;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly long _scheduleSlot;
        private readonly EPStatementHandleCallback _handle;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="timeWindowViewFactory">for copying the view in a group-by</param>
        /// <param name="timeDeltaComputation">is the computation of number of milliseconds before events gets pushedout of the timeWindow as oldData in the Update method.</param>
        /// <param name="viewUpdatedCollection">is a collection the view must Update when receiving events</param>
        public TimeWindowView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimeWindowViewFactory timeWindowViewFactory,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            _agentInstanceContext = agentInstanceContext;
            _timeWindowViewFactory = timeWindowViewFactory;
            _timeDeltaComputation = timeDeltaComputation;
            _viewUpdatedCollection = viewUpdatedCollection;
            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            _timeWindow = new TimeWindow(agentInstanceContext.IsRemoveStream);

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, timeWindowViewFactory.ViewName),
                    i => i.AViewScheduledEval(),
                    Expire)
            };

            _handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);

            if (agentInstanceContext.StatementContext.ScheduleAdjustmentService != null)
                agentInstanceContext.StatementContext.ScheduleAdjustmentService.AddCallback(this);
            agentInstanceContext.AddTerminationCallback(Stop);
        }

        public void Adjust(long delta)
        {
            _timeWindow.Adjust(delta);
        }

        public View CloneView()
        {
            return _timeWindowViewFactory.MakeView(_agentInstanceContext);
        }

        /// <summary>Returns the (optional) collection handling random access to window contents for prior or previous events. </summary>
        /// <returns>buffer for events</returns>
        public ViewUpdatedCollection GetViewUpdatedCollection()
        {
            return _viewUpdatedCollection;
        }

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            Instrument.With(
                i => i.QViewProcessIRStream(this, _timeWindowViewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream(),
                () =>
                {
                    long timestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;

                    if (oldData != null)
                    {
                        for (int i = 0; i < oldData.Length; i++)
                        {
                            _timeWindow.Remove(oldData[i]);
                        }
                    }

                    // we don't care about removed data from a prior view
                    if ((newData != null) && (newData.Length > 0))
                    {
                        // If we have an empty window about to be filled for the first time, schedule a callback
                        // for now plus timeDeltaComputation
                        if (_timeWindow.IsEmpty())
                        {
                            long current = _agentInstanceContext.StatementContext.SchedulingService.Time;
                            ScheduleCallback(_timeDeltaComputation.DeltaAdd(current));
                        }

                        // add data points to the timeWindow
                        for (int i = 0; i < newData.Length; i++)
                        {
                            _timeWindow.Add(timestamp, newData[i]);
                        }

                        if (_viewUpdatedCollection != null)
                        {
                            _viewUpdatedCollection.Update(newData, null);
                        }
                    }

                    // Update child views
                    if (HasViews)
                    {
                        Instrument.With(
                            i => i.QViewIndicate(this, _timeWindowViewFactory.ViewName, newData, oldData),
                            i => i.AViewIndicate(),
                            () => UpdateChildren(newData, oldData));
                    }
                });
        }

        /// <summary>
        /// This method removes (expires) objects from the window and schedules a new callback for the
        /// time when the next oldest message would expire from the window.
        /// </summary>
        public void Expire()
        {
            long current = _agentInstanceContext.StatementContext.SchedulingService.Time;
            long expireBeforeTimestamp = current - _timeDeltaComputation.DeltaSubtract(current) + 1;

            // Remove from the timeWindow any events that have an older or timestamp then the given timestamp
            // The window : from X to (X - timeDeltaComputation + 1)
            var expired = _timeWindow.ExpireEvents(expireBeforeTimestamp);

            // If there are child views, fireStatementStopped Update method
            if (HasViews)
            {
                if ((expired != null) && (expired.IsNotEmpty()))
                {
                    EventBean[] oldEvents = expired.ToArray();
                    if (_viewUpdatedCollection != null)
                    {
                        _viewUpdatedCollection.Update(null, oldEvents);
                    }

                    Instrument.With(
                        i => i.QViewIndicate(this, _timeWindowViewFactory.ViewName, null, oldEvents),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(null, oldEvents));
                }
            }

            ScheduleExpiryCallback();
        }

        protected void ScheduleExpiryCallback()
        {
            // If we still have events in the window, schedule new callback
            if (_timeWindow.IsEmpty())
            {
                return;
            }
            var oldestTimestamp = _timeWindow.OldestTimestamp;
            var currentTimestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;
            var scheduleTime = _timeDeltaComputation.DeltaAdd(oldestTimestamp.Value) + oldestTimestamp - currentTimestamp;
            ScheduleCallback(scheduleTime.Value);
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation => _timeDeltaComputation;

        private void ScheduleCallback(long timeAfterCurrentTime)
        {
            _agentInstanceContext.StatementContext.SchedulingService.Add(timeAfterCurrentTime, _handle, _scheduleSlot);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _timeWindow.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _timeWindow.IsEmpty();
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
            if (_agentInstanceContext.StatementContext.ScheduleAdjustmentService != null)
            {
                _agentInstanceContext.StatementContext.ScheduleAdjustmentService.RemoveCallback(this);
            }
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            _timeWindow.VisitView(viewDataVisitor, _timeWindowViewFactory);
        }

        public ViewFactory ViewFactory => _timeWindowViewFactory;
    }
}
