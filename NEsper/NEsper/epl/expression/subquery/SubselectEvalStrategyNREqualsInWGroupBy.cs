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
    public class SubselectEvalStrategyNREqualsInWGroupBy : SubselectEvalStrategyNREqualsInBase
    {
        private readonly ExprEvaluator _havingEval;

        public SubselectEvalStrategyNREqualsInWGroupBy(
            ExprEvaluator valueEval,
            ExprEvaluator selectEval,
            bool notIn,
            Coercer coercer,
            ExprEvaluator havingEval)
            : base(valueEval, selectEval, notIn, coercer)
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
            if (leftResult == null)
            {
                return null;
            }

            AggregationService aggregationService =
                aggregationServiceAnyPartition.GetContextPartitionAggregationService(
                    exprEvaluatorContext.AgentInstanceId);
            ICollection<Object> groupKeys = aggregationService.GetGroupKeys(exprEvaluatorContext);
            var evaluateParams = new EvaluateParams(events, true, exprEvaluatorContext);

            // Evaluate each select until we have a match
            bool hasNullRow = false;
            foreach (Object groupKey in groupKeys)
            {
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);

                if (_havingEval != null)
                {
                    var pass = (bool?) _havingEval.Evaluate(evaluateParams);
                    if ((pass == null) || (false.Equals(pass)))
                    {
                        continue;
                    }
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
