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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalMinMaxScalarLambda
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _max;
        private readonly ObjectArrayEventType _resultEventType;

        public EnumEvalMinMaxScalarLambda(ExprEvaluator innerExpression, int streamCountIncoming, bool max, ObjectArrayEventType resultEventType)
            : base(innerExpression, streamCountIncoming)
        {
            _max = max;
            _resultEventType = resultEventType;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            IComparable minKey = null;

            var resultEvent = new ObjectArrayEventBean(new Object[1], _resultEventType);
            var coll = (ICollection<Object>)target;
            foreach (Object next in coll)
            {

                resultEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = resultEvent;

                Object comparable = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (comparable == null)
                {
                    continue;
                }

                if (minKey == null)
                {
                    minKey = (IComparable)comparable;
                }
                else
                {
                    if (_max)
                    {
                        if (minKey.CompareTo(comparable) < 0)
                        {
                            minKey = (IComparable)comparable;
                        }
                    }
                    else
                    {
                        if (minKey.CompareTo(comparable) > 0)
                        {
                            minKey = (IComparable)comparable;
                        }
                    }
                }
            }

            return minKey;
        }
    }
}