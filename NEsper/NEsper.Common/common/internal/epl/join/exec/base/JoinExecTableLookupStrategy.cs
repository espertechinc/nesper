///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    /// Strategy for looking up, in some sort of table or index, an event, potentially based on the
    /// events properties, and returning a set of matched events.
    /// </summary>
    public interface JoinExecTableLookupStrategy
    {
        /// <summary>
        /// Returns matched events for a event to look up for. Never returns an empty result set,
        /// always returns null to indicate no results.
        /// </summary>
        /// <param name="theEvent">to look up</param>
        /// <param name="cursor">the path in the query that the lookup took</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>set of matching events, or null if none matching</returns>
        ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext);

        LookupStrategyType LookupStrategyType { get; }
    }
} // end of namespace