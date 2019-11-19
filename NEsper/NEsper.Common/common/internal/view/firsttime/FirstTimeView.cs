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

namespace com.espertech.esper.common.@internal.view.firsttime
{
    public class FirstTimeView : ViewSupport,
        DataWindowView,
        AgentInstanceStopCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly FirstTimeViewFactory factory;
        private readonly long scheduleSlot;
        private readonly TimePeriodProvide timePeriodProvide;

        // Current running parameters
        private EPStatementHandleCallbackSchedule handle;
        private bool isClosed;

        public FirstTimeView(
            FirstTimeViewFactory factory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.factory = factory;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            ScheduleCallback();
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => Events.IsEmpty();

        public bool IsClosed {
            set => isClosed = value;
        }

        public LinkedHashSet<EventBean> Events { get; } = new LinkedHashSet<EventBean>();

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

            OneEventCollection oldDataToPost = null;
            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    var removed = Events.Remove(anOldData);
                    if (removed) {
                        if (oldDataToPost == null) {
                            oldDataToPost = new OneEventCollection();
                        }

                        oldDataToPost.Add(anOldData);
                    }
                }
            }

            // add data points to the timeWindow
            OneEventCollection newDataToPost = null;
            if (!isClosed && newData != null) {
                foreach (var aNewData in newData) {
                    Events.Add(aNewData);
                    if (newDataToPost == null) {
                        newDataToPost = new OneEventCollection();
                    }

                    newDataToPost.Add(aNewData);
                }
            }

            // If there are child views, call update method
            if (Child != null && (newDataToPost != null || oldDataToPost != null)) {
                EventBean[] nd = newDataToPost != null ? newDataToPost.ToArray() : null;
                EventBean[] od = oldDataToPost != null ? oldDataToPost.ToArray() : null;
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, nd, od);
                Child.Update(nd, od);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Events.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(Events, true, factory.ViewName, null);
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        private void ScheduleCallback()
        {
            long afterTime = timePeriodProvide.DeltaAdd(
                agentInstanceContext.StatementContext.SchedulingService.Time,
                null,
                true,
                agentInstanceContext);

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                    isClosed = true;
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
    }
} // end of namespace