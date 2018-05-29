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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    ///     View for a moving window extending the specified amount of time into the past, driven entirely by external timing
    ///     supplied within long-type timestamp values in a field of the event beans that the view receives.
    ///     The view is completely driven by timestamp values that are supplied by the events it receives,
    ///     and does not use the schedule service time.
    ///     It requires a field name as parameter for a field that returns ascending long-type timestamp values.
    ///     It also requires a long-type parameter setting the time length in milliseconds of the time window.
    ///     Events are expected to provide long-type timestamp values in natural order. The view does
    ///     itself not use the current system time for keeping track of the time window, but just the
    ///     timestamp values supplied by the events sent in.
    ///     The arrival of new events with a newer timestamp then past events causes the window to be re-evaluated and the
    ///     oldest
    ///     events pushed out of the window. Ie. Assume event X1 with timestamp T1 is in the window.
    ///     When event Xn with timestamp Tn arrives, and the window time length in milliseconds is t, then if
    ///     ((Tn - T1) &gt; t == true) then event X1 is pushed as oldData out of the window. It is assumed that
    ///     events are sent in in their natural order and the timestamp values are ascending.
    /// </summary>
    public class ExternallyTimedWindowView
        : ViewSupport
        , DataWindowView
        , CloneableView
    {
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly ExternallyTimedWindowViewFactory _externallyTimedWindowViewFactory;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;

        private readonly TimeWindow _timeWindow;
        private readonly ExprNode _timestampExpression;
        private readonly ExprEvaluator _timestampExpressionEval;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        internal AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="externallyTimedWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="timestampExpression">is the field name containing a long timestamp valuethat should be in ascending order for the natural order of
        /// events and is intended to reflect
        /// System.currentTimeInMillis but does not necessarily have to.
        /// out of the window as oldData in the update method. The view compares
        /// each events timestamp against the newest event timestamp and those with a delta
        /// greater then secondsBeforeExpiry are pushed out of the window.</param>
        /// <param name="timestampExpressionEval">The timestamp expression eval.</param>
        /// <param name="timeDeltaComputation">The time delta computation.</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must update when receiving events</param>
        /// <param name="agentInstanceViewFactoryContext">context for expression evalauation</param>
        public ExternallyTimedWindowView(
            ExternallyTimedWindowViewFactory externallyTimedWindowViewFactory,
            ExprNode timestampExpression,
            ExprEvaluator timestampExpressionEval,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            ViewUpdatedCollection viewUpdatedCollection,
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            _externallyTimedWindowViewFactory = externallyTimedWindowViewFactory;
            _timestampExpression = timestampExpression;
            _timestampExpressionEval = timestampExpressionEval;
            _timeDeltaComputation = timeDeltaComputation;
            _viewUpdatedCollection = viewUpdatedCollection;
            _timeWindow = new TimeWindow(agentInstanceViewFactoryContext.IsRemoveStream);
            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
        }

        /// <summary>
        ///     Returns the field name to get timestamp values from.
        /// </summary>
        /// <value>field name for timestamp values</value>
        public ExprNode TimestampExpression => _timestampExpression;

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation => _timeDeltaComputation;

        public View CloneView()
        {
            return _externallyTimedWindowViewFactory.MakeView(AgentInstanceViewFactoryContext);
        }

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get()
                    .QViewProcessIRStream(this, _externallyTimedWindowViewFactory.ViewName, newData, oldData);
            }
            long timestamp = -1;

            // add data points to the window
            // we don't care about removed data from a prior view
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    timestamp = GetLongValue(newData[i]);
                    _timeWindow.Add(timestamp, newData[i]);
                }
            }

            // Remove from the window any events that have an older timestamp then the last event's timestamp
            ArrayDeque<EventBean> expired = null;
            if (timestamp != -1)
            {
                expired =
                    _timeWindow.ExpireEvents(timestamp - _timeDeltaComputation.DeltaSubtract(timestamp) + 1);
            }

            EventBean[] oldDataUpdate = null;
            if ((expired != null) && (!expired.IsEmpty()))
            {
                oldDataUpdate = expired.ToArray();
            }

            if ((oldData != null) && (AgentInstanceViewFactoryContext.IsRemoveStream))
            {
                foreach (EventBean anOldData in oldData)
                {
                    _timeWindow.Remove(anOldData);
                }

                if (oldDataUpdate == null)
                {
                    oldDataUpdate = oldData;
                }
                else
                {
                    oldDataUpdate = CollectionUtil.AddArrayWithSetSemantics(oldData, oldDataUpdate);
                }
            }

            if (_viewUpdatedCollection != null)
            {
                _viewUpdatedCollection.Update(newData, oldDataUpdate);
            }

            // If there are child views, fireStatementStopped update method
            if (HasViews)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _externallyTimedWindowViewFactory.ViewName, newData, oldDataUpdate); }
                UpdateChildren(newData, oldDataUpdate);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _timeWindow.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            _timeWindow.VisitView(viewDataVisitor, _externallyTimedWindowViewFactory);
        }

        public override String ToString()
        {
            return GetType().FullName +
                   " timestampExpression=" + _timestampExpression;
        }

        private long GetLongValue(EventBean obj)
        {
            _eventsPerStream[0] = obj;
            var num = _timestampExpressionEval.Evaluate(new EvaluateParams(_eventsPerStream, true, AgentInstanceViewFactoryContext));
            return num.AsLong();
        }

        /// <summary>
        ///     Returns true to indicate the window is empty, or false if the view is not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _timeWindow.IsEmpty();
        }

        public ViewUpdatedCollection GetViewUpdatedCollection()
        {
            return _viewUpdatedCollection;
        }

        public ViewFactory ViewFactory => _externallyTimedWindowViewFactory;
    }
}