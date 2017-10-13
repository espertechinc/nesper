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
    /// <summary>Represents a in-subselect evaluation strategy.</summary>
    public class SubselectEvalStrategyNREqualsInFiltered : SubselectEvalStrategyNREqualsInBase
    {
        private readonly ExprEvaluator _filterEval;
    
        public SubselectEvalStrategyNREqualsInFiltered(ExprEvaluator valueEval, ExprEvaluator selectEval, bool notIn, Coercer coercer, ExprEvaluator filterEval)
            : base(valueEval, selectEval, notIn, coercer)
        {
            _filterEval = filterEval;
        }

        protected override Object EvaluateInternal(
            Object leftResult,
            EventBean[] eventsZeroOffset,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            bool hasNullRow = false;
            var evaluateParams = new EvaluateParams(eventsZeroOffset, true, exprEvaluatorContext);

            foreach (EventBean subselectEvent in matchingEvents)
            {
                // Prepare filter expression event list
                eventsZeroOffset[0] = subselectEvent;

                // Eval filter expression
                var pass = _filterEval.Evaluate(evaluateParams);
                if ((pass == null) || (false.Equals(pass)))
                {
                    continue;
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
                    rightResult = eventsZeroOffset[0].Underlying;
                }

                if (rightResult == null)
                {
                    hasNullRow = true;
                }
                else
                {
                    if (Coercer == null)
                    {
                        if (leftResult.Equals(rightResult))
                        {
                            return !IsNotIn;
                        }
                    }
                    else
                    {
                        var left = Coercer.Invoke(leftResult);
                        var right = Coercer.Invoke(rightResult);
                        if (left.Equals(right))
                        {
                            return !IsNotIn;
                        }
                    }
                }
            }

            if (hasNullRow)
            {
                return null;
            }

            return IsNotIn;
        }
    }
} // end of namespace
