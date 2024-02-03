///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    /// <summary>
    ///     Strategy for use in poll-based joins to reduce a cached result set (represented by <seealso cref="EventTable" />),
    ///     in
    ///     which the cache result set may have been indexed, to fewer rows following the join-criteria in a where clause.
    /// </summary>
    public interface HistoricalIndexLookupStrategy
    {
        /// <summary>
        ///     Look up into the index, potentially using some of the properties in the lookup event,
        ///     returning a partial or full result in respect to the index.
        /// </summary>
        /// <param name="lookupEvent">provides properties to use as key values for indexes</param>
        /// <param name="index">is the table providing the cache result set, potentially indexed by index fields</param>
        /// <param name="context">context</param>
        /// <returns>full set or partial index iterator</returns>
        IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] index,
            ExprEvaluatorContext context);
    }
} // end of namespace