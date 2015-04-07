///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardObjectArrayRemapWWiden : EvalInsertNoWildcardObjectArrayRemap
    {
        private readonly TypeWidener[] _wideners;
    
        public EvalInsertNoWildcardObjectArrayRemapWWiden(SelectExprContext selectExprContext, EventType resultEventType, int[] remapped, TypeWidener[] wideners)
            : base(selectExprContext, resultEventType, remapped)
        {
            this._wideners = wideners;
        }
    
        public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            ExprEvaluator[] expressionNodes = SelectExprContext.ExpressionNodes;
    
            var result = new object[ResultEventType.PropertyNames.Length];
            for (var i = 0; i < expressionNodes.Length; i++) {
                var value = expressionNodes[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                if (_wideners[i] != null) {
                    value = _wideners[i].Invoke(value);
                }
                result[Remapped[i]] = value;
            }
    
            return SelectExprContext.EventAdapterService.AdapterForTypedObjectArray(result, ResultEventType);
        }
    }
}
