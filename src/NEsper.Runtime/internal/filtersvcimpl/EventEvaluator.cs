///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Interface for matching an event instance based on the event's property values to
    ///     filters, specifically filter parameter constants or ranges.
    /// </summary>
    public interface EventEvaluator
    {
        /// <summary>
        ///     Perform the matching of an event based on the event property values, adding any callbacks for matches found to
        ///     the matches list.
        /// </summary>
        /// <param name="theEvent">is the event object wrapper to obtain event property values from</param>
        /// <param name="matches">accumulates the matching filter callbacks</param>
        /// <param name="ctx">evaluator context</param>
        void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx);

        void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack);
    }
}