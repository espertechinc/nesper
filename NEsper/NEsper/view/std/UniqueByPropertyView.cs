///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// This view includes only the most recent among events having the same value for the specified field or fields.
    /// The view accepts the field name as parameter from which the unique values are obtained.
    /// For example, a trade's symbol could be used as a unique value.
    /// In this example, the first trade for symbol IBM would be posted as new data to child views.
    /// When the second trade for symbol IBM arrives the second trade is posted as new data to child views,
    /// and the first trade is posted as old data.
    /// Should more than one trades for symbol IBM arrive at the same time (like when batched)
    /// then the child view will get all new events in newData and all new events in oldData minus the most recent event.
    /// When the current new event arrives as old data, the the current unique event gets thrown away and
    /// posted as old data to child views.
    /// Iteration through the views data shows only the most recent events received for the unique value in the order
    /// that events arrived in.
    /// The type of the field returning the unique value can be any type but should override equals and hashCode()
    /// as the type plays the role of a key in a map storing unique values.
    /// </summary>
    public class UniqueByPropertyView
        : ViewSupport
        , CloneableView
        , DataWindowView
    {
        private readonly UniqueByPropertyViewFactory _viewFactory;
        private readonly ExprEvaluator[] _criteriaExpressionsEvals;
        private readonly IDictionary<object, EventBean> _mostRecentEvents = new NullableDictionary<object, EventBean>();
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="viewFactory">The view factory.</param>
        /// <param name="agentInstanceViewFactoryContext">context for expression evaluation</param>
        public UniqueByPropertyView(UniqueByPropertyViewFactory viewFactory, AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            _viewFactory = viewFactory;
            _criteriaExpressionsEvals = ExprNodeUtility.GetEvaluators(viewFactory.CriteriaExpressions);
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
        }

        public View CloneView()
        {
            return new UniqueByPropertyView(_viewFactory, _agentInstanceViewFactoryContext);
        }

        /// <summary>
        /// Returns the name of the field supplying the unique value to keep the most recent record for.
        /// </summary>
        /// <value>expressions for unique value</value>
        public ExprNode[] CriteriaExpressions
        {
            get { return _viewFactory.CriteriaExpressions; }
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, UniqueByPropertyViewFactory.NAME, newData, oldData); }
            OneEventCollection postOldData = null;

            if (HasViews)
            {
                postOldData = new OneEventCollection();
            }

            if (newData != null)
            {
                for (var i = 0; i < newData.Length; i++)
                {
                    // Obtain unique value
                    var key = GetUniqueKey(newData[i]);

                    // If there are no child views, just update the own collection
                    if (!HasViews)
                    {
                        _mostRecentEvents.Put(key, newData[i]);
                        continue;
                    }

                    // Post the last value as old data
                    var lastValue = _mostRecentEvents.Get(key);
                    if (lastValue != null)
                    {
                        postOldData.Add(lastValue);
                    }

                    // Override with recent event
                    _mostRecentEvents.Put(key, newData[i]);
                }
            }

            if (oldData != null)
            {
                for (var i = 0; i < oldData.Length; i++)
                {
                    // Obtain unique value
                    var key = GetUniqueKey(oldData[i]);

                    // If the old event is the current unique event, remove and post as old data
                    var lastValue = _mostRecentEvents.Get(key);
                    if (lastValue == null || !lastValue.Equals(oldData[i]))
                    {
                        continue;
                    }

                    postOldData.Add(lastValue);
                    _mostRecentEvents.Remove(key);
                }
            }

            // If there are child views, fireStatementStopped update method
            if (HasViews)
            {
                if (postOldData.IsEmpty())
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, UniqueByPropertyViewFactory.NAME, newData, null); }
                    UpdateChildren(newData, null);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
                }
                else
                {
                    var postOldDataArray = postOldData.ToArray();
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, UniqueByPropertyViewFactory.NAME, newData, postOldDataArray); }
                    UpdateChildren(newData, postOldDataArray);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        /// <summary>
        /// Returns true if the view is empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _mostRecentEvents.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _mostRecentEvents.Values.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name + " uniqueFieldNames=" + _viewFactory.CriteriaExpressions;
        }

        protected object GetUniqueKey(EventBean theEvent)
        {
            var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceViewFactoryContext);

            _eventsPerStream[0] = theEvent;
            if (_criteriaExpressionsEvals.Length == 1)
            {
                return _criteriaExpressionsEvals[0].Evaluate(evaluateParams);
            }

            var values = new object[_criteriaExpressionsEvals.Length];
            for (var i = 0; i < _criteriaExpressionsEvals.Length; i++)
            {
                values[i] = _criteriaExpressionsEvals[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(values);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_mostRecentEvents, true, UniqueByPropertyViewFactory.NAME, _mostRecentEvents.Count, _mostRecentEvents.Count);
        }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
} // end of namespace
