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
    public class EnumEvalOrderByAscDescEvents
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _descending;

        public EnumEvalOrderByAscDescEvents(ExprEvaluator innerExpression, int streamCountIncoming, bool descending)
            : base(innerExpression, streamCountIncoming)
        {
            this._descending = descending;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var sort = new OrderedDictionary<IComparable, Object>();
            var hasColl = false;

            foreach (EventBean next in target)
            {
                eventsLambda[StreamNumLambda] = next;

                var comparable = (IComparable)InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                var entry = sort.Get(comparable);

                if (entry == null)
                {
                    sort.Put(comparable, next);
                    continue;
                }

                if (entry is LinkedList<object>)
                {
                    ((LinkedList<object>)entry).AddLast(next);
                    continue;
                }

                var linkedList = new LinkedList<Object>();
                linkedList.AddLast(entry);
                linkedList.AddLast(next);
                sort.Put(comparable, linkedList);
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
                if (entry.Value is LinkedList<Object>)
                {
                    foreach (var value in (LinkedList<Object>)entry.Value)
                    {
                        coll.AddLast(value);
                    }
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