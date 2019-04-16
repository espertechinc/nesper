///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    /// <summary>
    ///     Simple table of events without an index, based on a List implementation rather then a set
    ///     since we know there cannot be duplicates (such as a poll returning individual rows).
    /// </summary>
    public class UnindexedEventTableList : EventTable
    {
        private readonly IList<EventBean> eventSet;
        private readonly int streamNum;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventSet">is a list initializing the table</param>
        /// <param name="streamNum">stream number</param>
        public UnindexedEventTableList(
            IList<EventBean> eventSet,
            int streamNum)
        {
            this.eventSet = eventSet;
            this.streamNum = streamNum;
        }

        public void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QIndexAddRemove(this, newData, oldData);

            if (newData != null) {
                CompatExtensions.AddAll(eventSet, newData);
            }

            if (oldData != null) {
                foreach (var removeEvent in oldData) {
                    eventSet.Remove(removeEvent);
                }
            }

            exprEvaluatorContext.InstrumentationProvider.AIndexAddRemove();
        }

        public void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null) {
                CompatExtensions.AddAll(eventSet, events);
            }
        }

        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null) {
                foreach (var removeEvent in events) {
                    eventSet.Remove(removeEvent);
                }
            }
        }

        public void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventSet.Add(@event);
        }

        public void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventSet.Remove(@event);
        }

        public bool IsEmpty => eventSet.IsEmpty();

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName();
        }

        public void Clear()
        {
            eventSet.Clear();
        }

        public void Destroy()
        {
            Clear();
        }

        public int? NumberOfEvents => eventSet.Count;

        public int NumKeys => 0;

        public object Index => eventSet;

        public EventTableOrganization Organization => new EventTableOrganization(
            null, false, false, streamNum, null, EventTableOrganizationType.UNORGANIZED);

        public Type ProviderClass => typeof(UnindexedEventTableList);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (eventSet == null) {
                return CollectionUtil.NULL_EVENT_ITERATOR;
            }

            return eventSet.GetEnumerator();
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }
    }
} // end of namespace