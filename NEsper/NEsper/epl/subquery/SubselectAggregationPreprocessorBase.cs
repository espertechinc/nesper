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

namespace com.espertech.esper.epl.subquery
{
    public abstract class SubselectAggregationPreprocessorBase
    {
        private readonly AggregationService _aggregationService;
        private readonly ExprEvaluator _filterEval;
        private readonly ExprEvaluator[] _groupKeys;

        protected SubselectAggregationPreprocessorBase(AggregationService aggregationService, ExprEvaluator filterEval, ExprEvaluator[] groupKeys)
        {
            _aggregationService = aggregationService;
            _filterEval = filterEval;
            _groupKeys = groupKeys;
        }

        public AggregationService AggregationService
        {
            get { return _aggregationService; }
        }

        public ExprEvaluator FilterEval
        {
            get { return _filterEval; }
        }

        public ExprEvaluator[] GroupKeys
        {
            get { return _groupKeys; }
        }

        public abstract void Evaluate(EventBean[] eventsPerStream, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext);
    
        protected Object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            if (_groupKeys.Length == 1)
            {
                return _groupKeys[0].Evaluate(evaluateParams);
            }
            var keys = new Object[_groupKeys.Length];
            for (int i = 0; i < _groupKeys.Length; i++)
            {
                keys[i] = _groupKeys[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(keys);
        }
    }
} // end of namespace
