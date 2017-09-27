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

    public class SubselectEvalStrategyNRRelOpAnyWGroupBy : SubselectEvalStrategyNRRelOpBase
    {
        private readonly ExprEvaluator _havingEval;

        public SubselectEvalStrategyNRRelOpAnyWGroupBy(
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
            AggregationService aggregationServiceAnyPartition)
        {
            var aggregationService =
                aggregationServiceAnyPartition.GetContextPartitionAggregationService(
                    exprEvaluatorContext.AgentInstanceId);
            var groupKeys = aggregationService.GetGroupKeys(exprEvaluatorContext);
            var hasNonNullRow = false;
            var hasRows = false;

            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            foreach (var groupKey in groupKeys)
            {
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);

                if (_havingEval != null)
                {
                    var pass = _havingEval.Evaluate(evaluateParams);
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

                if (valueRight != null)
                {
                    hasNonNullRow = true;
                }

                if ((leftResult != null) && (valueRight != null))
                {
                    if (Computer.Invoke(leftResult, valueRight))
                    {
                        return true;
                    }
                }
            }

            if (!hasRows)
            {
                return false;
            }
            if ((!hasNonNullRow) || (leftResult == null))
            {
                return null;
            }
            return false;
        }
    }
} // end of namespace
