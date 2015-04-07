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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalMinMaxEvents
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _max;

        public EnumEvalMinMaxEvents(ExprEvaluator innerExpression, int streamCountIncoming, bool max)
            : base(innerExpression, streamCountIncoming)
        {
            _max = max;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            IComparable minKey = null;

            var beans = target;
            foreach (EventBean next in beans)
            {
                eventsLambda[StreamNumLambda] = next;

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
