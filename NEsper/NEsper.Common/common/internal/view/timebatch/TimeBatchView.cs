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
        AgentInstanceStopCallback,
        DataWindowView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext agentInstanceContext;

        // View parameters
        private readonly TimeBatchViewFactory factory;
        private readonly long scheduleSlot;
        private readonly TimePeriodProvide timePeriodProvide;
        private readonly ViewUpdatedCollection viewUpdatedCollection;
        private ArrayDeque<EventBean> currentBatch = new ArrayDeque<EventBean>();
        private long? currentReferencePoint;

        // Current running parameters
        private EPStatementHandleCallbackSchedule handle;
        private bool isCallbackScheduled;
        private ArrayDeque<EventBean> lastBatch;

        public TimeBatchView(
            TimeBatchViewFactory factory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ViewUpdatedCollection viewUpdatedCollection,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.factory = factory;
            this.viewUpdatedCollection = viewUpdatedCollection;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            // schedule the first callback
            if (factory.isStartEager) {
                if (currentReferencePoint == null) {
                    currentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                }

                ScheduleCallback();
                isCallbackScheduled = true;
            }
        }

        public ViewFactory ViewFactory => factory;

        public void Stop(AgentInstanceStopServices services)
        {
            if (handle != null) {
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext, handle, ScheduleObjectType.view, factory.ViewName);
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

            // we don't care about removed data from a prior view
            if (newData == null || newData.Length == 0) {
                agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
                return;
            }

            // If we have an empty window about to be filled for the first time, schedule a callback
            if (currentBatch.IsEmpty()) {
                if (currentReferencePoint == null) {
                    currentReferencePoint = factory.optionalReferencePoint;
                    if (currentReferencePoint == null) {
                        currentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                    }
                }

                // Schedule the next callback if there is none currently scheduled
                if (!isCallbackScheduled) {
                    ScheduleCallback();
                    isCallbackScheduled = true;
                }
            }

            // add data points to the timeWindow
            foreach (var newEvent in newData) {
                currentBatch.Add(newEvent);
            }

            // We do not update child views, since we batch the events.
            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return currentBatch.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(currentBatch, true, factory.ViewName, null);
            viewDataVisitor.VisitPrimary(lastBatch, true, factory.ViewName, null);
        }

        /// <summary>
        ///     This method updates child views and clears the batch of events.
        ///     We schedule a new callback at this time if there were events in the batch.
        /// </summary>
        private void SendBatch()
        {
            isCallbackScheduled = false;

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

                if (newData != null || oldData != null || factory.isForceUpdate) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                    Child.Update(newData, oldData);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            // Only if forceOutput is enabled or
            // there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if (!currentBatch.IsEmpty() || lastBatch != null && !lastBatch.IsEmpty()
                                        ||
                                        factory.isForceUpdate) {
                ScheduleCallback();
                isCallbackScheduled = true;
            }

            lastBatch = currentBatch;
            currentBatch = new ArrayDeque<EventBean>();
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
                   " initialReferencePoint=" + factory.optionalReferencePoint;
        }

        private void ScheduleCallback()
        {
            var current = agentInstanceContext.StatementContext.SchedulingService.Time;
            TimePeriodDeltaResult deltaWReference = timePeriodProvide.DeltaAddWReference(
                current, currentReferencePoint.Value, null, true, agentInstanceContext);
            long afterTime = deltaWReference.Delta;
            currentReferencePoint = deltaWReference.LastReference;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext, ScheduleObjectType.view, factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                    SendBatch();
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                afterTime, agentInstanceContext, handle, ScheduleObjectType.view, factory.ViewName);
            agentInstanceContext.StatementContext.SchedulingService.Add(afterTime, handle, scheduleSlot);
        }
    }
} // end of namespace