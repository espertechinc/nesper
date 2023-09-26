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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.timewin
{
    /// <summary>
    ///     This view is a moving timeWindow extending the specified amount of milliseconds into the past.
    ///     The view bases the timeWindow on the time obtained from the scheduling service.
    ///     All incoming events receive a timestamp and are placed in a sorted map by timestamp.
    ///     The view does not care about old data published by the parent view to this view.
    ///     <para />
    ///     Events leave or expire from the time timeWindow by means of a scheduled callback registered with the
    ///     scheduling service. Thus child views receive updates containing old data only asynchronously
    ///     as the system-time-based timeWindow moves on. However child views receive updates containing new data
    ///     as soon as the new data arrives.
    /// </summary>
    public class TimeWindowView : ViewSupport,
        DataWindowView,
        AgentInstanceMgmtCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EPStatementHandleCallbackSchedule handle;
        private readonly long scheduleSlot;
        private readonly TimePeriodProvide timePeriodProvide;
        private readonly TimeWindow timeWindow;
        private readonly TimeWindowViewFactory timeWindowViewFactory;

        public TimeWindowView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimeWindowViewFactory timeWindowViewFactory,
            ViewUpdatedCollection viewUpdatedCollection,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.timeWindowViewFactory = timeWindowViewFactory;
            ViewUpdatedCollection = viewUpdatedCollection;
            timeWindow = new TimeWindow(agentInstanceContext.IsRemoveStream);
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        this.agentInstanceContext,
                        ScheduleObjectType.view,
                        timeWindowViewFactory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(timeWindowViewFactory);
                    Expire();
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
        }

        /// <summary>
        ///     Returns the (optional) collection handling random access to window contents for prior or previous events.
        /// </summary>
        /// <returns>buffer for events</returns>
        public ViewUpdatedCollection ViewUpdatedCollection { get; }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => timeWindow.IsEmpty();

        public ViewFactory ViewFactory => timeWindowViewFactory;

        public void Stop(AgentInstanceStopServices services)
        {
            if (handle != null) {
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    handle,
                    ScheduleObjectType.view,
                    timeWindowViewFactory.ViewName);
                agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
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
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, timeWindowViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(timeWindowViewFactory, newData, oldData);
            var timestamp = agentInstanceContext.StatementContext.SchedulingService.Time;

            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    timeWindow.Remove(oldData[i]);
                }
            }

            // we don't care about removed data from a prior view
            if (newData != null && newData.Length > 0) {
                // If we have an empty window about to be filled for the first time, schedule a callback
                // for now plus millisecondsBeforeExpiry
                if (timeWindow.IsEmpty()) {
                    var current = agentInstanceContext.StatementContext.SchedulingService.Time;
                    ScheduleCallback(timePeriodProvide.DeltaAdd(current, null, true, agentInstanceContext));
                }

                // add data points to the timeWindow
                for (var i = 0; i < newData.Length; i++) {
                    timeWindow.Add(timestamp, newData[i]);
                }

                ViewUpdatedCollection?.Update(newData, null);
            }

            // update child views
            agentInstanceContext.InstrumentationProvider.QViewIndicate(timeWindowViewFactory, newData, oldData);
            Child.Update(newData, oldData);
            agentInstanceContext.InstrumentationProvider.AViewIndicate();

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return timeWindow.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            timeWindow.VisitView(viewDataVisitor, timeWindowViewFactory);
        }

        /// <summary>
        ///     This method removes (expires) objects from the window and schedules a new callback for the
        ///     time when the next oldest message would expire from the window.
        /// </summary>
        private void Expire()
        {
            var current = agentInstanceContext.StatementContext.SchedulingService.Time;
            var expireBeforeTimestamp =
                current - timePeriodProvide.DeltaSubtract(current, null, true, agentInstanceContext) + 1;

            // Remove from the timeWindow any events that have an older or timestamp then the given timestamp
            // The window : from X to (X - millisecondsBeforeExpiry + 1)
            var expired = timeWindow.ExpireEvents(expireBeforeTimestamp);

            // If there are child views, fireStatementStopped update method
            if (Child != null) {
                if (expired != null && !expired.IsEmpty()) {
                    var oldEvents = expired.ToArray();
                    ViewUpdatedCollection?.Update(null, oldEvents);

                    agentInstanceContext.InstrumentationProvider.QViewIndicate(timeWindowViewFactory, null, oldEvents);
                    Child.Update(null, oldEvents);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            ScheduleExpiryCallback();
        }

        private void ScheduleExpiryCallback()
        {
            var scheduleTime = ComputeScheduleTime();
            if (scheduleTime == -1) {
                return;
            }

            ScheduleCallback(scheduleTime);
        }

        private long ComputeScheduleTime()
        {
            if (timeWindow.IsEmpty()) {
                return -1;
            }

            var oldestTimestamp = timeWindow.OldestTimestamp.Value; // Null check?
            var currentTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
            return timePeriodProvide.DeltaAdd(oldestTimestamp, null, true, agentInstanceContext) +
                   oldestTimestamp -
                   currentTimestamp;
        }

        private void ScheduleCallback(long timeAfterCurrentTime)
        {
            agentInstanceContext.AuditProvider.ScheduleAdd(
                timeAfterCurrentTime,
                agentInstanceContext,
                handle,
                ScheduleObjectType.view,
                timeWindowViewFactory.ViewName);
            agentInstanceContext.StatementContext.SchedulingService.Add(timeAfterCurrentTime, handle, scheduleSlot);
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
} // end of namespace