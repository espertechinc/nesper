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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// This view is a moving window extending the into the past until the expression passed to it returns false.
    /// </summary>
    public class ExpressionWindowView : ExpressionViewBase
    {
        private readonly ExpressionWindowViewFactory _dataWindowViewFactory;
        private readonly ArrayDeque<ExpressionWindowTimestampEventPair> _window =
            new ArrayDeque<ExpressionWindowTimestampEventPair>();
        private readonly EventBean[] _removedEvents = new EventBean[1];

        /// <summary>
        /// Constructor creates a moving window extending the specified number of elements into the past.
        /// </summary>
        /// <param name="dataWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must update when receiving events</param>
        /// <param name="expiryExpression">The expiry expression.</param>
        /// <param name="AggregationServiceFactoryDesc">The aggregation service factory desc.</param>
        /// <param name="builtinEventProps">The builtin event props.</param>
        /// <param name="variableNames">variable names</param>
        /// <param name="AgentInstanceContext">The agent instance context.</param>
        public ExpressionWindowView(ExpressionWindowViewFactory dataWindowViewFactory,
                                    ViewUpdatedCollection viewUpdatedCollection,
                                    ExprEvaluator expiryExpression,
                                    AggregationServiceFactoryDesc AggregationServiceFactoryDesc,
                                    ObjectArrayEventBean builtinEventProps,
                                    ISet<String> variableNames,
                                    AgentInstanceViewFactoryChainContext AgentInstanceContext)
            : base(viewUpdatedCollection, expiryExpression, AggregationServiceFactoryDesc, builtinEventProps, variableNames, AgentInstanceContext)
        {
            _dataWindowViewFactory = dataWindowViewFactory;
        }

        public override string ViewName => _dataWindowViewFactory.ViewName;

        public override View CloneView()
        {
            return _dataWindowViewFactory.MakeView(AgentInstanceContext);
        }

        /// <summary>
        /// Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _window.IsEmpty();
        }

        public override void ScheduleCallback()
        {
            Expire(null, null);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _dataWindowViewFactory.ViewName, newData, oldData); }

            // add data points to the window
            if (newData != null)
            {
                foreach (EventBean newEvent in newData)
                {
                    var pair = new ExpressionWindowTimestampEventPair(AgentInstanceContext.TimeProvider.Time, newEvent);
                    _window.Add(pair);
                    InternalHandleAdd(pair);
                }

                if (AggregationService != null)
                {
                    AggregationService.ApplyEnter(newData, null, AgentInstanceContext);
                }
            }

            if (oldData != null)
            {
                _window.RemoveWhere(
                   pair => oldData.Any(anOldData => pair.TheEvent == anOldData),
                   pair => InternalHandleRemoved(pair));

                if (AggregationService != null)
                {
                    AggregationService.ApplyLeave(oldData, null, AgentInstanceContext);
                }
            }

            // expire events
            Expire(newData, oldData);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
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
        private void Expire(EventBean[] newData, EventBean[] oldData)
        {

            OneEventCollection expired = null;
            if (oldData != null)
            {
                expired = new OneEventCollection();
                expired.Add(oldData);
            }
            int expiredCount = 0;
            if (!_window.IsEmpty())
            {
                ExpressionWindowTimestampEventPair newest = _window.Last;

                while (true)
                {
                    ExpressionWindowTimestampEventPair first = _window.First;

                    bool pass = CheckEvent(first, newest, expiredCount);
                    if (!pass)
                    {
                        if (expired == null)
                        {
                            expired = new OneEventCollection();
                        }
                        EventBean removed = _window.RemoveFirst().TheEvent;
                        expired.Add(removed);
                        if (AggregationService != null)
                        {
                            _removedEvents[0] = removed;
                            AggregationService.ApplyLeave(_removedEvents, null, AgentInstanceContext);
                        }
                        expiredCount++;
                        InternalHandleExpired(first);
                    }
                    else
                    {
                        break;
                    }

                    if (_window.IsEmpty())
                    {
                        if (AggregationService != null)
                        {
                            AggregationService.ClearResults(AgentInstanceContext);
                        }
                        break;
                    }
                }
            }

            // Check for any events that get pushed out of the window
            EventBean[] expiredArr = null;
            if (expired != null)
            {
                expiredArr = expired.ToArray();
            }

            // update event buffer for access by expressions, if any
            if (ViewUpdatedCollection != null)
            {
                ViewUpdatedCollection.Update(newData, expiredArr);
            }

            // If there are child views, call update method
            if (HasViews)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _dataWindowViewFactory.ViewName, newData, expiredArr); }
                UpdateChildren(newData, expiredArr);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
            }
        }

        private bool CheckEvent(ExpressionWindowTimestampEventPair first, ExpressionWindowTimestampEventPair newest, int numExpired)
        {
            ExpressionViewOAFieldEnumExtensions.Populate(BuiltinEventProps.Properties, _window.Count, first.Timestamp, newest.Timestamp,
                    this, numExpired, first.TheEvent, newest.TheEvent);
            EventsPerStream[0] = first.TheEvent;

            foreach (AggregationServiceAggExpressionDesc aggregateNode in AggregateNodes)
            {
                aggregateNode.AssignFuture(AggregationService);
            }

            var result = ExpiryExpression.Evaluate(new EvaluateParams(EventsPerStream, true, AgentInstanceContext));
            if (result == null)
            {
                return false;
            }
            return true.Equals(result);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return new ExpressionWindowTimestampEventPairEnumerator(
                _window.GetEnumerator());
        }

        // Handle variable updates by scheduling a re-evaluation with timers
        public override void Update(Object newValue, Object oldValue)
        {
            if (!AgentInstanceContext.StatementContext.SchedulingService.IsScheduled(ScheduleHandle))
            {
                AgentInstanceContext.StatementContext.SchedulingService.Add(0, ScheduleHandle, ScheduleSlot);
            }
        }

        public ArrayDeque<ExpressionWindowTimestampEventPair> Window => _window;

        public override void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_window, true, _dataWindowViewFactory.ViewName, null);
        }

        public override ViewFactory ViewFactory => _dataWindowViewFactory;
    }
}
