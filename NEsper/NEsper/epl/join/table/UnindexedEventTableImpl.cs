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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
	/// <summary>
	/// Simple table of events without an index.
	/// </summary>
	public class UnindexedEventTableImpl : UnindexedEventTable
	{
	    private readonly ISet<EventBean> _eventSet = new LinkedHashSet<EventBean>();

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">is the indexed stream's number</param>
	    public UnindexedEventTableImpl(int streamNum)
            : base(streamNum)
	    {
	    }

	    public override void Clear()
	    {
	        _eventSet.Clear();
	    }

	    public override void Destroy()
        {
	        Clear();
	    }

	    public override void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
	        if (newData != null) {
	            _eventSet.AddAll(newData);
	        }
	        if (oldData != null) {
	            foreach (EventBean removeEvent in oldData) {
	                _eventSet.Remove(removeEvent);
	            }
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
	    }

	    public override void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (events != null) {

	            if (InstrumentationHelper.ENABLED && events.Length > 0) {
	                InstrumentationHelper.Get().QIndexAdd(this, events);
	                _eventSet.AddAll(events);
	                InstrumentationHelper.Get().AIndexAdd();
	                return;
	            }

	            _eventSet.AddAll(events);
	        }
	    }

	    public override void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (events != null) {

	            if (InstrumentationHelper.ENABLED && events.Length > 0) {
	                InstrumentationHelper.Get().QIndexRemove(this, events);
	                foreach (EventBean removeEvent in events) {
	                    _eventSet.Remove(removeEvent);
	                }
	                InstrumentationHelper.Get().AIndexRemove();
	                return;
	            }

	            foreach (EventBean removeEvent in events) {
	                _eventSet.Remove(removeEvent);
	            }
	        }
	    }

	    public override void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext) {
	        _eventSet.Add(@event);
	    }

	    public override void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext) {
	        _eventSet.Remove(@event);
	    }

	    public override bool IsEmpty()
	    {
	        return _eventSet.IsEmpty();
	    }

	    /// <summary>
	    /// Returns events in table.
	    /// </summary>
	    /// <value>all events</value>
	    public override ISet<EventBean> EventSet
	    {
	        get { return _eventSet; }
	    }

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        return _eventSet.GetEnumerator();
	    }

	    public override int? NumberOfEvents
	    {
	        get { return _eventSet.Count; }
	    }

	    public override object Index
	    {
	        get { return _eventSet; }
	    }

	    public override Type ProviderClass
	    {
	        get { return typeof (UnindexedEventTable); }
	    }
	}
} // end of namespace
