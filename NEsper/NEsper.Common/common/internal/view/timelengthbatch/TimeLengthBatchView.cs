///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.timelengthbatch
{
    /// <summary>
    ///     A data view that aggregates events in a stream and releases them in one batch if either one of these
    ///     conditions is reached, whichever comes first: One, a time interval passes. Two, a given number of events collected.
    ///     <para />
    ///     The view releases the batched events after the interval or number of events as new data to child views. The prior
    ///     batch if
    ///     not empty is released as old data to child view. The view DOES release intervals with no old or new data.
    ///     It does not collect old data published by a parent view.
    ///     If there are no events in the current and prior batch, the view WILL invoke the update method of child views.
    ///     <para />
    ///     The view starts the first interval when the view is created.
    /// </summary>
    public class TimeLengthBatchView : ViewSupport,
        AgentInstanceMgmtCallback,
        DataWindowView
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly TimeLengthBatchViewFactory factory;
        private readonly long scheduleSlot;
        private readonly int size;
        private readonly TimePeriodProvide timePeriodProvide;
        private readonly ViewUpdatedCollection viewUpdatedCollection;
        internal long? callbackScheduledTime;
        internal List<EventBean> currentBatch = new List<EventBean>();
        internal EPStatementHandleCallbackSchedule handle;

        // Current running parameters
        internal List<EventBean> lastBatch;

        public TimeLengthBatchView(
            TimeLengthBatchViewFactory factory,
            int size,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ViewUpdatedCollection viewUpdatedCollection,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.factory = factory;
            this.size = size;
            this.viewUpdatedCollection = viewUpdatedCollection;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            // schedule the first callback
            if (factory.IsStartEager) {
                ScheduleCallback(0);
            }
        }

        public ViewFactory ViewFactory => factory;

        public void Stop(AgentInstanceStopServices services)
        {
            if (handle != null) {
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    handle,
                    ScheduleObjectType.view,
                    factory.ViewName);
                agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
            }
        }

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    if (currentBatch.Remove(oldData[i])) {
                        InternalHandleRemoved(oldData[i]);
                    }
                }
            }

            // we don't care about removed data from a prior view
            if (newData == null || newData.Length == 0) {
                agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
                return;
            }

            // Add data points
            foreach (var newEvent in newData) {
                currentBatch.Add(newEvent);
                InternalHandleAdded(newEvent);
            }

            // We are done unless we went over the boundary
            if (currentBatch.Count < size) {
                // Schedule a callback if there is none scheduled
                if (callbackScheduledTime == null) {
                    ScheduleCallback(0);
                }

                agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
                return;
            }

            // send a batch of events
            SendBatch(false);

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return currentBatch.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(lastBatch, true, factory.ViewName, null);
            viewDataVisitor.VisitPrimary(currentBatch, true, factory.ViewName, null);
        }

        public void InternalHandleAdded(EventBean newEvent)
        {
            // no action required
        }

        public void InternalHandleRemoved(EventBean eventBean)
        {
            // no action required
        }

        /// <summary>
        ///     This method updates child views and clears the batch of events.
        ///     We cancel and old callback and schedule a new callback at this time if there were events in the batch.
        /// </summary>
        /// <param name="isFromSchedule">true if invoked from a schedule, false if not</param>
        protected void SendBatch(bool isFromSchedule)
        {
            // No more callbacks scheduled if called from a schedule
            if (isFromSchedule) {
                callbackScheduledTime = null;
            }
            else {
                // Remove schedule if called from on overflow due to number of events
                if (callbackScheduledTime != null) {
                    agentInstanceContext.AuditProvider.ScheduleRemove(
                        agentInstanceContext,
                        handle,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
                    callbackScheduledTime = null;
                }
            }

            // If there are child views and the batch was filled, fireStatementStopped update method
            if (Child != null) {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (!currentBatch.IsEmpty()) {
                    newData = currentBatch.ToArray();
                }

                if (lastBatch != null && !lastBatch.IsEmpty()) {
                    oldData = lastBatch.ToArray();
                }

                // Post new data (current batch) and old data (prior batch)
                if (viewUpdatedCollection != null) {
                    viewUpdatedCollection.Update(newData, oldData);
                }

                if (newData != null || oldData != null || factory.IsForceUpdate) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                    Child.Update(newData, oldData);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            // Only if there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if (!currentBatch.IsEmpty() ||
                lastBatch != null && !lastBatch.IsEmpty() ||
                factory.isForceUpdate) {
                ScheduleCallback(0);
            }

            // Flush and roll
            lastBatch = currentBatch;
            currentBatch = new List<EventBean>();
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (lastBatch != null) {
                if (!lastBatch.IsEmpty()) {
                    return false;
                }
            }

            return currentBatch.IsEmpty();
        }

        public override string ToString()
        {
            return GetType().Name +
                   " numberOfEvents=" +
                   size;
        }

        protected void ScheduleCallback(long delta)
        {
            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                    SendBatch(true);
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
            var currentTime = agentInstanceContext.StatementContext.SchedulingService.Time;
            var scheduled = timePeriodProvide.DeltaAdd(currentTime, null, true, agentInstanceContext) - delta;
            agentInstanceContext.StatementContext.SchedulingService.Add(scheduled, handle, scheduleSlot);
            callbackScheduledTime = agentInstanceContext.StatementContext.SchedulingService.Time + scheduled;
        }
        
        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace