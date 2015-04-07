///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.context;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Interface for a prepared on-demand query that can be executed multiple times.
    /// </summary>
    public interface EPOnDemandPreparedQuery
    {
        /// <summary>
        /// Execute the prepared query returning query results.
        /// </summary>
        /// <returns>query result</returns>
        EPOnDemandQueryResult Execute();

        /// <summary>
        /// For use with named windows that have a context declared and that may therefore have multiple context 
        /// partitions, allows to target context partitions for query execution selectively.
        /// </summary>
        /// <param name="contextPartitionSelectors">selects context partitions to consider</param>
        /// <returns>query result</returns>
        EPOnDemandQueryResult Execute(ContextPartitionSelector[] contextPartitionSelectors);
        
        /// <summary>Returns the event type, representing the columns of the select-clause. </summary>
        /// <returns>event type</returns>
        EventType EventType { get; }
    }
}
