///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor prototype for the simplest case: no aggregation functions used in the select clause, and no group-by.
    /// </summary>
    public class ResultSetProcessorSimpleFactory : ResultSetProcessorFactory
    {
        private readonly bool _isSelectRStream;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly ExprEvaluator _optionalHavingExpr;
    
        /// <summary>Ctor. </summary>
        /// <param name="selectExprProcessor">for processing the select expression and generting the readonly output rows</param>
        /// <param name="optionalHavingNode">having clause expression node</param>
        /// <param name="isSelectRStream">true if remove stream events should be generated</param>
        public ResultSetProcessorSimpleFactory(SelectExprProcessor selectExprProcessor,
                                               ExprEvaluator optionalHavingNode,
                                               bool isSelectRStream)
        {
            _selectExprProcessor = selectExprProcessor;
            _optionalHavingExpr = optionalHavingNode;
            _isSelectRStream = isSelectRStream;
        }
    
        public ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext) 
        {
            return new ResultSetProcessorSimple(this, _selectExprProcessor, orderByProcessor, agentInstanceContext);
        }

        public EventType ResultEventType
        {
            get { return _selectExprProcessor.ResultEventType; }
        }

        public bool HasAggregation
        {
            get { return false; }
        }

        public bool IsSelectRStream
        {
            get { return _isSelectRStream; }
        }

        public ExprEvaluator OptionalHavingExpr
        {
            get { return _optionalHavingExpr; }
        }
    }
}
