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
using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.subquery
{
    using RelationalComputer = Func<object, object, bool>;

    public class SubselectEvalStrategyNRRelOpAllAnyAggregated : SubselectEvalStrategyNRRelOpBase
    {
        private readonly ExprEvaluator _havingEval;

        public SubselectEvalStrategyNRRelOpAllAnyAggregated(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool resultWhenNoMatchingEvents,
            RelationalComputer computer,
            ExprEvaluator havingEval)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents, computer)
        {

            _havingEval = havingEval;
        }

        protected override Object EvaluateInternal(
            Object leftResult,
            EventBean[] events,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService)
        {
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            if (_havingEval != null)
            {
                var pass = _havingEval.Evaluate(evaluateParams);
                if ((pass == null) || (false.Equals(pass)))
                {
                    return null;
                }
            }
            var valueRight = SelectEval.Evaluate(evaluateParams);
            if (valueRight == null)
            {
                return null;
            }
            return Computer.Invoke(leftResult, valueRight);
        }
    }
} // end of namespace
