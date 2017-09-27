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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Strategy for subselects with "=/!=/&gt;&lt; ANY".</summary>
    public class SubselectEvalStrategyNREqualsAnyDefault : SubselectEvalStrategyNREqualsBase
    {
        private readonly ExprEvaluator _filterEval;

        public SubselectEvalStrategyNREqualsAnyDefault(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool resultWhenNoMatchingEvents,
            bool notIn,
            Coercer coercer,
            ExprEvaluator filterEval)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents, notIn, coercer)
        {
            _filterEval = filterEval;
        }
    
        protected override Object EvaluateInternal(Object leftResult, EventBean[] events, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, AggregationService aggregationService)
        {
            bool hasNullRow = false;

            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);
            foreach (EventBean theEvent in matchingEvents)
            {
                events[0] = theEvent;
    
                Object rightResult;
                if (SelectEval != null)
                {
                    rightResult = SelectEval.Evaluate(evaluateParams);
                }
                else {
                    rightResult = events[0].Underlying;
                }
    
                // Eval filter expression
                if (_filterEval != null) {
                    var pass = _filterEval.Evaluate(evaluateParams);
                    if ((pass == null) || (false.Equals(pass))) {
                        continue;
                    }
                }
                if (leftResult == null) {
                    return null;
                }
    
                if (rightResult != null) {
                    if (Coercer == null) {
                        bool eq = leftResult.Equals(rightResult);
                        if ((IsNot && !eq) || (!IsNot && eq)) {
                            return true;
                        }
                    } else {
                        var left = Coercer.Invoke(leftResult);
                        var right = Coercer.Invoke(rightResult);
                        bool eq = left.Equals(right);
                        if ((IsNot && !eq) || (!IsNot && eq)) {
                            return true;
                        }
                    }
                } else {
                    hasNullRow = true;
                }
            }
    
            if (hasNullRow) {
                return null;
            }
    
            return false;
        }
    }
} // end of namespace
