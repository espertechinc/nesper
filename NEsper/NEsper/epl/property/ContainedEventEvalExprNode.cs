///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.property
{
    public class ContainedEventEvalExprNode : ContainedEventEval
    {
        private readonly ExprEvaluator _evaluator;
        private readonly EventBeanFactory _eventBeanFactory;
    
        public ContainedEventEvalExprNode(ExprEvaluator evaluator, EventBeanFactory eventBeanFactory) {
            _evaluator = evaluator;
            _eventBeanFactory = eventBeanFactory;
        }
    
        public Object GetFragment(EventBean eventBean, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
            Object result = _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
    
            if (result == null) {
                return null;
            }

            var asArray = result as Array;
            if (asArray != null)
            {
                return asArray
                    .Cast<object>()
                    .Where(item => item != null)
                    .Select(item => _eventBeanFactory.Wrap(item))
                    .ToArray();
            }
    
            if (result is ICollection) {
                var collection = (ICollection)result;
                return collection
                    .Cast<object>()
                    .Where(item => item != null)
                    .Select(item => _eventBeanFactory.Wrap(item))
                    .ToArray();
            }
    
            if (result is IEnumerable) {
                var enumerable = (IEnumerable) result;
                return enumerable
                    .Cast<object>()
                    .Where(item => item != null)
                    .Select(item => _eventBeanFactory.Wrap(item))
                    .ToArray();
            }
                    
            return null;
        }
    }
}
