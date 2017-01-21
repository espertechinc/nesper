///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyUngroupedProp 
        : ExprTableEvalStrategyUngroupedBase 
        , ExprTableAccessEvalStrategy
    {
        private readonly int _propertyIndex;
        private readonly ExprEvaluatorEnumerationGivenEvent _optionalEnumEval;

        public ExprTableEvalStrategyUngroupedProp(TableAndLockProviderUngrouped provider, int propertyIndex, ExprEvaluatorEnumerationGivenEvent optionalEnumEval)
            : base(provider)
        {
            _propertyIndex = propertyIndex;
            _optionalEnumEval = optionalEnumEval;
        }
    
        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null) {
                return null;
            }
            return @event.Properties[_propertyIndex];
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null) {
                return null;
            }
            return _optionalEnumEval.EvaluateEventGetROCollectionEvents(@event, context);
        }
    
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null) {
                return null;
            }
            return _optionalEnumEval.EvaluateEventGetEventBean(@event, context);
        }
    
        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null) {
                return null;
            }
            return _optionalEnumEval.EvaluateEventGetROCollectionScalar(@event, context);
        }
    
        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}
