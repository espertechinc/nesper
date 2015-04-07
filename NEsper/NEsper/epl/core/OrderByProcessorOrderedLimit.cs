///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Sorter and row limiter in one: sorts using a sorter and row limits
    /// </summary>
    public class OrderByProcessorOrderedLimit : OrderByProcessor
    {
        private readonly OrderByProcessorImpl _orderByProcessor;
        private readonly RowLimitProcessor _rowLimitProcessor;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="orderByProcessor">the sorter</param>
        /// <param name="rowLimitProcessor">the row limiter</param>
        public OrderByProcessorOrderedLimit(OrderByProcessorImpl orderByProcessor, RowLimitProcessor rowLimitProcessor)
        {
            _orderByProcessor = orderByProcessor;
            _rowLimitProcessor = rowLimitProcessor;
        }
    
        public EventBean[] Sort(EventBean[] outgoingEvents, EventBean[][] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            _rowLimitProcessor.DetermineCurrentLimit();

            if (_rowLimitProcessor.CurrentRowLimit == 1 &&
                _rowLimitProcessor.CurrentOffset == 0 &&
                outgoingEvents != null && 
                outgoingEvents.Length > 1)
            {
                EventBean minmax = _orderByProcessor.DetermineLocalMinMax(outgoingEvents, generatingEvents, isNewData, exprEvaluatorContext);
                return new EventBean[] { minmax };
            }

            EventBean[] sorted = _orderByProcessor.Sort(outgoingEvents, generatingEvents, isNewData, exprEvaluatorContext);
            return _rowLimitProcessor.ApplyLimit(sorted);
        }
    
        public EventBean[] Sort(EventBean[] outgoingEvents, IList<GroupByRollupKey> currentGenerators, bool newData, AgentInstanceContext agentInstanceContext, OrderByElement[][] elementsPerLevel) {
            EventBean[] sorted = _orderByProcessor.Sort(outgoingEvents, currentGenerators, newData, agentInstanceContext, elementsPerLevel);
            return _rowLimitProcessor.ApplyLimit(sorted);
        }
    
        public EventBean[] Sort(EventBean[] outgoingEvents, EventBean[][] generatingEvents, Object[] groupByKeys, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] sorted = _orderByProcessor.Sort(outgoingEvents, generatingEvents, groupByKeys, isNewData, exprEvaluatorContext);
            return _rowLimitProcessor.ApplyLimit(sorted);
        }
    
        public Object GetSortKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    
        public Object GetSortKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, OrderByElement[] elementsForLevel) {
            return _orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext, elementsForLevel);
        }
    
        public Object[] GetSortKeyPerRow(EventBean[] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _orderByProcessor.GetSortKeyPerRow(generatingEvents, isNewData, exprEvaluatorContext);
        }
    
        public EventBean[] Sort(EventBean[] outgoingEvents, Object[] orderKeys, ExprEvaluatorContext exprEvaluatorContext)
        {
            _rowLimitProcessor.DetermineCurrentLimit();

            if (_rowLimitProcessor.CurrentRowLimit == 1 &&
                _rowLimitProcessor.CurrentOffset == 0 &&
                outgoingEvents != null && 
                outgoingEvents.Length > 1)
            {
                EventBean minmax = _orderByProcessor.DetermineLocalMinMax(outgoingEvents, orderKeys);
                return new EventBean[] { minmax };
            }

            EventBean[] sorted = _orderByProcessor.Sort(outgoingEvents, orderKeys, exprEvaluatorContext);
            return _rowLimitProcessor.ApplyLimit(sorted);
        }
    }
}
