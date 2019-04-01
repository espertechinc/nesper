///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// View for the on-delete statement that handles removing events from a named window.
	/// </summary>
	public abstract class NamedWindowOnExprBaseView : ViewSupport
	{
	    /// <summary>
	    /// The event type of the events hosted in the named window.
	    /// </summary>
	    private readonly SubordWMatchExprLookupStrategy _lookupStrategy;
	    private readonly ExprEvaluatorContext _exprEvaluatorContext;

	    /// <summary>
	    /// The root view accepting removals (old data).
	    /// </summary>
	    protected readonly NamedWindowRootViewInstance RootView;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="lookupStrategy">for handling trigger events to determine deleted events</param>
	    /// <param name="rootView">to indicate which events to delete</param>
	    /// <param name="exprEvaluatorContext">context for expression evalauation</param>
	    protected NamedWindowOnExprBaseView(
	        SubordWMatchExprLookupStrategy lookupStrategy,
	        NamedWindowRootViewInstance rootView,
	        ExprEvaluatorContext exprEvaluatorContext)
	    {
	        _lookupStrategy = lookupStrategy;
	        RootView = rootView;
	        _exprEvaluatorContext = exprEvaluatorContext;
	    }

	    /// <summary>
	    /// Implemented by on-trigger views to action on the combination of trigger and matching events in the named window.
	    /// </summary>
	    /// <param name="triggerEvents">is the trigger events (usually 1)</param>
	    /// <param name="matchingEvents">is the matching events retrieved via lookup strategy</param>
	    public abstract void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents);

	    public override void Update(EventBean[] newData, EventBean[] oldData)
	    {
	        if (newData == null)
	        {
	            return;
	        }

	        // Determine via the lookup strategy a subset of events to process
	        EventBean[] eventsFound = _lookupStrategy.Lookup(newData, _exprEvaluatorContext);

	        // Let the implementation handle the delete or other action
	        HandleMatching(newData, eventsFound);
	    }

	    /// <summary>
	    /// returns expr context.
	    /// </summary>
	    /// <value>context</value>
	    public ExprEvaluatorContext ExprEvaluatorContext => _exprEvaluatorContext;
	}
} // end of namespace
