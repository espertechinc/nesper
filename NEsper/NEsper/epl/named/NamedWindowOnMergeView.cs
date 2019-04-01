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
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// View for the on-delete statement that handles removing events from a named window.
	/// </summary>
	public class NamedWindowOnMergeView : NamedWindowOnExprBaseView
	{
	    private readonly NamedWindowOnMergeViewFactory _parent;
	    private EventBean[] _lastResult;

	    public NamedWindowOnMergeView(SubordWMatchExprLookupStrategy lookupStrategy, NamedWindowRootViewInstance rootView, ExprEvaluatorContext exprEvaluatorContext, NamedWindowOnMergeViewFactory parent)
	        : base(lookupStrategy, rootView, exprEvaluatorContext)
        {
	        _parent = parent;
	    }

	    public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_MERGE, triggerEvents, matchingEvents);}

	        var newData = new OneEventCollection();
	        OneEventCollection oldData = null;
	        var eventsPerStream = new EventBean[3]; // first:named window, second: trigger, third:before-update (optional)

	        if ((matchingEvents == null) || (matchingEvents.Length == 0)){

	            var unmatched = _parent.NamedWindowOnMergeHelper.Unmatched;

	            foreach (var triggerEvent in triggerEvents) {
	                eventsPerStream[1] = triggerEvent;

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThens(false, triggerEvent, unmatched.Count);}

	                var count = -1;
	                foreach (var action in unmatched) {
	                    count++;

	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThenItem(false, count);}
	                    if (!action.IsApplies(eventsPerStream, base.ExprEvaluatorContext)) {
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(false, false);}
	                        continue;
	                    }
	                    action.Apply(null, eventsPerStream, newData, oldData, base.ExprEvaluatorContext);
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(false, true);}
	                    break;  // apply no other actions
	                }

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThens(false);}
	            }
	        }
	        else {

	            // handle update/
	            oldData = new OneEventCollection();

	            var matched = _parent.NamedWindowOnMergeHelper.Matched;

	            foreach (var triggerEvent in triggerEvents) {
	                eventsPerStream[1] = triggerEvent;
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThens(true, triggerEvent, matched.Count);}

	                foreach (var matchingEvent in matchingEvents) {
	                    eventsPerStream[0] = matchingEvent;

	                    var count = -1;
	                    foreach (var action in matched) {
	                        count++;

	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThenItem(true, count);}
	                        if (!action.IsApplies(eventsPerStream, base.ExprEvaluatorContext)) {
	                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(true, false);}
	                            continue;
	                        }
	                        action.Apply(matchingEvent, eventsPerStream, newData, oldData, base.ExprEvaluatorContext);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(true, true);}
	                        break;  // apply no other actions
	                    }
	                }

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThens(true);}
	            }

	        }

	        if (!newData.IsEmpty() || (oldData != null && !oldData.IsEmpty()))
	        {
	            if ((MetricReportingPath.IsMetricsEnabled) && (_parent.CreateNamedWindowMetricHandle.IsEnabled) && !newData.IsEmpty())
	            {
	                _parent.MetricReportingService.AccountTime(_parent.CreateNamedWindowMetricHandle, 0, 0, newData.ToArray().Length);
	            }

	            // Events to delete are indicated via old data
	            // The on-merge listeners receive the events deleted, but only if there is interest
	            if (_parent.StatementResultService.IsMakeNatural) {
	                var eventsPerStreamNaturalNew = newData.IsEmpty() ? null : newData.ToArray();
	                var eventsPerStreamNaturalOld = (oldData == null || oldData.IsEmpty()) ? null : oldData.ToArray();
	                RootView.Update(EventBeanUtility.Denaturalize(eventsPerStreamNaturalNew), EventBeanUtility.Denaturalize(eventsPerStreamNaturalOld));
	                UpdateChildren(eventsPerStreamNaturalNew, eventsPerStreamNaturalOld);
	            }
	            else {
	                var eventsPerStreamNew = newData.IsEmpty() ? null : newData.ToArray();
	                var eventsPerStreamOld = (oldData == null || oldData.IsEmpty()) ? null : oldData.ToArray();
	                RootView.Update(eventsPerStreamNew, eventsPerStreamOld);
	                if (_parent.StatementResultService.IsMakeSynthetic) {
	                    UpdateChildren(eventsPerStreamNew, eventsPerStreamOld);
	                }
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
