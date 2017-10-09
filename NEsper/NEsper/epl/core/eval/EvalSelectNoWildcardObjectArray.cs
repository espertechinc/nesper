///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectNoWildcardObjectArray
        : SelectExprProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventType _resultEventType;
        private readonly SelectExprContext _selectExprContext;

        public EvalSelectNoWildcardObjectArray(SelectExprContext selectExprContext, EventType resultEventType)
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
            ExprEvaluator[] expressionNodes = _selectExprContext.ExpressionNodes;
            EventAdapterService eventAdapterService = _selectExprContext.EventAdapterService;

            // Evaluate all expressions and build a map of name-value pairs
            var props = new Object[expressionNodes.Length];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);

            for (int i = 0; i < expressionNodes.Length; i++)
            {
                Object evalResult = expressionNodes[i].Evaluate(evaluateParams);
                props[i] = evalResult;
            }

            return eventAdapterService.AdapterForTypedObjectArray(props, _resultEventType);
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }

        #endregion
    }
}