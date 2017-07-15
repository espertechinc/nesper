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
    public class SubselectEvalStrategyNREqualsAllWGroupBy : SubselectEvalStrategyNREqualsBase{
    
        private readonly ExprEvaluator havingEval;
    
        public SubselectEvalStrategyNREqualsAllWGroupBy(ExprEvaluator valueEval, ExprEvaluator selectEval, bool resultWhenNoMatchingEvents, bool notIn, Coercer coercer, ExprEvaluator havingEval)
            : base(valueEval, selectEval, resultWhenNoMatchingEvents, notIn, coercer)
        {
            
            this.havingEval = havingEval;
        }
    
        protected override Object EvaluateInternal(Object leftResult, EventBean[] events, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, AggregationService aggregationServiceAnyPartition)
        {
            AggregationService aggregationService = aggregationServiceAnyPartition.GetContextPartitionAggregationService(exprEvaluatorContext.AgentInstanceId);
            ICollection<Object> groupKeys = aggregationService.GetGroupKeys(exprEvaluatorContext);
            bool hasNullRow = false;
    
            foreach (Object groupKey in groupKeys) {
                if (leftResult == null) {
                    return null;
                }
                aggregationService.SetCurrentAccess(groupKey, exprEvaluatorContext.AgentInstanceId, null);
    
                if (havingEval != null) {
                    bool? pass = (bool?) havingEval.Evaluate(events, true, exprEvaluatorContext);
                    if ((pass == null) || (false.Equals(pass))) {
                        continue;
                    }
                }
    
                Object rightResult;
                if (SelectEval != null) {
                    rightResult = SelectEval.Evaluate(events, true, exprEvaluatorContext);
                } else {
                    rightResult = events[0].Underlying;
                }
    
                if (rightResult != null) {
                    if (Coercer == null) {
                        bool eq = leftResult.Equals(rightResult);
                        if ((IsNot && eq) || (!IsNot && !eq)) {
                            return false;
                        }
                    } else {
                        Number left = Coercer.CoerceBoxed((Number) leftResult);
                        Number right = Coercer.CoerceBoxed((Number) rightResult);
                        bool eq = left.Equals(right);
                        if ((IsNot && eq) || (!IsNot && !eq)) {
                            return false;
                        }
                    }
                } else {
                    hasNullRow = true;
                }
            }
    
            if (hasNullRow) {
                return null;
            }
            return true;
        }
    }
} // end of namespace
