///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.timebatch
{
    /// <summary>
    ///     Same as the <seealso cref="TimeBatchView" />, this view also supports fast-remove from the batch for remove stream
    ///     events.
    /// </summary>
    public class TimeBatchViewRStream : ViewSupport,
        AgentInstanceMgmtCallback,
        DataWindowView
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly TimeBatchViewFactory factory;
        private readonly long scheduleSlot;
        private readonly TimePeriodProvide timePeriodProvide;
        private LinkedHashSet<EventBean> currentBatch = new LinkedHashSet<EventBean>();

        // Current running parameters
        private long? currentReferencePoint;
        private EPStatementHandleCallbackSchedule handle;
        private bool isCallbackScheduled;
        private LinkedHashSet<EventBean> lastBatch;

        public TimeBatchViewRStream(
            TimeBatchViewFactory factory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.factory = factory;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            // schedule the first callback
            if (factory.isStartEager) {
                currentReferencePoint = agentInstanceContext.StatementContext.SchedulingService.Time;
                ScheduleCallback();
                isCallbackScheduled = true;
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
                    currentBatch.Remove(oldData[i]);
                }
            }

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
        protected void SendBatch()
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

                if (newData != null || oldData != null || factory.isForceUpdate) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                    Child.Update(newData, oldData);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            // Only if forceOutput is enabled or
            // there have been any events in this or the last interval do we schedule a callback,
            // such as to not waste resources when no events arrive.
            if (!currentBatch.IsEmpty() ||
                (lastBatch != null && !lastBatch.IsEmpty()) ||
                factory.isForceUpdate) {
                ScheduleCallback();
                isCallbackScheduled = true;
            }

            lastBatch = currentBatch;
            currentBatch = new LinkedHashSet<EventBean>();
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
                   " initialReferencePoint=" +
                   factory.optionalReferencePoint;
        }

        protected void ScheduleCallback()
        {
            var current = agentInstanceContext.StatementContext.SchedulingService.Time;
            var deltaWReference = timePeriodProvide.DeltaAddWReference(
                current,
                currentReferencePoint.Value,
                null,
                true,
                agentInstanceContext);
            var afterTime = deltaWReference.Delta;
            currentReferencePoint = deltaWReference.LastReference;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                    SendBatch();
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                afterTime,
                agentInstanceContext,
                handle,
                ScheduleObjectType.view,
                factory.ViewName);
            agentInstanceContext.StatementContext.SchedulingService.Add(afterTime, handle, scheduleSlot);
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace