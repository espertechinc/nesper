///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.table
{
	/// <summary>
	/// Simple table of events without an index.
	/// </summary>
	public abstract class UnindexedEventTable : EventTable
	{
	    private readonly int _streamNum;

	    public abstract ISet<EventBean> EventSet { get; }

	    public abstract IEnumerator<EventBean> GetEnumerator();
	    public abstract void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract bool IsEmpty();
	    public abstract void Clear();
	    public abstract void Destroy();
	    public abstract Type ProviderClass { get; }
	    public abstract int? NumberOfEvents { get; }
	    public abstract object Index { get; }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">is the indexed stream's number</param>
	    protected UnindexedEventTable(int streamNum)
	    {
	        _streamNum = streamNum;
	    }

	    public override string ToString()
	    {
	        return ToQueryPlan();
	    }

	    public string ToQueryPlan()
	    {
	        return GetType().Name + " streamNum=" + _streamNum;
	    }

	    public virtual int NumKeys
	    {
	        get { return 0; }
	    }

	    public virtual EventTableOrganization Organization
	    {
	        get
	        {
	            return new EventTableOrganization(null, false, false, _streamNum, null, EventTableOrganizationType.UNORGANIZED);
	        }
	    }
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
} // end of namespace
