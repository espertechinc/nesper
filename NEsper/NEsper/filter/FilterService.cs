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

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Interface for filtering events by event type and event property values. Allows adding and removing filters.
    /// <para /> 
    /// Filters are defined by a <seealso cref="FilterSpecCompiled" /> and are associated with a
    /// <seealso cref="FilterHandle" /> callback. Implementations may decide if the same filter callback can be
    /// registered twice for different or some filter specifications. 
    /// <para /> 
    /// The performance of an implementation of this service is crucial in achieving a high overall event throughput.
    /// </summary>
    public interface FilterService : IDisposable
    {
        /// <summary>
        /// Finds matching filters to the event passed in and collects their associated callback method.
        /// </summary>
        /// <param name="theEvent">is the event to be matched against filters</param>
        /// <param name="matches">is a collection that is populated via add method with any handles for matching filters</param>
        /// <returns>filter current version</returns>
        long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches);

        /// <summary>
        /// Finds matching filters to the event passed in and collects their associated callback method, for a particular statement only
        /// </summary>
        /// <param name="theEvent">is the event to be matched against filters</param>
        /// <param name="matches">is a collection that is populated via add method with any handles for matching filters</param>
        /// <param name="statementId">statement for which to return results for</param>
        /// <returns>filter current version</returns>
        long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches, int statementId);

        /// <summary>
        /// Add a filter for events as defined by the filter specification, and register a callback to be invoked upon evaluation of an event that matches the filter spec.
        /// </summary>
        /// <param name="filterValueSet">is a specification of filter parameters, containsevent type information, event property values and operators</param>
        /// <param name="callback">is the callback to be invoked when the filter matches an event</param>
        FilterServiceEntry Add(FilterValueSet filterValueSet, FilterHandle callback);

        /// <summary>
        /// Remove a filter callback.
        /// </summary>
        /// <param name="callback">is the callback to be removed</param>
        /// <param name="filterServiceEntry">The filter service entry.</param>
        void Remove(FilterHandle callback, FilterServiceEntry filterServiceEntry);

        /// <summary>
        /// Return a count of the number of events evaluated by this service.
        /// </summary>
        /// <value>count of invocations of evaluate method</value>
        long NumEventsEvaluated { get; }

        /// <summary>
        /// Reset the number of events evaluated
        /// </summary>
        void ResetStats();

        /// <summary>
        /// Returns filter version.
        /// </summary>
        /// <value>filter version</value>
        long FiltersVersion { get; }

        void RemoveType(EventType type);
    }
}
