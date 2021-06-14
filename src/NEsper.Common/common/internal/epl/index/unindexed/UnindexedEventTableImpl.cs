///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.unindexed
{
    /// <summary>
    ///     Simple table of events without an index.
    /// </summary>
    public class UnindexedEventTableImpl : UnindexedEventTable
    {
        private readonly ISet<EventBean> eventSet = new LinkedHashSet<EventBean>();

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the indexed stream's number</param>
        public UnindexedEventTableImpl(int streamNum)
            : base(streamNum)
        {
        }

        public override bool IsEmpty => eventSet.IsEmpty();

        /// <summary>
        ///     Returns events in table.
        /// </summary>
        /// <returns>all events</returns>
        public override ISet<EventBean> EventSet => eventSet;

        public override int? NumberOfEvents => eventSet.Count;

        public override object Index => eventSet;

        public override Type ProviderClass => typeof(UnindexedEventTable);

        public override void Clear()
        {
            eventSet.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }

        public override void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QIndexAddRemove(this, newData, oldData);

            if (newData != null) {
                eventSet.AddAll(newData);
            }

            if (oldData != null) {
                foreach (var removeEvent in oldData) {
                    eventSet.Remove(removeEvent);
                }
            }

            exprEvaluatorContext.InstrumentationProvider.AIndexAddRemove();
        }

        public override void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null) {
                eventSet.AddAll(events);
            }
        }

        public override void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null) {
                foreach (var removeEvent in events) {
                    eventSet.Remove(removeEvent);
                }
            }
        }

        public override void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventSet.Add(@event);
        }

        public override void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            eventSet.Remove(@event);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return eventSet.GetEnumerator();
        }
    }
} // end of namespace