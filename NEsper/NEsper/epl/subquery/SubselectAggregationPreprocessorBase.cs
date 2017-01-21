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
using com.espertech.esper.collection;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.subquery
{
    public abstract class SubselectAggregationPreprocessorBase
    {
        protected readonly AggregationService AggregationService;
        protected readonly ExprEvaluator FilterExpr;
        protected readonly ExprEvaluator[] GroupKeys;

        protected SubselectAggregationPreprocessorBase(
            AggregationService aggregationService,
            ExprEvaluator filterExpr,
            ExprEvaluator[] groupKeys)
        {
            AggregationService = aggregationService;
            FilterExpr = filterExpr;
            GroupKeys = groupKeys;
        }

        public abstract void Evaluate(
            EventBean[] eventsPerStream,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext);

        protected Object GenerateGroupKey(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            if (GroupKeys.Length == 1)
            {
                return GroupKeys[0].Evaluate(evaluateParams);
            }
            var keys = new Object[GroupKeys.Length];
            for (int i = 0; i < GroupKeys.Length; i++)
            {
                keys[i] = GroupKeys[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(keys);
        }
    }
}