///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Processor _prototype for result sets for instances that apply the select-clause, group-by-clause and having-clauses as supplied.
    /// </summary>
    public interface ResultSetProcessorFactory
    {
        /// <summary>
        /// Returns the event type of processed results.
        /// </summary>
        /// <value>event type of the resulting events posted by the processor.</value>
        EventType ResultEventType { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has aggregation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has aggregation; otherwise, <c>false</c>.
        /// </value>
        bool HasAggregation { get; }

        /// <summary>
        /// Instantiates the specified order by processor.
        /// </summary>
        /// <param name="orderByProcessor">The order by processor.</param>
        /// <param name="aggregationService">The aggregation service.</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <returns></returns>
        ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext);

        /// <summary>
        /// Gets the type of the result set processor.
        /// </summary>
        /// <value>
        /// The type of the result set processor.
        /// </value>
        ResultSetProcessorType ResultSetProcessorType { get; }
    }
}
