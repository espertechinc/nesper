///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertNoWildcardObjectArray
        : EvalBase,
          SelectExprProcessor
    {
        public EvalInsertNoWildcardObjectArray(SelectExprContext selectExprContext, EventType resultEventType)
            : base(selectExprContext, resultEventType)
        {
        }

        public EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            var result = new Object[base.ExprNodes.Length];
            for (int i = 0; i < base.ExprNodes.Length; i++)
            {
                result[i] = base.ExprNodes[i].Evaluate(evaluateParams);
            }

            return base.EventAdapterService.AdapterForTypedObjectArray(result, base.ResultEventType);
        }
    }
} // end of namespace