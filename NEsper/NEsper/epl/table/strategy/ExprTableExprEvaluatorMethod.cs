///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableExprEvaluatorMethod 
        : ExprTableExprEvaluatorBase 
        , ExprEvaluator
    {
        private readonly int methodNum;
    
        public ExprTableExprEvaluatorMethod(ExprNode exprNode, string tableName, string subpropName, int streamNum, Type returnType, int methodNum)
            : base(exprNode, tableName, subpropName, streamNum, returnType)
        {
            this.methodNum = methodNum;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QExprTableSubproperty(exprNode, tableName, subpropName);
            }
    
            var oa = (ObjectArrayBackedEventBean) eventsPerStream[streamNum];
            var row = ExprTableEvalStrategyUtil.GetRow(oa);
            var result = row.Methods[methodNum].Value;
    
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AExprTableSubproperty(result);
            }
            return result;
        }

        public Type ReturnType
        {
            get { return returnType; }
        }
    }
}
