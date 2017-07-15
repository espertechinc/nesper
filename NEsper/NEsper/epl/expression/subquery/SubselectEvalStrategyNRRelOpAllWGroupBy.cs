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

    public class SubselectEvalStrategyNRRelOpAllWGroupBy : SubselectEvalStrategyNRRelOpBase
    {
        private readonly ExprEvaluator _havingEval;

        public SubselectEvalStrategyNRRelOpAllWGroupBy(
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
            AggregationService aggregationService =
                aggregationServiceAnyPartition.GetContextPartitionAggregationService(
                    exprEvaluatorContext.AgentInstanceId);
            ICollection<Object> groupKeys = aggregationService.GetGroupKeys(exprEvaluatorContext);
            bool hasRows = false;
            bool hasNullRow = false;

            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            foreach (Object groupKey in groupKeys)
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
