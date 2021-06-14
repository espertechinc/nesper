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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    ///     This view is a moving window extending the into the past until the expression passed to it returns false.
    /// </summary>
    public class ExpressionWindowView : ExpressionViewBase
    {
        private readonly EventBean[] removedEvents = new EventBean[1];

        internal readonly ArrayDeque<ExpressionWindowTimestampEventPair> window =
            new ArrayDeque<ExpressionWindowTimestampEventPair>();

        public ExpressionWindowView(
            ExpressionWindowViewFactory factory,
            ViewUpdatedCollection viewUpdatedCollection,
            ObjectArrayEventBean builtinEventProps,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
            : base(
                factory,
                viewUpdatedCollection,
                builtinEventProps,
                agentInstanceContext)
        {
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => window.IsEmpty();

        public ArrayDeque<ExpressionWindowTimestampEventPair> Window => window;

        public override void ScheduleCallback()
        {
            Expire(null, null);
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            // add data points to the window
            if (newData != null) {
                foreach (var newEvent in newData) {
                    var pair = new ExpressionWindowTimestampEventPair(agentInstanceContext.TimeProvider.Time, newEvent);
                    window.Add(pair);
                    InternalHandleAdd(pair);
                }

                aggregationService?.ApplyEnter(newData, null, agentInstanceContext);
            }

            if (oldData != null) {
                window.RemoveWhere(
                    pair => oldData.Any(anOldData => pair.TheEvent == anOldData),
                    InternalHandleRemoved);

                aggregationService?.ApplyLeave(oldData, null, agentInstanceContext);
            }

            // expire events
            Expire(newData, oldData);
            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public void InternalHandleRemoved(ExpressionWindowTimestampEventPair pair)
        {
            // no action required
        }

        public void InternalHandleExpired(ExpressionWindowTimestampEventPair pair)
        {
            // no action required
        }

        public void InternalHandleAdd(ExpressionWindowTimestampEventPair pair)
        {
            // no action required
        }

        // Called based on schedule evaluation registered when a variable changes (new data is null).
        // Called when new data arrives.
        private void Expire(
            EventBean[] newData,
            EventBean[] oldData)
        {
            OneEventCollection expired = null;
            if (oldData != null) {
                expired = new OneEventCollection();
                expired.Add(oldData);
            }

            var expiredCount = 0;
            if (!window.IsEmpty()) {
                var newest = window.Last;

                while (true) {
                    var first = window.First;

                    var pass = CheckEvent(first, newest, expiredCount);
                    if (!pass) {
                        if (expired == null) {
                            expired = new OneEventCollection();
                        }

                        var removed = window.RemoveFirst().TheEvent;
                        expired.Add(removed);
                        if (aggregationService != null) {
                            removedEvents[0] = removed;
                            aggregationService.ApplyLeave(removedEvents, null, agentInstanceContext);
                        }

                        expiredCount++;
                        InternalHandleExpired(first);
                    }
                    else {
                        break;
                    }

                    if (window.IsEmpty()) {
                        aggregationService?.ClearResults(agentInstanceContext);

                        break;
                    }
                }
            }

            // Check for any events that get pushed out of the window
            EventBean[] expiredArr = null;
            if (expired != null) {
                expiredArr = expired.ToArray();
            }

            // update event buffer for access by expressions, if any
            viewUpdatedCollection?.Update(newData, expiredArr);

            // If there are child views, call update method
            if (child != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, expiredArr);
                child.Update(newData, expiredArr);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }
        }

        private bool CheckEvent(
            ExpressionWindowTimestampEventPair first,
            ExpressionWindowTimestampEventPair newest,
            int numExpired)
        {
            ExpressionViewOAFieldEnumExtensions.Populate(
                builtinEventProps.Properties,
                window.Count,
                first.Timestamp,
                newest.Timestamp,
                this,
                numExpired,
                first.TheEvent,
                newest.TheEvent);
            eventsPerStream[0] = first.TheEvent;
            return ExpressionBatchViewUtil.Evaluate(eventsPerStream, agentInstanceContext, factory, aggregationService);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return ExpressionWindowTimestampEventPairEnumerator.Create(window.GetEnumerator());
        }

        // Handle variable updates by scheduling a re-evaluation with timers
        public override void Update(
            object newValue,
            object oldValue)
        {
            if (!agentInstanceContext.StatementContext.SchedulingService.IsScheduled(scheduleHandle)) {
                agentInstanceContext.AuditProvider.ScheduleAdd(
                    0,
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.view,
                    factory.ViewName);
                agentInstanceContext.StatementContext.SchedulingService.Add(0, scheduleHandle, scheduleSlot);
            }
        }

        public override void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(window, true, factory.ViewName, null);
        }
    }
} // end of namespace