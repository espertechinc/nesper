///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardObjectArrayRemap : SelectExprProcessor
    {
        private readonly SelectExprContext _selectExprContext;
        private readonly EventType _resultEventType;
        private readonly int[] _remapped;
    
        public EvalInsertNoWildcardObjectArrayRemap(SelectExprContext selectExprContext, EventType resultEventType, int[] remapped)
        {
            _selectExprContext = selectExprContext;
            _resultEventType = resultEventType;
            _remapped = remapped;
        }

        public EventType ResultEventType => _resultEventType;

        public int[] Remapped => _remapped;

        public SelectExprContext SelectExprContext => _selectExprContext;

        public virtual EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            var expressionNodes = _selectExprContext.ExpressionNodes;
    
            var result = new object[_resultEventType.PropertyNames.Length];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            for (int i = 0; i < expressionNodes.Length; i++)
            {
                result[_remapped[i]] = expressionNodes[i].Evaluate(evaluateParams);
            }

            return _selectExprContext.EventAdapterService.AdapterForTypedObjectArray(result, _resultEventType);
        }
    }
}
