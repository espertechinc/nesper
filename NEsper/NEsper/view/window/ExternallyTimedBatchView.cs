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
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Batch window based on timestamp of arriving events.
    /// </summary>
    public class ExternallyTimedBatchView
        : ViewSupport
        , DataWindowView
        , CloneableView
    {
        private readonly ExternallyTimedBatchViewFactory _factory;
        private readonly ExprNode _timestampExpression;
        private readonly ExprEvaluator _timestampExpressionEval;
        private readonly ExprTimePeriodEvalDeltaConst _timeDeltaComputation;

        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        internal EventBean[] LastBatch;

        private long? _oldestTimestamp;
        internal readonly ISet<EventBean> Window = new LinkedHashSet<EventBean>();
        internal long? ReferenceTimestamp;

        internal ViewUpdatedCollection ViewUpdatedCollection;
        internal AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">for copying this view in a group-by</param>
        /// <param name="timestampExpression">is the field name containing a long timestamp valuethat should be in ascending order for the natural order of events and is intended to reflect
        /// System.currentTimeInMillis but does not necessarily have to.
        /// out of the window as oldData in the update method. The view compares
        /// each events timestamp against the newest event timestamp and those with a delta
        /// greater then secondsBeforeExpiry are pushed out of the window.</param>
        /// <param name="timestampExpressionEval">The timestamp expression eval.</param>
        /// <param name="timeDeltaComputation">The time delta computation.</param>
        /// <param name="optionalReferencePoint">The optional reference point.</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must update when receiving events</param>
        /// <param name="agentInstanceViewFactoryContext">context for expression evalauation</param>
        public ExternallyTimedBatchView(ExternallyTimedBatchViewFactory factory,
                                        ExprNode timestampExpression,
                                        ExprEvaluator timestampExpressionEval,
                                        ExprTimePeriodEvalDeltaConst timeDeltaComputation,
                                        long? optionalReferencePoint,
                                        ViewUpdatedCollection viewUpdatedCollection,
                                        AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            _factory = factory;
            _timestampExpression = timestampExpression;
            _timestampExpressionEval = timestampExpressionEval;
            _timeDeltaComputation = timeDeltaComputation;
            ViewUpdatedCollection = viewUpdatedCollection;
            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            ReferenceTimestamp = optionalReferencePoint;
        }

        public View CloneView()
        {
            return _factory.MakeView(AgentInstanceViewFactoryContext);
        }

        /// <summary>
        /// Returns the field name to get timestamp values from.
        /// </summary>
        /// <value>field name for timestamp values</value>
        public ExprNode TimestampExpression => _timestampExpression;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _factory.ViewName, newData, oldData); }

            // remove points from data window
            if (oldData != null && oldData.Length != 0)
            {
                foreach (EventBean anOldData in oldData)
                {
                    Window.Remove(anOldData);
                    HandleInternalRemovedEvent(anOldData);
                }
                DetermineOldestTimestamp();
            }

            // add data points to the window
            EventBean[] batchNewData = null;
            if (newData != null)
            {
                foreach (EventBean newEvent in newData)
                {

                    long timestamp = GetLongValue(newEvent);
                    if (ReferenceTimestamp == null)
                    {
                        ReferenceTimestamp = timestamp;
                    }

                    if (_oldestTimestamp == null)
                    {
                        _oldestTimestamp = timestamp;
                    }
                    else
                    {
                        var delta = _timeDeltaComputation.DeltaAddWReference(
                            _oldestTimestamp.Value, ReferenceTimestamp.Value);
                        ReferenceTimestamp = delta.LastReference;
                        if (timestamp - _oldestTimestamp >= delta.Delta)
                        {
                            if (batchNewData == null)
                            {
                                batchNewData = Window.ToArray();
                            }
                            else
                            {
                                batchNewData = EventBeanUtility.AddToArray(batchNewData, Window);
                            }
                            Window.Clear();
                            _oldestTimestamp = null;
                        }
                    }

                    Window.Add(newEvent);
                    HandleInternalAddEvent(newEvent, batchNewData != null);
                }
            }

            if (batchNewData != null)
            {
                HandleInternalPostBatch(Window, batchNewData);
                if (ViewUpdatedCollection != null)
                {
                    ViewUpdatedCollection.Update(batchNewData, LastBatch);
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _factory.ViewName, newData, LastBatch); }
                UpdateChildren(batchNewData, LastBatch);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
                LastBatch = batchNewData;
                DetermineOldestTimestamp();
            }
            if (oldData != null && oldData.Length > 0)
            {
                if (ViewUpdatedCollection != null)
                {
                    ViewUpdatedCollection.Update(null, oldData);
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _factory.ViewName, null, oldData); }
                UpdateChildren(null, oldData);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Window.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName +
                    " timestampExpression=" + _timestampExpression;
        }
        /// <summary>
        /// Returns true to indicate the window is empty, or false if the view is not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return Window.IsEmpty();
        }

        public ExprTimePeriodEvalDeltaConst GetTimeDeltaComputation()
        {
            return _timeDeltaComputation;
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(Window, true, _factory.ViewName, null);
        }

        public ViewFactory ViewFactory => _factory;

        protected void DetermineOldestTimestamp()
        {
            if (Window.IsEmpty())
            {
                _oldestTimestamp = null;
            }
            else
            {
                _oldestTimestamp = GetLongValue(Window.First());
            }
        }

        protected void HandleInternalPostBatch(ISet<EventBean> window, EventBean[] batchNewData)
        {
            // no action require
        }

        protected void HandleInternalRemovedEvent(EventBean anOldData)
        {
            // no action require
        }

        protected void HandleInternalAddEvent(EventBean anNewData, bool isNextBatch)
        {
            // no action require
        }

        private long GetLongValue(EventBean obj)
        {
            _eventsPerStream[0] = obj;
            var num = _timestampExpressionEval.Evaluate(
                new EvaluateParams(_eventsPerStream, true, AgentInstanceViewFactoryContext));
            return num.AsLong();
        }
    }
}
