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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.subquery
{
    public abstract class SubselectAggregationPreprocessorBase {
    
        protected readonly AggregationService aggregationService;
        protected readonly ExprEvaluator filterEval;
        protected readonly ExprEvaluator[] groupKeys;
    
        public SubselectAggregationPreprocessorBase(AggregationService aggregationService, ExprEvaluator filterEval, ExprEvaluator[] groupKeys) {
            this.aggregationService = aggregationService;
            this.filterEval = filterEval;
            this.groupKeys = groupKeys;
        }
    
        public abstract void Evaluate(EventBean[] eventsPerStream, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
    
        protected Object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            if (groupKeys.Length == 1) {
                return groupKeys[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }
            var keys = new Object[groupKeys.Length];
            for (int i = 0; i < groupKeys.Length; i++) {
                keys[i] = groupKeys[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }
            return new MultiKeyUntyped(keys);
        }
    }
} // end of namespace
