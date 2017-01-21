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
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Strategy for looking up, in some sort of table or index, or a set of events, potentially 
    /// based on the events properties, and returning a set of matched events.
    /// </summary>
    public interface SubordTableLookupStrategy
    {
        /// <summary>
        /// Returns matched events for a set of events to look up for. Never returns an empty 
        /// result set, always returns null to indicate no results.
        /// </summary>
        /// <param name="events">to look up</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// set of matching events, or null if none matching
        /// </returns>
        ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context);
    
        String ToQueryPlan();

        LookupStrategyDesc StrategyDesc { get; }
    }
}
