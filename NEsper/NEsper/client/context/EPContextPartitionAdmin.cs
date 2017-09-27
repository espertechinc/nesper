///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.context
{
    /// <summary>
    /// Service interface for administration of contexts and context partitions.
    /// </summary>
    public interface EPContextPartitionAdmin
    {
        /// <summary>
        /// Returns the statement names associated to the context of the given name.
        /// <para />Returns null if a context declaration for the name does not exist.
        /// </summary>
        /// <param name="contextName">context name to return statements for</param>
        /// <returns>statement names, or null if the context does not exist, or empty list if no statements areassociated to the context (counting started and stopped statements, not destroyed ones).
        /// </returns>
        string[] GetContextStatementNames(string contextName);
    
        /// <summary>
        /// Returns the nesting level for the context declaration, i.e. 1 for unnested and &gt;1 for nested contexts.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <returns>nesting level</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        int GetContextNestingLevel(string contextName);
    
        /// <summary>
        /// Dispose one or more context partitions dropping the associated state and removing associated context partition metadata.
        /// <para />For key-partitioned contexts and hash-segmented contexts the next event for such
        /// context partition allocates a new context partition for that key or hash.
        /// <para />If context partitions cannot be found they are not part of the collection returned.
        /// Only context partitions in stopped or started state can be destroyed.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>collection of the destroyed context partition ids and descriptors</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ContextPartitionCollection DestroyContextPartitions(string contextName, ContextPartitionSelector selector);
    
        /// <summary>
        /// Stop one or more context partitions that are currently started, dropping the associated state and but keeping
        /// associated context partition metadata for the purpose of starting it again.
        /// <para />Stopping a context partition means any associated statements no longer process
        /// events or time for that context partition only, and dropping all such associated state.
        /// <para />If context partitions cannot be found they are not part of the collection returned.
        /// Stopped context partitions remain stopped and are not returned.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>collection of the stopped context partition ids and descriptors</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ContextPartitionCollection StopContextPartitions(string contextName, ContextPartitionSelector selector);
    
        /// <summary>
        /// Start one or more context partitions that were previously stopped.
        /// <para />Starting a context partition means any associated statements beging to process
        /// events or time for that context partition, starting fresh with newly allocated state.
        /// <para />If context partitions cannot be found they are not part of the collection returned.
        /// Started context partitions remain started and are not returned.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>collection of the started context partition ids and descriptors</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ContextPartitionCollection StartContextPartitions(string contextName, ContextPartitionSelector selector);
    
        /// <summary>
        /// Returns information about selected context partitions including state.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>collection of the context partition ids and descriptors</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ContextPartitionCollection GetContextPartitions(string contextName, ContextPartitionSelector selector);
    
        /// <summary>
        /// Returns the context partition ids.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>set of the context partition ids</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ISet<int> GetContextPartitionIds(string contextName, ContextPartitionSelector selector);
    
        /// <summary>
        /// Dispose the context partition returning its descriptor.
        /// <para />For key-partitioned contexts and hash-segmented contexts the next event for such
        /// context partition allocates a new context partition for that key or hash.
        /// <para />Only context partitions in stopped or started state can be destroyed.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="agentInstanceId">the context partition id number</param>
        /// <returns>descriptor or null if the context partition is not found</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        ContextPartitionDescriptor DestroyContextPartition(string contextName, int agentInstanceId);
    
        /// <summary>
        /// Stop the context partition if it is currently started and returning its descriptor.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="agentInstanceId">the context partition id number</param>
        /// <returns>descriptor or null if the context partition is not found or is already stopped</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        ContextPartitionDescriptor StopContextPartition(string contextName, int agentInstanceId);
    
        /// <summary>
        /// Start the context partition if it is currently stopped and returning its descriptor.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="agentInstanceId">the context partition id number</param>
        /// <returns>descriptor or null if the context partition is not found or is already started</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        ContextPartitionDescriptor StartContextPartition(string contextName, int agentInstanceId);
    
        /// <summary>
        /// Returning the descriptor of a given context partition.
        /// </summary>
        /// <param name="contextName">context name</param>
        /// <param name="agentInstanceId">the context partition id number</param>
        /// <returns>descriptor or null if the context partition is not found</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        ContextPartitionDescriptor GetDescriptor(string contextName, int agentInstanceId);
    }
}
