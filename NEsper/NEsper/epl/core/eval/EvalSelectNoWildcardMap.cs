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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectNoWildcardMap : SelectExprProcessor
    {
        private readonly EventType _resultEventType;
        private readonly SelectExprContext _selectExprContext;

        private static int seq = 0;
        private readonly int id = seq++;

        public EvalSelectNoWildcardMap(SelectExprContext selectExprContext, EventType resultEventType)
        {
            _selectExprContext = selectExprContext;
            _resultEventType = resultEventType;
        }

        #region SelectExprProcessor Members

        public EventBean Process(EventBean[] eventsPerStream,
                                 bool isNewData,
                                 bool isSynthesize,
                                 ExprEvaluatorContext exprEvaluatorContext)
        {
            var expressionNodes = _selectExprContext.ExpressionNodes;
            var columnNames = _selectExprContext.ColumnNames;
            var eventAdapterService = _selectExprContext.EventAdapterService;

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);

            // Evaluate all expressions and build a map of name-value pairs
            var props = new Dictionary<String, Object>();

            unchecked
            {
                for (int ii = 0; ii < expressionNodes.Length; ii++)
                {
                    var evalResult = expressionNodes[ii].Evaluate(evaluateParams);
                    props.Put(columnNames[ii], evalResult);
                }
            }

            return eventAdapterService.AdapterForTypedMap(props, _resultEventType);
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }

        #endregion
    }
}