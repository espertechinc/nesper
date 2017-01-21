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


namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalMostLeastFrequentEvent
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _isMostFrequent;

        public EnumEvalMostLeastFrequentEvent(ExprEvaluator innerExpression, int streamCountIncoming, bool mostFrequent)
            : base(innerExpression, streamCountIncoming)
        {
            _isMostFrequent = mostFrequent;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {

            var items = new LinkedHashMap<Object, int?>();

            foreach (EventBean next in target)
            {
                eventsLambda[StreamNumLambda] = next;

                Object item = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                int? existing = items.Get(item);
                if (!existing.HasValue)
                {
                    existing = 1;
                }
                else
                {
                    existing++;
                }
                items[item] = existing;
            }

            return GetResult(items, _isMostFrequent);
        }

        internal static Object GetResult(IDictionary<Object, int?> items, bool mostFrequent)
        {
            if (mostFrequent)
            {
                Object maxKey = null;
                int max = Int32.MinValue;
                foreach (var entry in items)
                {
                    if (entry.Value > max)
                    {
                        maxKey = entry.Key;
                        max = entry.Value.Value;
                    }
                }
                return maxKey;
            }

            int min = Int32.MaxValue;
            Object minKey = null;
            foreach (var entry in items)
            {
                if (entry.Value < min)
                {
                    minKey = entry.Key;
                    min = entry.Value.Value;
                }
            }
            return minKey;
        }
    }
}