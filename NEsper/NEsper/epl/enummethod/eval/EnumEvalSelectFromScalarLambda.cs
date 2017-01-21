///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalSelectFromScalarLambda
        : EnumEvalBase
        , EnumEval
    {
        private readonly ObjectArrayEventType _resultEventType;

        public EnumEvalSelectFromScalarLambda(ExprEvaluator innerExpression, int streamCountIncoming, ObjectArrayEventType resultEventType)
            : base(innerExpression, streamCountIncoming)
        {
            _resultEventType = resultEventType;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty())
            {
                return target;
            }

            var resultEvent = new ObjectArrayEventBean(new Object[1], _resultEventType);
            var values = (ICollection<Object>)target;
            var queue = new LinkedList<object>();
            foreach (Object next in values)
            {

                resultEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = resultEvent;

                var item = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (item != null)
                {
                    queue.AddLast(item);
                }
            }

            return queue;
        }
    }
}
