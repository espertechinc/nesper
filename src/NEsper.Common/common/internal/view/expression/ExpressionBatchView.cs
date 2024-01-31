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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    ///     This view is a moving window extending the into the past until the expression passed to it returns false.
    /// </summary>
    public class ExpressionBatchView : ExpressionViewBase
    {
        internal readonly ISet<EventBean> window = new LinkedHashSet<EventBean>();
        internal EventBean[] lastBatch;
        internal EventBean newestEvent;
        internal long newestEventTimestamp;
        internal EventBean oldestEvent;
        internal long oldestEventTimestamp;

        public ExpressionBatchView(
            ExpressionBatchViewFactory factory,
            ViewUpdatedCollection viewUpdatedCollection,
            ObjectArrayEventBean builtinEventProps,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
            : base(factory, viewUpdatedCollection, builtinEventProps, agentInstanceContext)
        {
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => window.IsEmpty();

        public override void ScheduleCallback()
        {
            var fireBatch = EvaluateExpression(null, window.Count);
            if (fireBatch) {
                Expire(window.Count);
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            var fireBatch = false;

            // remove points from data window
            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    window.Remove(anOldData);
                }

                aggregationService?.ApplyLeave(oldData, null, agentInstanceContext);

                if (!window.IsEmpty()) {
                    oldestEvent = window.First();
                }
                else {
                    oldestEvent = null;
                }

                fireBatch = EvaluateExpression(null, window.Count);
            }

            // add data points to the window
            var numEventsInBatch = -1;
            if (newData != null && newData.Length > 0) {
                if (window.IsEmpty()) {
                    oldestEventTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
                }

                newestEventTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
                if (oldestEvent == null) {
                    oldestEvent = newData[0];
                }

                foreach (var newEvent in newData) {
                    window.Add(newEvent);
                    aggregationService?.ApplyEnter(new[] { newEvent }, null, agentInstanceContext);

                    newestEvent = newEvent;
                    if (!fireBatch) {
                        fireBatch = EvaluateExpression(newEvent, window.Count);
                        if (fireBatch && !((ExpressionBatchViewFactory)factory).IsIncludeTriggeringEvent) {
                            numEventsInBatch = window.Count - 1;
                        }
                    }
                }
            }

            // may fire the batch
            if (fireBatch) {
                Expire(numEventsInBatch);
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        // Called based on schedule evaluation registered when a variable changes (new data is null).
        // Called when new data arrives.
        public void Expire(int numEventsInBatch)
        {
            if (numEventsInBatch == window.Count || numEventsInBatch == -1) {
                var batchNewData = window.ToArrayOrNull();
                viewUpdatedCollection?.Update(batchNewData, lastBatch);

                // post
                if (batchNewData != null || lastBatch != null) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, batchNewData, lastBatch);
                    Child.Update(batchNewData, lastBatch);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }

                // clear
                window.Clear();
                lastBatch = batchNewData;
                aggregationService?.ClearResults(agentInstanceContext);

                oldestEvent = null;
                newestEvent = null;
            }
            else {
                var batchNewData = new EventBean[numEventsInBatch];
                var itemsInBatch = window.Take(batchNewData.Length).ToList();
                for (var ii = 0; ii < itemsInBatch.Count; ii++) {
                    batchNewData[ii] = itemsInBatch[ii];
                    window.Remove(itemsInBatch[ii]);
                }

                viewUpdatedCollection?.Update(batchNewData, lastBatch);

                // post
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, batchNewData, lastBatch);
                Child.Update(batchNewData, lastBatch);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                // clear
                lastBatch = batchNewData;
                aggregationService?.ApplyLeave(batchNewData, null, agentInstanceContext);
                oldestEvent = window.First();
            }
        }

        public override void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(window, true, factory.ViewName, null);
            viewDataVisitor.VisitPrimary(lastBatch, factory.ViewName);
        }

        private bool EvaluateExpression(
            EventBean arriving,
            int windowSize)
        {
            ExpressionViewOAFieldEnumExtensions.Populate(
                builtinEventProps.Properties,
                windowSize,
                oldestEventTimestamp,
                newestEventTimestamp,
                this,
                0,
                oldestEvent,
                newestEvent);
            eventsPerStream[0] = arriving;
            return ExpressionBatchViewUtil.Evaluate(eventsPerStream, agentInstanceContext, factory, aggregationService);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return window.GetEnumerator();
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
    }
} // end of namespace