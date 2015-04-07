///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalTakeWhileIndexEvents : EnumEval
    {
        private readonly ExprEvaluator _innerExpression;
        private readonly int _streamNumLambda;
        private readonly ObjectArrayEventType _indexEventType;
    
        public EnumEvalTakeWhileIndexEvents(ExprEvaluator innerExpression, int streamNumLambda, ObjectArrayEventType indexEventType) {
            _innerExpression = innerExpression;
            _streamNumLambda = streamNumLambda;
            _indexEventType = indexEventType;
        }

        public int StreamNumSize
        {
            get { return _streamNumLambda + 2; }
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty()) {
                return target;
            }
    
            var indexEvent = new ObjectArrayEventBean(new Object[1], _indexEventType);
    
            if (target.Count == 1)
            {
                EventBean item = (EventBean) target.First();
                indexEvent.Properties[0] = 0;
                eventsLambda[_streamNumLambda] = item;
                eventsLambda[_streamNumLambda + 1] = indexEvent;

                var pass = (bool?) _innerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.GetValueOrDefault(false))
                {
                    return Collections.GetEmptyList<object>();
                }
                return Collections.SingletonList(item);
            }

            var result = new LinkedList<Object>();
    
            int count = -1;
            foreach (EventBean next in target) {
    
                count++;
    
                indexEvent.Properties[0] = count;
                eventsLambda[_streamNumLambda] = next;
                eventsLambda[_streamNumLambda + 1] = indexEvent;

                var pass = (bool?)_innerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.GetValueOrDefault(false))
                {
                    break;
                }
    
                result.AddLast(next);
            }
    
            return result;
        }
    }
}
