///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyUngroupedMethod : ExprTableEvalStrategyUngroupedBase , ExprTableAccessEvalStrategy {
    
        private readonly int _methodOffset;

        public ExprTableEvalStrategyUngroupedMethod(TableAndLockProviderUngrouped provider, int methodOffset)
            : base(provider)
        {
            this._methodOffset = methodOffset;
        }
    
        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null) 
            {
                return null;
            }
            AggregationRowPair row = ExprTableEvalStrategyUtil.GetRow(@event);
            return row.Methods[_methodOffset].Value;
        }
    
        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw new IllegalStateException("Not typable");
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    
        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}
