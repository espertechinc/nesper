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

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationStateMinMaxByEver
        : AggregationState
        , AggregationStateSorted
    {
        protected readonly AggregationStateMinMaxByEverSpec Spec;
        protected EventBean CurrentMinMaxBean;
        protected Object CurrentMinMax;

        public AggregationStateMinMaxByEver(AggregationStateMinMaxByEverSpec spec)
        {
            Spec = spec;
        }

        public void Clear()
        {
            CurrentMinMax = null;
            CurrentMinMaxBean = null;
        }

        public virtual void ApplyEnter(
            EventBean[] eventsPerStream, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }

            AddEvent(theEvent, eventsPerStream, exprEvaluatorContext);
        }

        public virtual void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            // this is an ever-type aggregation
        }

        public EventBean FirstValue
        {
            get
            {
                if (Spec.IsMax)
                {
                    throw new UnsupportedOperationException("Only accepts max-value queries");
                }
                return CurrentMinMaxBean;
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (!Spec.IsMax)
                {
                    throw new UnsupportedOperationException("Only accepts min-value queries");
                }
                return CurrentMinMaxBean;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException();
        }

        public IEnumerator<EventBean> GetReverseEnumerator()
        {
            throw new UnsupportedOperationException();
        }

        public ICollection<EventBean> CollectionReadOnly()
        {
            if (CurrentMinMaxBean != null)
            {
                return Collections.SingletonList(CurrentMinMaxBean);
            }
            return null;
        }

        public int Count
        {
            get { return CurrentMinMax == null ? 0 : 1; }
        }

        protected void AddEvent(
            EventBean theEvent,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Object comparable = AggregationStateSortedImpl.GetComparable(
                Spec.Criteria, eventsPerStream, true, exprEvaluatorContext);
            if (CurrentMinMax == null)
            {
                CurrentMinMax = comparable;
                CurrentMinMaxBean = theEvent;
            }
            else
            {
                int compareResult = Spec.Comparator.Compare(CurrentMinMax, comparable);
                if (Spec.IsMax)
                {
                    if (compareResult < 0)
                    {
                        CurrentMinMax = comparable;
                        CurrentMinMaxBean = theEvent;
                    }
                }
                else
                {
                    if (compareResult > 0)
                    {
                        CurrentMinMax = comparable;
                        CurrentMinMaxBean = theEvent;
                    }
                }
            }
        }
    }
}
