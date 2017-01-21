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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Strategy for use in poll-based joins to reduce a cached result set (represented by <see cref="EventTable" />), 
    /// in which the cache result set may have been indexed, to fewer rows following the join-criteria in a where clause.
    /// </summary>
    public interface HistoricalIndexLookupStrategy
    {
        /// <summary>
        /// Look up into the index, potentially using some of the properties in the procLookup event, returning a partial or full result in respect to the index.
        /// </summary>
        /// <param name="lookupEvent">provides properties to use as key values for indexes</param>
        /// <param name="index">is the table providing the cache result set, potentially indexed by index fields</param>
        /// <param name="context">The context.</param>
        /// <returns>full set or partial index iterator</returns>
        IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] index, ExprEvaluatorContext context);
    
        String ToQueryPlan();
    }

    public class ProxyHistoricalIndexLookupStrategy : HistoricalIndexLookupStrategy
    {
        public Func<EventBean, EventTable[], ExprEvaluatorContext, IEnumerator<EventBean>> ProcLookup { get; set; }
        public Func<string> ProcToQueryPlan { get; set; }

        public ProxyHistoricalIndexLookupStrategy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyHistoricalIndexLookupStrategy"/> class.
        /// </summary>
        /// <param name="procLookup">The procLookup.</param>
        /// <param name="procToQueryPlan">To plan.</param>
        public ProxyHistoricalIndexLookupStrategy(Func<EventBean, EventTable[], ExprEvaluatorContext, IEnumerator<EventBean>> procLookup,
                                                  Func<string> procToQueryPlan)
        {
            ProcLookup = procLookup;
            ProcToQueryPlan = procToQueryPlan;
        }

        /// <summary>
        /// Look up into the index, potentially using some of the properties in the procLookup event, returning a partial or full result in respect to the index.
        /// </summary>
        /// <param name="lookupEvent">provides properties to use as key values for indexes</param>
        /// <param name="index">is the table providing the cache result set, potentially indexed by index fields</param>
        /// <param name="context">The context.</param>
        /// <returns>full set or partial index iterator</returns>
        public IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] index, ExprEvaluatorContext context)
        {
            return ProcLookup.Invoke(lookupEvent, index, context);
        }

        public string ToQueryPlan()
        {
            return ProcToQueryPlan.Invoke();
        }
    }
}
