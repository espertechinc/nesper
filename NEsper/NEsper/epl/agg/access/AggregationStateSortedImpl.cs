///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationStateSortedImpl : AggregationStateWithSize, AggregationStateSorted
    {
        protected readonly AggregationStateSortedSpec Spec;
        protected readonly SortedDictionary<Object, Object> Sorted;
        protected int Size;
    
        /// <summary>Ctor. </summary>
        /// <param name="spec">aggregation spec</param>
        public AggregationStateSortedImpl(AggregationStateSortedSpec spec) {
            Spec = spec;
            Sorted = new SortedDictionary<Object, Object>(spec.Comparator);
        }
    
        public void Clear() {
            Sorted.Clear();
            Size = 0;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }
            if (ReferenceEvent(theEvent)) {
                Object comparable = GetComparable(Spec.Criteria, eventsPerStream, true, exprEvaluatorContext);
                Object existing = Sorted.Get(comparable);
                if (existing == null) {
                    Sorted.Put(comparable, theEvent);
                }
                else if (existing is EventBean) {
                    var coll = new LinkedList<object>();
                    coll.AddLast(existing);
                    coll.AddLast(theEvent);
                    Sorted.Put(comparable, coll);
                }
                else {
                    var q = (LinkedList<object>)existing;
                    q.AddLast(theEvent);
                }
                Size++;
            }
        }
    
        protected virtual bool ReferenceEvent(EventBean theEvent) {
            // no action
            return true;
        }
    
        protected virtual bool DereferenceEvent(EventBean theEvent) {
            // no action
            return true;
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }
            if (DereferenceEvent(theEvent)) {
                Object comparable = GetComparable(Spec.Criteria, eventsPerStream, false, exprEvaluatorContext);
                Object existing = Sorted.Get(comparable);
                if (existing != null) {
                    if (existing.Equals(theEvent)) {
                        Sorted.Remove(comparable);
                        Size--;
                    }
                    else if (existing is LinkedList<object>)
                    {
                        var q = (LinkedList<object>)existing;
                        q.Remove(theEvent);
                        if (q.IsEmpty()) {
                            Sorted.Remove(comparable);
                        }
                        Size--;
                    }
                }
            }
        }

        public EventBean FirstValue
        {
            get
            {
                if (Sorted.IsEmpty())
                {
                    return null;
                }
                var max = Sorted.Values.First();
                return CheckedPayload(max);
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (Sorted.IsEmpty())
                {
                    return null;
                }
                var min = Sorted.Values.Last();
                return CheckedPayload(min);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return new AggregationStateSortedEnumerator(Sorted, false);
        }
    
        public IEnumerator<EventBean> GetReverseEnumerator()
        {
            return new AggregationStateSortedEnumerator(Sorted, true);
        }
    
        public ICollection<EventBean> CollectionReadOnly()
        {
            return new AggregationStateSortedWrappingCollection(Sorted, Size);
        }

        public int Count
        {
            get { return Size; }
        }

        public static Object GetComparable(ExprEvaluator[] criteria, EventBean[] eventsPerStream, bool istream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (criteria.Length == 1) {
                return criteria[0].Evaluate(new EvaluateParams(eventsPerStream, istream, exprEvaluatorContext));
            }
            else
            {
                var result = new Object[criteria.Length];
                var count = 0;
                foreach (ExprEvaluator expr in criteria) {
                    result[count++] = expr.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                }
                return new MultiKeyUntyped(result);
            }
        }
    
        private EventBean CheckedPayload(Object value)
        {
            if (value is EventBean) {
                return (EventBean) value;
            } 
            else if (value is IEnumerable<EventBean>)
            {
                return ((IEnumerable<EventBean>) value).First();
            }
            else if (value is IEnumerable)
            {
                var @enum = ((IEnumerable) value).GetEnumerator();
                @enum.MoveNext();
                return @enum.Current as EventBean;
            }

            throw new ArgumentException("invalid value", "value");
        }
    }
}
