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

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalMinMaxScalar
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _max;

        public EnumEvalMinMaxScalar(int streamCountIncoming, bool max)
            : base(streamCountIncoming)
        {
            _max = max;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            IComparable minKey = null;

            foreach (Object next in target)
            {
                var comparable = next;
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
