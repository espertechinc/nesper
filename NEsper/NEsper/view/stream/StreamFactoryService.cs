///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.filter;

namespace com.espertech.esper.view.stream
{
    /// <summary>
    /// Service on top of the filter service for reuseing filter callbacks and their associated
    /// EventStream instances. Same filter specifications (equal) do not need to be added to the 
    /// filter service twice and the EventStream instance that is the stream of events for that 
    /// filter can be reused. 
    /// <para/>
    /// We are re-using streams such that views under such streams can be reused for efficient 
    /// resource use.
    /// </summary>
    public interface StreamFactoryService
    {
        /// <summary>
        /// Create or reuse existing EventStream instance representing that event filter. When called for some filters, should return same stream.
        /// </summary>
        /// <param name="statementId">the statement id</param>
        /// <param name="filterSpec">event filter definition</param>
        /// <param name="filterService">filter service to activate filter if not already active</param>
        /// <param name="epStatementAgentInstanceHandle">is the statements-own handle for use in registering callbacks with services</param>
        /// <param name="isJoin">is indicatng whether the stream will participate in a join statement, informationnecessary for stream reuse and multithreading concerns</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="hasOrderBy">if the consumer has order-by</param>
        /// <param name="filterWithSameTypeSubselect">if set to <c>true</c> [filter with same type subselect].</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="stateless">if set to <c>true</c> [stateless].</param>
        /// <param name="streamNum">The stream num.</param>
        /// <param name="isCanIterateUnbound">if set to <c>true</c> [is can iterate unbound].</param>
        /// <returns>
        /// event stream representing active filter
        /// </returns>
        Pair<EventStream, IReaderWriterLock> CreateStream(
            int statementId,
            FilterSpecCompiled filterSpec,
            FilterService filterService,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            bool isJoin,
            AgentInstanceContext agentInstanceContext,
            bool hasOrderBy,
            bool filterWithSameTypeSubselect,
            Attribute[] annotations,
            bool stateless,
            int streamNum,
            bool isCanIterateUnbound);

        /// <summary>
        /// Drop the event stream associated with the filter passed in. Throws an exception if already dropped.
        /// </summary>
        /// <param name="filterSpec">is the event filter definition associated with the event stream to be dropped</param>
        /// <param name="filterService">to be used to deactivate filter when the last event stream is dropped</param>
        /// <param name="isJoin">is indicatng whether the stream will participate in a join statement, informationnecessary for stream reuse and multithreading concerns</param>
        /// <param name="hasOrderBy">if the consumer has an order-by clause</param>
        /// <param name="filterWithSameTypeSubselect">if set to <c>true</c> [filter with same type subselect].</param>
        /// <param name="stateless">if set to <c>true</c> [stateless].</param>
        void DropStream(FilterSpecCompiled filterSpec, FilterService filterService, bool isJoin, bool hasOrderBy, bool filterWithSameTypeSubselect, bool stateless);
    
        /// <summary>Dispose the service. </summary>
        void Destroy();
    }
}
