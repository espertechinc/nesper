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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// This view is a moving window extending the into the past until the expression passed to it returns false.
    /// </summary>
    public class ExpressionBatchView : ExpressionViewBase
    {
        private readonly ExpressionBatchViewFactory _dataWindowViewFactory;

        protected readonly ICollection<EventBean> Window = new LinkedHashSet<EventBean>();

        protected EventBean[] LastBatch;
        protected long NewestEventTimestamp;
        protected long OldestEventTimestamp;
        protected EventBean OldestEvent;
        protected EventBean NewestEvent;

        /// <summary>
        /// Constructor creates a moving window extending the specified number of elements into the past.
        /// </summary>
        /// <param name="dataWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must Update when receiving events</param>
        /// <param name="expiryExpression">The expiry expression.</param>
        /// <param name="aggregationServiceFactoryDesc">The aggregation service factory desc.</param>
        /// <param name="builtinEventProps">The builtin event props.</param>
        /// <param name="variableNames">variable names</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        public ExpressionBatchView(ExpressionBatchViewFactory dataWindowViewFactory,
                                   ViewUpdatedCollection viewUpdatedCollection,
                                   ExprEvaluator expiryExpression,
                                   AggregationServiceFactoryDesc aggregationServiceFactoryDesc,
                                   ObjectArrayEventBean builtinEventProps,
                                   ISet<String> variableNames,
                                   AgentInstanceViewFactoryChainContext agentInstanceContext)
            : base(viewUpdatedCollection, expiryExpression, aggregationServiceFactoryDesc, builtinEventProps, variableNames, agentInstanceContext)
        {
            this._dataWindowViewFactory = dataWindowViewFactory;
        }

        public override string ViewName => _dataWindowViewFactory.ViewName;

        public override View CloneView()
        {
            return _dataWindowViewFactory.MakeView(AgentInstanceContext);
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return Window.IsEmpty();
        }

        public override void ScheduleCallback()
        {
            bool fireBatch = EvaluateExpression(null, Window.Count);
            if (fireBatch)
            {
                Expire(Window.Count);
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _dataWindowViewFactory.ViewName, newData, oldData); }

            bool fireBatch = false;

            // remove points from data window
            if (oldData != null)
            {
                foreach (EventBean anOldData in oldData)
                {
                    Window.Remove(anOldData);
                }
                if (AggregationService != null)
                {
                    AggregationService.ApplyLeave(oldData, null, AgentInstanceContext);
                }

                if (Window.IsNotEmpty())
                {
                    OldestEvent = Window.First();
                }
                else
                {
                    OldestEvent = null;
                }

                fireBatch = EvaluateExpression(null, Window.Count);
            }

            // add data points to the window
            int numEventsInBatch = -1;
            if (newData != null && newData.Length > 0)
            {
                if (Window.IsEmpty())
                {
                    OldestEventTimestamp = AgentInstanceContext.StatementContext.SchedulingService.Time;
                }
                NewestEventTimestamp = AgentInstanceContext.StatementContext.SchedulingService.Time;
                if (OldestEvent == null)
                {
                    OldestEvent = newData[0];
                }

                foreach (EventBean newEvent in newData)
                {
                    Window.Add(newEvent);
                    if (AggregationService != null)
                    {
                        AggregationService.ApplyEnter(new EventBean[] { newEvent }, null, AgentInstanceContext);
                    }
                    NewestEvent = newEvent;
                    if (!fireBatch)
                    {
                        fireBatch = EvaluateExpression(newEvent, Window.Count);
                        if (fireBatch && !_dataWindowViewFactory.IsIncludeTriggeringEvent)
                        {
                            numEventsInBatch = Window.Count - 1;
                        }
                    }
                }
            }

            // may fire the batch
            if (fireBatch)
            {
                Expire(numEventsInBatch);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        // Called based on schedule evaluation registered when a variable changes (new data is null).
        // Called when new data arrives.
        public void Expire(int numEventsInBatch)
        {

            if (numEventsInBatch == Window.Count || numEventsInBatch == -1)
            {
                var batchNewData = Window.ToArray();
                if (ViewUpdatedCollection != null)
                {
                    ViewUpdatedCollection.Update(batchNewData, LastBatch);
                }

                // post
                if (batchNewData != null || LastBatch != null)
                {
                    Instrument.With(
                        i => i.QViewIndicate(this, _dataWindowViewFactory.ViewName, batchNewData, LastBatch),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(batchNewData, LastBatch));
                }

                // clear
                Window.Clear();
                LastBatch = batchNewData;
                if (AggregationService != null)
                {
                    AggregationService.ClearResults(AgentInstanceContext);
                }
                OldestEvent = null;
                NewestEvent = null;
            }
            else
            {
                var batchNewData = Window.Take(numEventsInBatch).ToArray();
                unchecked
                {
                    for (int ii = 0; ii < batchNewData.Length; ii++)
                    {
                        Window.Remove(batchNewData[ii]);
                    }
                }

                if (ViewUpdatedCollection != null)
                {
                    ViewUpdatedCollection.Update(batchNewData, LastBatch);
                }

                // post
                if (batchNewData != null || LastBatch != null)
                {
                    Instrument.With(
                        i => i.QViewIndicate(this, _dataWindowViewFactory.ViewName, batchNewData, LastBatch),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(batchNewData, LastBatch));
                }

                // clear
                LastBatch = batchNewData;
                if (AggregationService != null)
                {
                    AggregationService.ApplyLeave(batchNewData, null, AgentInstanceContext);
                }
                OldestEvent = Window.FirstOrDefault();
            }
        }

        public override void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(Window, true, _dataWindowViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(LastBatch, _dataWindowViewFactory.ViewName);
        }

        private bool EvaluateExpression(EventBean arriving, int windowSize)
        {
            ExpressionViewOAFieldEnumExtensions.Populate(BuiltinEventProps.Properties, windowSize, OldestEventTimestamp, NewestEventTimestamp, this, 0, OldestEvent, NewestEvent);
            EventsPerStream[0] = arriving;

            foreach (AggregationServiceAggExpressionDesc aggregateNode in AggregateNodes)
            {
                aggregateNode.AssignFuture(AggregationService);
            }

            var result = ExpiryExpression.Evaluate(new EvaluateParams(EventsPerStream, true, AgentInstanceContext));
            if (result == null)
            {
                return false;
            }

            return result.AsBoolean();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Window.GetEnumerator();
        }

        // Handle variable updates by scheduling a re-evaluation with timers
        public override void Update(Object newValue, Object oldValue)
        {
            if (!AgentInstanceContext.StatementContext.SchedulingService.IsScheduled(ScheduleHandle))
            {
                AgentInstanceContext.StatementContext.SchedulingService.Add(0, ScheduleHandle, ScheduleSlot);
            }
        }

        public override ViewFactory ViewFactory => _dataWindowViewFactory;
    }
}
