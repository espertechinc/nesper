///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.@internal.filtersvc
{
    /// <summary>
    ///     Interface for a callback method to be called when an event matches a filter specification. Provided as a
    ///     convenience
    ///     for use as a filter handle for registering with the <seealso cref="FilterService" />.
    /// </summary>
    public interface FilterHandleCallback
    {
        /// <summary>
        ///     Returns true if the filter applies to subselects.
        /// </summary>
        bool IsSubSelect { get; }

        /// <summary>
        ///     Indicate that an event was evaluated by the <seealso cref="FilterService" /> which
        ///     matches the filter specification <seealso cref="FilterSpecCompiled" /> associated with this callback.
        /// </summary>
        /// <param name="theEvent">the event received that matches the filter specification</param>
        /// <param name="allStmtMatches">All STMT matches.</param>
        void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches);
    }
}