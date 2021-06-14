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

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    /// <summary>
    ///     Table of events allowing add and remove. Lookup in table is coordinated
    ///     through the underlying implementation.
    /// </summary>
    public interface EventTable : IEnumerable<EventBean>
    {
        Type ProviderClass { get; }

        /// <summary>
        ///     If the number of events is readily available, an implementation will return that number
        ///     or it may return null to indicate that the count is not readily available.
        /// </summary>
        /// <returns>number of events</returns>
        int? NumberOfEvents { get; }

        /// <summary>
        ///     If the index retains events using some key-based organization this returns the number of keys,
        ///     and may return -1 to indicate that either the number of keys is not available or
        ///     costly to obtain.
        ///     <para />
        ///     The number returned can be an estimate and may not be accurate.
        /// </summary>
        /// <returns>number of keys</returns>
        int NumKeys { get; }

        /// <summary>
        ///     Return the index object itself, or an object-array for multiple index structures.
        ///     <para />
        ///     May return null if the information is not readily available, i.e. externally maintained index
        /// </summary>
        /// <returns>index object</returns>
        object Index { get; }

        EventTableOrganization Organization { get; }

        /// <summary>
        ///     Add and remove events from table.
        ///     <para />
        ///     It is up to the index to decide whether to add first and then remove,
        ///     or whether to remove and then add.
        ///     <para />
        ///     It is important to note that a given event can be in both the
        ///     removed and the added events. This means that unique indexes probably need to remove first
        ///     and then add. Most other non-unique indexes will add first and then remove
        ///     since the an event can be both in the add and the remove stream.
        /// </summary>
        /// <param name="newData">to add</param>
        /// <param name="oldData">to remove</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        ///     Add events to table.
        /// </summary>
        /// <param name="events">to add</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        ///     Add event to table.
        /// </summary>
        /// <param name="event">to add</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        ///     Remove events from table.
        /// </summary>
        /// <param name="events">to remove</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        ///     Remove event from table.
        /// </summary>
        /// <param name="event">to remove</param>
        /// <param name="exprEvaluatorContext">evaluator context</param>
        void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

#if INTENDED_HIDE
/// <summary>
///     Returns an iterator over events in the table. Not required to be implemented for all indexes.
///     Full table scans and providers that have easy access to an iterator may implement.
/// </summary>
/// <returns>table iterator</returns>
/// <throws>UnsupportedOperationException for operation not supported for this type of index</throws>
        IEnumerator<EventBean> GetEnumerator();
#endif

        /// <summary>
        ///     Returns true if the index is definitely empty,
        ///     or false if is not definitely empty but we can not certain.
        /// </summary>
        /// <value>true for definitely empty index, false for there-may-be-rows and please-check-by-iterating</value>
        bool IsEmpty { get; }

        /// <summary>
        ///     Clear out index.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Destroy index.
        /// </summary>
        void Destroy();

        string ToQueryPlan();
    }
} // end of namespace