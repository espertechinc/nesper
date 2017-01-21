///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.view.std
{
	/// <summary>
	/// This view retains the first event for each multi-key of distinct property values.
	/// <para />The view does not post a remove stream unless explicitly deleted from.
	/// <para />The view swallows any insert stream events that provide no new distinct set of property values.
	/// </summary>
	public class FirstUniqueByPropertyView : ViewSupport , CloneableView, DataWindowView
	{
	    private readonly FirstUniqueByPropertyViewFactory _viewFactory;
	    private readonly ExprEvaluator[] _uniqueCriteriaEval;
	    private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly IDictionary<object, EventBean> _firstEvents = new Dictionary<object, EventBean>();
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;

	    /// <summary>
	    /// Constructor.
	    /// </summary>
	    public FirstUniqueByPropertyView(FirstUniqueByPropertyViewFactory viewFactory, AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
	    {
	        _viewFactory = viewFactory;
	        _uniqueCriteriaEval = ExprNodeUtility.GetEvaluators(viewFactory.CriteriaExpressions);
	        _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
	    }

	    public View CloneView()
	    {
	        return _viewFactory.MakeView(_agentInstanceViewFactoryContext);
	    }

	    /// <summary>
	    /// Returns the expressions supplying the unique value to keep the most recent record for.
	    /// </summary>
	    /// <value>expressions for unique value</value>
	    public ExprNode[] UniqueCriteria
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
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, FirstUniqueByPropertyViewFactory.NAME, newData, oldData);}

	        EventBean[] newDataToPost = null;
	        EventBean[] oldDataToPost = null;

	        if (oldData != null)
	        {
	            foreach (EventBean oldEvent in oldData)
	            {
	                // Obtain unique value
	                object key = GetUniqueKey(oldEvent);

	                // If the old event is the current unique event, remove and post as old data
	                EventBean lastValue = _firstEvents.Get(key);

	                if (lastValue != oldEvent)
	                {
	                    continue;
	                }

	                if (oldDataToPost == null)
	                {
	                    oldDataToPost = new EventBean[]{oldEvent};
	                }
	                else
	                {
	                    oldDataToPost = EventBeanUtility.AddToArray(oldDataToPost, oldEvent);
	                }

	                _firstEvents.Remove(key);
	                InternalHandleRemoved(key, lastValue);
	            }
	        }

	        if (newData != null)
	        {
	            foreach (EventBean newEvent in newData)
	            {
	                // Obtain unique value
	                object key = GetUniqueKey(newEvent);

	                // already-seen key
	                if (_firstEvents.ContainsKey(key))
	                {
	                    continue;
	                }

	                // store
	                _firstEvents.Put(key, newEvent);
	                InternalHandleAdded(key, newEvent);

	                // Post the new value
	                if (newDataToPost == null)
	                {
	                    newDataToPost = new EventBean[]{newEvent};
	                }
	                else
	                {
	                    newDataToPost = EventBeanUtility.AddToArray(newDataToPost, newEvent);
	                }
	            }
	        }

	        if ((HasViews) && ((newDataToPost != null) || (oldDataToPost != null)))
	        {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, FirstUniqueByPropertyViewFactory.NAME, newDataToPost, oldDataToPost);}
	            UpdateChildren(newDataToPost, oldDataToPost);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
	        }

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
	    }

	    public void InternalHandleRemoved(object key, EventBean lastValue) {
	        // no action required
	    }

	    public void InternalHandleAdded(object key, EventBean newEvent) {
	        // no action required
	    }

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        return _firstEvents.Values.GetEnumerator();
	    }

	    public override string ToString()
	    {
	        return GetType().Name + " uniqueCriteria=" + _viewFactory.CriteriaExpressions;
	    }

	    protected object GetUniqueKey(EventBean theEvent)
	    {
            var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceViewFactoryContext);
            
            _eventsPerStream[0] = theEvent;
	        if (_uniqueCriteriaEval.Length == 1) {
	            return _uniqueCriteriaEval[0].Evaluate(evaluateParams);
	        }

	        var values = new object[_uniqueCriteriaEval.Length];
	        for (int i = 0; i < _uniqueCriteriaEval.Length; i++)
	        {
	            values[i] = _uniqueCriteriaEval[i].Evaluate(evaluateParams);
	        }
	        return new MultiKeyUntyped(values);
	    }

	    /// <summary>
	    /// Returns true if empty.
	    /// </summary>
	    /// <returns>true if empty</returns>
	    public bool IsEmpty()
	    {
	        return _firstEvents.IsEmpty();
	    }

	    public void VisitView(ViewDataVisitor viewDataVisitor) {
	        viewDataVisitor.VisitPrimary(_firstEvents, true, FirstUniqueByPropertyViewFactory.NAME, _firstEvents.Count, _firstEvents.Count);
	    }

	    public ViewFactory ViewFactory
	    {
	        get { return _viewFactory; }
	    }
	}
} // end of namespace
