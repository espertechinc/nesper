///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.filtersvc
{
    /// <summary>
    ///     Interface for filtering events by event type and event property values. Allows adding and removing filters.
    ///     <para />
    ///     Filters are defined by a <seealso cref="FilterSpecActivatable" /> and are associated with a
    ///     <seealso cref="FilterHandle" />callback.
    ///     Implementations may decide if the same filter callback can be registered twice for different or some
    ///     filter specifications.
    ///     <para />
    ///     The performance of an implementation of this service is crucial in achieving a high overall event throughput.
    /// </summary>
    public interface FilterService
    {
        /// <summary>
        ///     Return a count of the number of events evaluated by this service.
        /// </summary>
        /// <returns>count of invocations of evaluate method</returns>
        long NumEventsEvaluated { get; }

        /// <summary>
        ///     Returns filter version.
        /// </summary>
        /// <returns>filter version</returns>
        long FiltersVersion { get; }

        /// <summary>
        ///     Finds matching filters to the event passed in and collects their associated callback method.
        /// </summary>
        /// <param name="theEvent">is the event to be matched against filters</param>
        /// <param name="matches">is a collection that is populated via add method with any handles for matching filters</param>
        /// <param name="ctx"></param>
        /// <returns>filter current version</returns>
        long Evaluate(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx);

        /// <summary>
        ///     Finds matching filters to the event passed in and collects their associated callback method, for a particular
        ///     statement only
        /// </summary>
        /// <param name="theEvent">is the event to be matched against filters</param>
        /// <param name="matches">is a collection that is populated via add method with any handles for matching filters</param>
        /// <param name="statementId">statement for which to return results for</param>
        /// <param name="ctx"></param>
        /// <returns>filter current version</returns>
        long Evaluate(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            int statementId,
            ExprEvaluatorContext ctx);

        /// <summary>
        ///     Add a filter for events as defined by the filter specification, and register a
        ///     callback to be invoked upon evaluation of an event that matches the filter spec.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="valueSet">
        ///     is a specification of filter parameters, containsevent type information, event property values and operators
        /// </param>
        /// <param name="callback">is the callback to be invoked when the filter matches an event</param>
        void Add(
            EventType eventType,
            FilterValueSetParam[][] valueSet,
            FilterHandle callback);

        /// <summary>
        ///     Remove a filter callback.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="valueSet">values</param>
        /// <param name="callback">is the callback to be removed</param>
        void Remove(
            FilterHandle callback,
            EventType eventType,
            FilterValueSetParam[][] valueSet);

        /// <summary>
        ///     Reset the number of events evaluated
        /// </summary>
        void ResetStats();

        /// <summary>
        ///     Destroy the service.
        /// </summary>
        void Destroy();

        void RemoveType(EventType type);

        void AcquireWriteLock();

        void ReleaseWriteLock();
    }
} // end of namespace