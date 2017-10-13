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
    /// <summary>Strategy for subselects with "=/!=/&gt;&lt; ALL".</summary>
    public class SubselectEvalStrategyNREqualsAllDefault : SubselectEvalStrategyNREqualsBase
    {
        private readonly ExprEvaluator _filterEval;

        public SubselectEvalStrategyNREqualsAllDefault(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool resultWhenNoMatchingEvents,
            bool notIn,
            Coercer coercer,
            ExprEvaluator filterEval)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents, notIn, coercer)
        {
            
            this._filterEval = filterEval;
        }

        protected override Object EvaluateInternal(
            Object leftResult,
            EventBean[] events,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            var hasNullRow = false;
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            foreach (EventBean theEvent in matchingEvents)
            {
                events[0] = theEvent;

                // Eval filter expression
                if (_filterEval != null)
                {
                    var pass = _filterEval.Evaluate(evaluateParams);
                    if ((pass == null) || (false.Equals(pass)))
                    {
                        continue;
                    }
                }
                if (leftResult == null)
                {
                    return null;
                }

                Object rightResult;
                if (SelectEval != null)
                {
                    rightResult = SelectEval.Evaluate(evaluateParams);
                }
                else
                {
                    rightResult = events[0].Underlying;
                }

                if (rightResult != null)
                {
                    if (Coercer == null)
                    {
                        bool eq = leftResult.Equals(rightResult);
                        if ((IsNot && eq) || (!IsNot && !eq))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        var left = Coercer.Invoke(leftResult);
                        var right = Coercer.Invoke(rightResult);
                        var eq = left.Equals(right);
                        if ((IsNot && eq) || (!IsNot && !eq))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    hasNullRow = true;
                }
            }

            if (hasNullRow)
            {
                return null;
            }
            return true;
        }
    }
} // end of namespace
