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
    public class EnumEvalOrderByAscDescScalarLambda
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _descending;
        private readonly ObjectArrayEventType _resultEventType;

        public EnumEvalOrderByAscDescScalarLambda(ExprEvaluator innerExpression, int streamCountIncoming, bool descending, ObjectArrayEventType resultEventType)
            : base(innerExpression, streamCountIncoming)
        {
            _descending = descending;
            _resultEventType = resultEventType;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var sort = new OrderedDictionary<IComparable, Object>();
            var hasColl = false;

            var resultEvent = new ObjectArrayEventBean(new Object[1], _resultEventType);
            var values = (ICollection<Object>)target;
            foreach (Object next in values)
            {
                resultEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = resultEvent;

                var comparable = (IComparable)InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                var entry = sort.Get(comparable);

                if (entry == null)
                {
                    sort.Put(comparable, next);
                    continue;
                }

                if (entry is ICollection<Object>)
                {
                    ((ICollection<Object>)entry).Add(next);
                    continue;
                }

                var mcoll = new LinkedList<Object>();
                mcoll.AddLast(entry);
                mcoll.AddLast(next);
                sort.Put(comparable, mcoll);
                hasColl = true;
            }

            IDictionary<IComparable, Object> sorted;
            if (_descending)
            {
                sorted = sort.Invert();
            }
            else
            {
                sorted = sort;
            }

            if (!hasColl)
            {
                return sorted.Values;
            }

            var coll = new LinkedList<Object>();
            foreach (var entry in sorted)
            {
                if (entry.Value is ICollection<object>)
                {
                    coll.AddAll((ICollection<object>)entry.Value);
                }
                else
                {
                    coll.AddLast(entry.Value);
                }
            }
            return coll;
        }
    }
}
