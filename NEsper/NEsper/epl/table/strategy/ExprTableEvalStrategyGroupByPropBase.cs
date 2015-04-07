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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyGroupByPropBase : ExprTableEvalStrategyGroupByBase , ExprTableAccessEvalStrategy
    {
        private readonly int _propertyIndex;
        private readonly ExprEvaluatorEnumerationGivenEvent _optionalEnumEval;

        public abstract object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
        public abstract ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        public abstract EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        public abstract ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        protected ExprTableEvalStrategyGroupByPropBase(ILockable @lock, IDictionary<Object, ObjectArrayBackedEventBean> aggregationState, int propertyIndex, ExprEvaluatorEnumerationGivenEvent optionalEnumEval)
            : base(@lock, aggregationState)
        {
            _propertyIndex = propertyIndex;
            _optionalEnumEval = optionalEnumEval;
        }
    
        public object EvaluateInternal(object groupKey, ExprEvaluatorContext context) {
            var row = LockTableReadAndGet(groupKey, context);
            if (row == null) {
                return null;
            }
            return row.Properties[_propertyIndex];
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEventsInternal(object groupKey, ExprEvaluatorContext context) {
            var row = LockTableReadAndGet(groupKey, context);
            if (row == null) {
                return null;
            }
            return _optionalEnumEval.EvaluateEventGetROCollectionEvents(row, context);
        }
    
        public EventBean EvaluateGetEventBeanInternal(object groupKey, ExprEvaluatorContext context) {
            var row = LockTableReadAndGet(groupKey, context);
            if (row == null) {
                return null;
            }
            return _optionalEnumEval.EvaluateEventGetEventBean(row, context);
        }
    
        public ICollection<object> EvaluateGetROCollectionScalarInternal(object groupKey, ExprEvaluatorContext context) {
            var row = LockTableReadAndGet(groupKey, context);
            if (row == null) {
                return null;
            }
            return _optionalEnumEval.EvaluateEventGetROCollectionScalar(row, context);
        }
    
        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }
    }
}
