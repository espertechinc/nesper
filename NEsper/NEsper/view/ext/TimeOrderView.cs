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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.view.ext
{
    /// <summary>
    /// Window retaining timestamped events up to a given number of seconds such that older events 
    /// that arrive later are sorted into the window and released in timestamp order. 
    /// <para/>
    /// The insert stream consists of all arriving events. The remove stream consists of events in 
    /// order of timestamp value as supplied by each event. <para/>Timestamp values on events should 
    /// match engine time. The window compares engine time to timestamp value and releases events 
    /// when the event's timestamp is less then engine time minus interval size (the event is older 
    /// then the window tail).
    /// <para/>
    /// The view accepts 2 parameters. The first parameter is the field name to get the event timestamp 
    /// value from, the second parameter defines the interval size.
    /// </summary>
    public class TimeOrderView
        : ViewSupport
        , DataWindowView
        , CloneableView
        , StoppableView
        , StopCallback
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ViewFactory _viewFactory;
        private readonly ExprNode _timestampExpression;
        private readonly ExprEvaluator _timestampEvaluator;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
        private readonly IStreamSortRankRandomAccess _optionalSortedRandomAccess;
        private readonly long _scheduleSlot;
        private readonly EPStatementHandleCallback _handle;

        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly OrderedDictionary<Object, Object> _sortedEvents;
        private bool _isCallbackScheduled;
        private int _eventCount;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="viewFactory">for copying this view in a group-by</param>
        /// <param name="timestampExpr">the property name of the event supplying timestamp values</param>
        /// <param name="timestampEvaluator">The timestamp evaluator.</param>
        /// <param name="timeDeltaComputation">The time delta computation.</param>
        /// <param name="optionalSortedRandomAccess">is the friend class handling the random access, if required byexpressions</param>
        public TimeOrderView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ViewFactory viewFactory,
            ExprNode timestampExpr,
            ExprEvaluator timestampEvaluator,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            IStreamSortRankRandomAccess optionalSortedRandomAccess)
        {
            _agentInstanceContext = agentInstanceContext;
            _viewFactory = viewFactory;
            _timestampExpression = timestampExpr;
            _timestampEvaluator = timestampEvaluator;
            _timeDeltaComputation = timeDeltaComputation;
            _optionalSortedRandomAccess = optionalSortedRandomAccess;
            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();

            _sortedEvents = new OrderedDictionary<Object, Object>();

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QViewScheduledEval(this, _viewFactory.ViewName),
                    i => i.AViewScheduledEval(),
                    Expire)
            };

            _handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            agentInstanceContext.AddTerminationCallback(Stop);
        }

        /// <summary>Returns the timestamp property name. </summary>
        /// <value>property name supplying timestamp values</value>
        public ExprNode TimestampExpression
        {
            get { return _timestampExpression; }
        }

        /// <summary>Returns the time interval size. </summary>
        /// <value>interval size</value>
        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        public View CloneView()
        {
            return _viewFactory.MakeView(_agentInstanceContext);
        }

        public override EventType EventType
        {
            get
            {
                // The schema is the parent view's schema
                return Parent.EventType;
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _viewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                EventBean[] postOldEventsArray = null;

                // Remove old data
                if (oldData != null)
                {
                    for (int i = 0; i < oldData.Length; i++)
                    {
                        var oldDataItem = oldData[i];
                        var sortValues = GetTimestamp(oldDataItem);
                        var result = CollectionUtil.RemoveEventByKeyLazyListMap(sortValues, oldDataItem, _sortedEvents);
                        if (result)
                        {
                            _eventCount--;
                            if (postOldEventsArray == null)
                            {
                                postOldEventsArray = oldData;
                            }
                            else
                            {
                                postOldEventsArray = CollectionUtil.AddArrayWithSetSemantics(
                                    postOldEventsArray, oldData);
                            }
                            InternalHandleRemoved(sortValues, oldDataItem);
                        }
                    }
                }

                if ((newData != null) && (newData.Length > 0))
                {
                    // figure out the current tail time
                    long engineTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
                    long windowTailTime = engineTime - _timeDeltaComputation.DeltaAdd(engineTime) + 1;
                    long oldestEvent = long.MaxValue;
                    if (_sortedEvents.IsNotEmpty())
                    {
                        oldestEvent = (long)_sortedEvents.Keys.First();
                    }
                    bool addedOlderEvent = false;

                    // add events or post events as remove stream if already older then tail time
                    List<EventBean> postOldEvents = null;
                    for (int i = 0; i < newData.Length; i++)
                    {
                        // get timestamp of event
                        var newEvent = newData[i];
                        var timestamp = GetTimestamp(newEvent);

                        // if the event timestamp indicates its older then the tail of the window, release it
                        if (timestamp < windowTailTime)
                        {
                            if (postOldEvents == null)
                            {
                                postOldEvents = new List<EventBean>(2);
                            }
                            postOldEvents.Add(newEvent);
                        }
                        else
                        {
                            if (timestamp < oldestEvent)
                            {
                                addedOlderEvent = true;
                                oldestEvent = timestamp.Value;
                            }

                            // add to list
                            CollectionUtil.AddEventByKeyLazyListMapBack(timestamp, newEvent, _sortedEvents);
                            _eventCount++;
                            InternalHandleAdd(timestamp, newEvent);
                        }
                    }

                    // If we do have data, check the callback
                    if (_sortedEvents.IsNotEmpty())
                    {
                        // If we haven't scheduled a callback yet, schedule it now
                        if (!_isCallbackScheduled)
                        {
                            long callbackWait = oldestEvent - windowTailTime + 1;
                            _agentInstanceContext.StatementContext.SchedulingService.Add(
                                callbackWait, _handle, _scheduleSlot);
                            _isCallbackScheduled = true;
                        }
                        else
                        {
                            // We may need to reschedule, and older event may have been added
                            if (addedOlderEvent)
                            {
                                oldestEvent = (long)_sortedEvents.Keys.First();
                                long callbackWait = oldestEvent - windowTailTime + 1;
                                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                                _agentInstanceContext.StatementContext.SchedulingService.Add(
                                    callbackWait, _handle, _scheduleSlot);
                                _isCallbackScheduled = true;
                            }
                        }
                    }

                    if (postOldEvents != null)
                    {
                        postOldEventsArray = postOldEvents.ToArray();
                    }

                    if (_optionalSortedRandomAccess != null)
                    {
                        _optionalSortedRandomAccess.Refresh(_sortedEvents, _eventCount, _eventCount);
                    }
                }

                // Update child views
                if (HasViews)
                {
                    Instrument.With(
                        i => i.QViewIndicate(this, _viewFactory.ViewName, newData, postOldEventsArray),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(newData, postOldEventsArray));
                }
            }
        }

        public void InternalHandleAdd(Object sortValues, EventBean newDataItem)
        {
            // no action required
        }

        public void InternalHandleRemoved(Object sortValues, EventBean oldDataItem)
        {
            // no action required
        }

        public void InternalHandleExpired(Object sortValues, EventBean oldDataItem)
        {
            // no action required
        }

        public void InternalHandleExpired(Object sortValues, IList<EventBean> oldDataItems)
        {
            // no action required
        }

        protected long? GetTimestamp(EventBean newEvent)
        {
            _eventsPerStream[0] = newEvent;
            return
                (long?)_timestampEvaluator.Evaluate(new EvaluateParams(_eventsPerStream, true, _agentInstanceContext));
        }

        /// <summary>True to indicate the sort window is empty, or false if not empty. </summary>
        /// <returns>true if empty sort window</returns>
        public bool IsEmpty()
        {
            return _sortedEvents.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return new SortWindowEnumerator(_sortedEvents);
        }

        public override String ToString()
        {
            return GetType().FullName +
                   " timestampExpression=" + _timestampExpression;
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_sortedEvents, false, _viewFactory.ViewName, _eventCount, null);
        }

        /// <summary>
        /// This method removes (expires) objects from the window and schedules a new callback for the 
        /// time when the next oldest message would expire from the window.
        /// </summary>
        protected void Expire()
        {
            long currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            long expireBeforeTimestamp = currentTime - _timeDeltaComputation.DeltaSubtract(currentTime) + 1;
            _isCallbackScheduled = false;

            IList<EventBean> releaseEvents = null;
            long? oldestKey;
            while (true)
            {
                if (_sortedEvents.IsEmpty())
                {
                    oldestKey = null;
                    break;
                }

                oldestKey = (long)_sortedEvents.Keys.First();
                if (oldestKey >= expireBeforeTimestamp)
                {
                    break;
                }

                var released = _sortedEvents.Delete(oldestKey);
                if (released != null)
                {
                    if (released is IList<EventBean>)
                    {
                        var releasedEventList = (IList<EventBean>)released;
                        if (releaseEvents == null)
                        {
                            releaseEvents = releasedEventList;
                        }
                        else
                        {
                            releaseEvents.AddAll(releasedEventList);
                        }
                        _eventCount -= releasedEventList.Count;
                        InternalHandleExpired(oldestKey, releasedEventList);
                    }
                    else
                    {
                        var releasedEvent = (EventBean)released;
                        if (releaseEvents == null)
                        {
                            releaseEvents = new List<EventBean>(4);
                        }
                        releaseEvents.Add(releasedEvent);
                        _eventCount--;
                        InternalHandleExpired(oldestKey, releasedEvent);
                    }
                }
            }

            if (_optionalSortedRandomAccess != null)
            {
                _optionalSortedRandomAccess.Refresh(_sortedEvents, _eventCount, _eventCount);
            }

            // If there are child views, do the Update method
            if (HasViews)
            {
                if ((releaseEvents != null) && (releaseEvents.IsNotEmpty()))
                {
                    EventBean[] oldEvents = releaseEvents.ToArray();
                    Instrument.With(
                        i => i.QViewIndicate(this, _viewFactory.ViewName, null, oldEvents),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(null, oldEvents));
                }
            }

            // If we still have events in the window, schedule new callback
            if (oldestKey == null)
            {
                return;
            }

            // Next callback
            long callbackWait = oldestKey.Value - expireBeforeTimestamp + 1;
            _agentInstanceContext.StatementContext.SchedulingService.Add(callbackWait, _handle, _scheduleSlot);
            _isCallbackScheduled = true;
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

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
}
