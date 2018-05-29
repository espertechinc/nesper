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
    
        public virtual void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }
            ReferenceAdd(theEvent, eventsPerStream, exprEvaluatorContext);
        }
    
        protected virtual bool ReferenceEvent(EventBean theEvent) {
            // no action
            return true;
        }
    
        protected virtual bool DereferenceEvent(EventBean theEvent) {
            // no action
            return true;
        }
    
        public virtual void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }
            DereferenceRemove(theEvent, eventsPerStream, exprEvaluatorContext);
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

        public int Count => Size;

        public static Object GetComparable(
            ExprEvaluator[] criteria,
            EventBean[] eventsPerStream,
            bool istream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (criteria.Length == 1) {
                return criteria[0].Evaluate(new EvaluateParams(eventsPerStream, istream, exprEvaluatorContext));
            }
            else {
                var result = new Object[criteria.Length];
                var count = 0;
                foreach (var expr in criteria) {
                    result[count++] = expr.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                }

                return new MultiKeyUntyped(result);
            }
        }

        protected void ReferenceAdd(
            EventBean theEvent, 
            EventBean[] eventsPerStream, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (ReferenceEvent(theEvent))
            {
                var comparable = GetComparable(Spec.Criteria, eventsPerStream, true, exprEvaluatorContext);
                var existing = Sorted.Get(comparable);
                if (existing == null)
                {
                    Sorted.Put(comparable, theEvent);
                }
                else if (existing is EventBean eventBean) {
                    var coll = new ArrayDeque<EventBean>(2);
                    coll.Add(eventBean);
                    coll.Add(theEvent);
                    Sorted.Put(comparable, coll);
                } else {
                    var arrayDeque = (ArrayDeque<EventBean>) existing;
                    arrayDeque.Add(theEvent);
                }
                Size++;
            }
        }

        protected void DereferenceRemove(
            EventBean theEvent, 
            EventBean[] eventsPerStream, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (DereferenceEvent(theEvent))
            {
                var comparable = GetComparable(Spec.Criteria, eventsPerStream, false, exprEvaluatorContext);
                var existing = Sorted.Get(comparable);
                if (existing != null)
                {
                    if (existing.Equals(theEvent))
                    {
                        Sorted.Remove(comparable);
                        Size--;
                    }
                    else if (existing is ArrayDeque<EventBean> arrayDeque) {
                        arrayDeque.Remove(theEvent);
                        if (arrayDeque.IsEmpty())
                        {
                            Sorted.Remove(comparable);
                        }
                        Size--;
                    }
                }
            }
        }

        private EventBean CheckedPayload(Object value)
        {
            if (value is EventBean eventBean) {
                return eventBean;
            }
            else if (value is ArrayDeque<EventBean> arrayDeque) {
                return arrayDeque.First;
            }
            else if (value is IEnumerable<EventBean>) {
                return ((IEnumerable<EventBean>) value).First();
            }
            else if (value is IEnumerable) {
                var @enum = ((IEnumerable) value).GetEnumerator();
                @enum.MoveNext();
                return @enum.Current as EventBean;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }
    }
}
