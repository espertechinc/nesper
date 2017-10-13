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
    public class SubselectEvalStrategyNREqualsInUnfiltered : SubselectEvalStrategyNREqualsInBase
    {
        public SubselectEvalStrategyNREqualsInUnfiltered(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool notIn,
            Coercer coercer)
            : base(valueEval, selectEval, notIn, coercer)
        {
        }

        protected override Object EvaluateInternal(
            Object leftResult,
            EventBean[] events,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            if (leftResult == null)
            {
                return null;
            }

            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            // Evaluate each select until we have a match
            bool hasNullRow = false;
            foreach (EventBean theEvent in matchingEvents)
            {
                events[0] = theEvent;

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
                else
                {
                    hasNullRow = true;
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
