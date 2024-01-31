///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.view.time_accum
{
    /// <summary>
    ///     A data window view that holds events in a stream and only removes events from a stream (rstream) if
    ///     no more events arrive for a given time interval.
    ///     <para />
    ///     No batch version of the view exists as the batch version is simply the remove stream of this view, which removes
    ///     in batches.
    ///     <para />
    ///     The view is continuous, the insert stream consists of arriving events. The remove stream
    ///     only posts current window contents when no more events arrive for a given timer interval.
    /// </summary>
    public class TimeAccumView : ViewSupport,
        DataWindowView,
        AgentInstanceMgmtCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;

        // View parameters
        private readonly TimeAccumViewFactory factory;
        private readonly long scheduleSlot;
        private readonly TimePeriodProvide timePeriodProvide;
        private readonly ViewUpdatedCollection viewUpdatedCollection;
        private long callbackScheduledTime;

        // Current running parameters
        private readonly List<EventBean> currentBatch = new List<EventBean>();
        private readonly EPStatementHandleCallbackSchedule handle;

        public TimeAccumView(
            TimeAccumViewFactory timeBatchViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ViewUpdatedCollection viewUpdatedCollection,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            factory = timeBatchViewFactory;
            this.viewUpdatedCollection = viewUpdatedCollection;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext.AgentInstanceContext,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                    SendRemoveStream();
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => currentBatch.IsEmpty();

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

            // we don't care about removed data from a prior view
            if (newData == null || newData.Length == 0) {
                return;
            }

            // If we have an empty window about to be filled for the first time, addSchedule a callback
            var removeSchedule = false;
            var addSchedule = false;
            var timestamp = agentInstanceContext.StatementContext.SchedulingService.Time;

            if (!currentBatch.IsEmpty()) {
                // check if we need to reschedule
                var callbackTime = timestamp + timePeriodProvide.DeltaAdd(timestamp, null, true, agentInstanceContext);
                if (callbackTime != callbackScheduledTime) {
                    removeSchedule = true;
                    addSchedule = true;
                }
            }
            else {
                addSchedule = true;
            }

            if (removeSchedule) {
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    handle,
                    ScheduleObjectType.view,
                    factory.ViewName);
                agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
            }

            if (addSchedule) {
                var timeIntervalSize = timePeriodProvide.DeltaAdd(timestamp, null, true, agentInstanceContext);
                agentInstanceContext.AuditProvider.ScheduleAdd(
                    timeIntervalSize,
                    agentInstanceContext,
                    handle,
                    ScheduleObjectType.view,
                    factory.ViewName);
                agentInstanceContext.StatementContext.SchedulingService.Add(timeIntervalSize, handle, scheduleSlot);
                callbackScheduledTime = timeIntervalSize + timestamp;
            }

            // add data points to the window
            foreach (var newEvent in newData) {
                currentBatch.Add(newEvent);
            }

            // forward insert stream to child views
            viewUpdatedCollection?.Update(newData, null);

            // update child views
            if (Child != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, null);
                Child.Update(newData, null);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return currentBatch.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(currentBatch, true, factory.ViewName, null);
        }

        /// <summary>
        ///     This method sends the remove stream for all accumulated events.
        /// </summary>
        private void SendRemoveStream()
        {
            callbackScheduledTime = -1;

            // If there are child views and the batch was filled, fireStatementStopped update method
            if (Child != null) {
                // Convert to object arrays
                EventBean[] oldData = null;
                if (!currentBatch.IsEmpty()) {
                    oldData = currentBatch.ToArray();
                }

                // Post old data
                viewUpdatedCollection?.Update(null, oldData);

                if (oldData != null) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldData);
                    Child.Update(null, oldData);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            currentBatch.Clear();
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace