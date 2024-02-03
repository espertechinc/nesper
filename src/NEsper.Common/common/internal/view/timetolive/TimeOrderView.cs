///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.timetolive
{
    /// <summary>
    ///     Window retaining timestamped events up to a given number of seconds such that
    ///     older events that arrive later are sorted into the window and released in timestamp order.
    ///     <para />
    ///     The insert stream consists of all arriving events. The remove stream consists of events in
    ///     order of timestamp value as supplied by each event.
    ///     <para />
    ///     Timestamp values on events should match runtime time. The window compares runtime time to timestamp value
    ///     and releases events when the event's timestamp is less then runtime time minus interval size (the
    ///     event is older then the window tail).
    ///     <para />
    ///     The view accepts 2 parameters. The first parameter is the field name to get the event timestamp value from,
    ///     the second parameter defines the interval size.
    /// </summary>
    public class TimeOrderView : ViewSupport,
        DataWindowView,
        AgentInstanceMgmtCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly TimeOrderViewFactory factory;
        private readonly EPStatementHandleCallbackSchedule handle;
        private readonly IStreamSortRankRandomAccess optionalSortedRandomAccess;
        private readonly long scheduleSlot;
        private readonly TimePeriodProvide timePeriodProvide;
        private int eventCount;

        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private bool isCallbackScheduled;
        private readonly IOrderedDictionary<object, object> sortedEvents;

        public TimeOrderView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimeOrderViewFactory factory,
            IStreamSortRankRandomAccess optionalSortedRandomAccess,
            TimePeriodProvide timePeriodProvide)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.factory = factory;
            this.optionalSortedRandomAccess = optionalSortedRandomAccess;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            this.timePeriodProvide = timePeriodProvide;

            sortedEvents = new OrderedListDictionary<object, object>();

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext.AgentInstanceContext,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                    Expire();
                    agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
        }

        /// <summary>
        ///     True to indicate the sort window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty sort window</returns>
        public bool IsEmpty => sortedEvents.IsEmpty();

        public ViewFactory Factory => factory;

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

            EventBean[] postOldEventsArray = null;

            // Remove old data
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    var oldDataItem = oldData[i];
                    object sortValues = GetTimestamp(oldDataItem);
                    var result = CollectionUtil.RemoveEventByKeyLazyListMap(sortValues, oldDataItem, sortedEvents);
                    if (!result) {
                        result = CollectionUtil.RemoveEventUnkeyedLazyListMap(oldDataItem, sortedEvents);
                    }

                    if (result) {
                        eventCount--;
                        if (postOldEventsArray == null) {
                            postOldEventsArray = oldData;
                        }
                        else {
                            postOldEventsArray = CollectionUtil.AddArrayWithSetSemantics(postOldEventsArray, oldData);
                        }
                    }
                }
            }

            if (newData != null && newData.Length > 0) {
                // figure out the current tail time
                var runtimeTime = agentInstanceContext.StatementContext.SchedulingService.Time;
                var windowTailTime = runtimeTime -
                                     timePeriodProvide.DeltaAdd(runtimeTime, null, true, agentInstanceContext) +
                                     1;
                var oldestEvent = long.MaxValue;
                if (!sortedEvents.IsEmpty()) {
                    oldestEvent = sortedEvents.First().Key.AsInt64();
                }

                var addedOlderEvent = false;

                // add events or post events as remove stream if already older then tail time
                List<EventBean> postOldEvents = null;
                for (var i = 0; i < newData.Length; i++) {
                    // get timestamp of event
                    var newEvent = newData[i];
                    var timestamp = GetTimestamp(newEvent);

                    // if the event timestamp indicates its older then the tail of the window, release it
                    if (timestamp < windowTailTime) {
                        if (postOldEvents == null) {
                            postOldEvents = new List<EventBean>(2);
                        }

                        postOldEvents.Add(newEvent);
                    }
                    else {
                        if (timestamp < oldestEvent) {
                            addedOlderEvent = true;
                            oldestEvent = timestamp.Value;
                        }

                        // add to list
                        CollectionUtil.AddEventByKeyLazyListMapBack(timestamp, newEvent, sortedEvents);
                        eventCount++;
                    }
                }

                // If we do have data, check the callback
                if (!sortedEvents.IsEmpty()) {
                    // If we haven't scheduled a callback yet, schedule it now
                    if (!isCallbackScheduled) {
                        var callbackWait = oldestEvent - windowTailTime + 1;
                        agentInstanceContext.AuditProvider.ScheduleAdd(
                            callbackWait,
                            agentInstanceContext,
                            handle,
                            ScheduleObjectType.view,
                            factory.ViewName);
                        agentInstanceContext.StatementContext.SchedulingService.Add(callbackWait, handle, scheduleSlot);
                        isCallbackScheduled = true;
                    }
                    else {
                        // We may need to reschedule, and older event may have been added
                        if (addedOlderEvent) {
                            oldestEvent = sortedEvents.First().Key.AsInt64();
                            var callbackWait = oldestEvent - windowTailTime + 1;
                            agentInstanceContext.AuditProvider.ScheduleRemove(
                                agentInstanceContext,
                                handle,
                                ScheduleObjectType.view,
                                factory.ViewName);
                            agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
                            agentInstanceContext.AuditProvider.ScheduleAdd(
                                callbackWait,
                                agentInstanceContext,
                                handle,
                                ScheduleObjectType.view,
                                factory.ViewName);
                            agentInstanceContext.StatementContext.SchedulingService.Add(
                                callbackWait,
                                handle,
                                scheduleSlot);
                            isCallbackScheduled = true;
                        }
                    }
                }

                if (postOldEvents != null) {
                    postOldEventsArray = postOldEvents.ToArray();
                }

                optionalSortedRandomAccess?.Refresh(sortedEvents, eventCount, eventCount);
            }

            // update child views
            if (Child != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, postOldEventsArray);
                Child.Update(newData, postOldEventsArray);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return sortedEvents.GetMultiLevelEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(sortedEvents, false, factory.ViewName, eventCount, null);
        }

        protected long? GetTimestamp(EventBean newEvent)
        {
            eventsPerStream[0] = newEvent;
            return (long?)factory.timestampEval.Evaluate(eventsPerStream, true, agentInstanceContext);
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        ///     This method removes (expires) objects from the window and schedules a new callback for the
        ///     time when the next oldest message would expire from the window.
        /// </summary>
        protected void Expire()
        {
            var currentTime = agentInstanceContext.StatementContext.SchedulingService.Time;
            var expireBeforeTimestamp = currentTime -
                                        timePeriodProvide.DeltaSubtract(currentTime, null, true, agentInstanceContext) +
                                        1;
            isCallbackScheduled = false;

            IList<EventBean> releaseEvents = null;
            long? oldestKey;
            while (true) {
                if (sortedEvents.IsEmpty()) {
                    oldestKey = null;
                    break;
                }

                oldestKey = (long?)sortedEvents.First().Key;
                if (oldestKey >= expireBeforeTimestamp) {
                    break;
                }

                var released = sortedEvents.Delete(oldestKey);
                if (released != null) {
                    if (released is IList<EventBean> releasedEventList) {
                        if (releaseEvents == null) {
                            releaseEvents = releasedEventList;
                        }
                        else {
                            releaseEvents.AddAll(releasedEventList);
                        }

                        eventCount -= releasedEventList.Count;
                    }
                    else {
                        var releasedEvent = (EventBean)released;
                        if (releaseEvents == null) {
                            releaseEvents = new List<EventBean>(4);
                        }

                        releaseEvents.Add(releasedEvent);
                        eventCount--;
                    }
                }
            }

            optionalSortedRandomAccess?.Refresh(sortedEvents, eventCount, eventCount);

            // If there are child views, do the update method
            if (Child != null) {
                if (releaseEvents != null && !releaseEvents.IsEmpty()) {
                    var oldEvents = releaseEvents.ToArray();
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldEvents);
                    Child.Update(null, oldEvents);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            // If we still have events in the window, schedule new callback
            if (oldestKey == null) {
                return;
            }

            // Next callback
            var callbackWait = oldestKey.Value - expireBeforeTimestamp + 1;
            agentInstanceContext.AuditProvider.ScheduleAdd(
                callbackWait,
                agentInstanceContext,
                handle,
                ScheduleObjectType.view,
                factory.ViewName);
            agentInstanceContext.StatementContext.SchedulingService.Add(callbackWait, handle, scheduleSlot);
            isCallbackScheduled = true;
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace