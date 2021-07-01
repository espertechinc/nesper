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

namespace com.espertech.esper.common.@internal.epl.index.unindexed
{
    /// <summary>
    ///     Simple table of events without an index.
    /// </summary>
    public abstract class UnindexedEventTable : EventTable
    {
        private readonly int streamNum;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the indexed stream's number</param>
        public UnindexedEventTable(int streamNum)
        {
            this.streamNum = streamNum;
        }

        public abstract ISet<EventBean> EventSet { get; }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() + " streamNum=" + streamNum;
        }

        public int NumKeys => 0;

        public EventTableOrganization Organization => new EventTableOrganization(
            null,
            false,
            false,
            streamNum,
            null,
            EventTableOrganizationType.UNORGANIZED);

        public override string ToString()
        {
            return ToQueryPlan();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<EventBean> GetEnumerator();
        public abstract Type ProviderClass { get; }
        public abstract int? NumberOfEvents { get; }
        public abstract object Index { get; }

        public abstract void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract bool IsEmpty { get; }
        public abstract void Clear();
        public abstract void Destroy();
    }
} // end of namespace