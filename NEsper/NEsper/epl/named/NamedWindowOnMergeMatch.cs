///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowOnMergeMatch
    {
        private readonly ExprEvaluator _optionalCond;
        private readonly IList<NamedWindowOnMergeAction> _actions;
    
        public NamedWindowOnMergeMatch(ExprNode optionalCond, IList<NamedWindowOnMergeAction> actions)
        {
            _optionalCond = optionalCond != null ? optionalCond.ExprEvaluator : null;
            _actions = actions;
        }
    
        public bool IsApplies(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (_optionalCond == null) {
                return true;
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, context);
            var result = _optionalCond.Evaluate(evaluateParams);
            return result != null && (bool) result;
        }
    
        public void Apply(EventBean matchingEvent, EventBean[] eventsPerStream, OneEventCollection newData, OneEventCollection oldData, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThenActions(_actions.Count); }
    
            int count = -1;
            foreach (NamedWindowOnMergeAction action in _actions) {
                count++;
    
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QInfraMergeWhenThenActionItem(count, action.GetName());
                    bool applies = action.IsApplies(eventsPerStream, context);
                    if (applies) {
                        action.Apply(matchingEvent, eventsPerStream, newData, oldData, context);
                    }
                    InstrumentationHelper.Get().AInfraMergeWhenThenActionItem(applies);
                    continue;
                }
    
                if (action.IsApplies(eventsPerStream, context)) {
                    action.Apply(matchingEvent, eventsPerStream, newData, oldData, context);
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenActions(); }
        }
    }
}
