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

namespace com.espertech.esper.epl.expression.subquery
{
    using RelationalComputer = Func<object, object, bool>;

    public class SubselectEvalStrategyNRRelOpAllDefault : SubselectEvalStrategyNRRelOpBase
    {
        private readonly ExprEvaluator _filterOrHavingEval;

        public SubselectEvalStrategyNRRelOpAllDefault(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool resultWhenNoMatchingEvents,
            RelationalComputer computer,
            ExprEvaluator filterOrHavingEval)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents, computer)
        {
            _filterOrHavingEval = filterOrHavingEval;
        }

        protected override Object EvaluateInternal(
            Object leftResult,
            EventBean[] events,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            bool hasRows = false;
            bool hasNullRow = false;

            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);
            foreach (EventBean subselectEvent in matchingEvents)
            {
                events[0] = subselectEvent;

                if (_filterOrHavingEval != null)
                {
                    var pass = _filterOrHavingEval.Evaluate(evaluateParams);
                    if ((pass == null) || (false.Equals(pass)))
                    {
                        continue;
                    }
                }
                hasRows = true;

                Object valueRight;
                if (SelectEval != null)
                {
                    valueRight = SelectEval.Evaluate(evaluateParams);
                }
                else
                {
                    valueRight = events[0].Underlying;
                }

                if (valueRight == null)
                {
                    hasNullRow = true;
                }
                else
                {
                    if (leftResult != null)
                    {
                        if (!Computer.Invoke(leftResult, valueRight))
                        {
                            return false;
                        }
                    }
                }
            }

            if (!hasRows)
            {
                return true;
            }
            if (leftResult == null)
            {
                return null;
            }
            if (hasNullRow)
            {
                return null;
            }
            return true;
        }
    }
} // end of namespace
