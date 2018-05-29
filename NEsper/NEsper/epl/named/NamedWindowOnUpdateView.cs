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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// View for the on-delete statement that handles removing events from a named window.
	/// </summary>
	public class NamedWindowOnUpdateView : NamedWindowOnExprBaseView
	{
	    private readonly NamedWindowOnUpdateViewFactory _parent;
	    private EventBean[] _lastResult;

	    public NamedWindowOnUpdateView(SubordWMatchExprLookupStrategy lookupStrategy, NamedWindowRootViewInstance rootView, ExprEvaluatorContext exprEvaluatorContext, NamedWindowOnUpdateViewFactory parent)
	        : base(lookupStrategy, rootView, exprEvaluatorContext)
        {
	        _parent = parent;
	    }

	    public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_UPDATE, triggerEvents, matchingEvents);}

	        if ((matchingEvents == null) || (matchingEvents.Length == 0)){
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction();}
	            return;
	        }

	        var eventsPerStream = new EventBean[3];

	        var newData = new OneEventCollection();
	        var oldData = new OneEventCollection();

	        foreach (var triggerEvent in triggerEvents) {
	            eventsPerStream[1] = triggerEvent;
	            foreach (var matchingEvent in matchingEvents) {
	                var copy = _parent.UpdateHelper.UpdateWCopy(matchingEvent, eventsPerStream, base.ExprEvaluatorContext);
	                newData.Add(copy);
	                oldData.Add(matchingEvent);
	            }
	        }

	        if (!newData.IsEmpty())
	        {
	            // Events to delete are indicated via old data
	            RootView.Update(newData.ToArray(), oldData.ToArray());

	            // The on-delete listeners receive the events deleted, but only if there is interest
	            if (_parent.StatementResultService.IsMakeNatural || _parent.StatementResultService.IsMakeSynthetic) {
	                UpdateChildren(newData.ToArray(), oldData.ToArray());
	            }
	        }

	        // Keep the last delete records
	        _lastResult = matchingEvents;
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction();}
	    }

	    public override EventType EventType => RootView.EventType;

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        if (_lastResult == null)
	            return EnumerationHelper.Empty<EventBean>();
            return ((IEnumerable<EventBean>)_lastResult).GetEnumerator();
	    }
	}
} // end of namespace
