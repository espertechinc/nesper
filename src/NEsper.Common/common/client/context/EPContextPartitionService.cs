///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     Service interface for administration of contexts and context partitions.
    /// </summary>
    public interface EPContextPartitionService
    {
        /// <summary>
        ///     Returns the statement names associated to the context of the given name.
        ///     <para />
        ///     Returns null if a context declaration for the name does not exist.
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name to return statements for</param>
        /// <returns>
        ///     statement names, or null if the context does not exist, or empty list if no statements areassociated to the context
        ///     (counting started and stopped statements, not destroyed ones).
        /// </returns>
        string[] GetContextStatementNames(
            string deploymentId,
            string contextName);

        /// <summary>
        ///     Returns the nesting level for the context declaration, i.e. 1 for unnested and &gt;1 for nested contexts.
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <returns>nesting level</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        int GetContextNestingLevel(
            string deploymentId,
            string contextName);

        /// <summary>
        ///     Returns information about selected context partitions including state.
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>collection of the context partition ids and descriptors</returns>
        /// <throws>ArgumentException        if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ContextPartitionCollection GetContextPartitions(
            string deploymentId,
            string contextName,
            ContextPartitionSelector selector);

        /// <summary>
        ///     Returns the context partition ids.
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <param name="selector">a selector that identifies the context partitions</param>
        /// <returns>set of the context partition ids</returns>
        /// <throws>ArgumentException        if a context by that name was not declared</throws>
        /// <throws>InvalidContextPartitionSelector if the selector type and context declaration mismatch</throws>
        ISet<int> GetContextPartitionIds(
            string deploymentId,
            string contextName,
            ContextPartitionSelector selector);

        /// <summary>
        ///     Returning the descriptor of a given context partition.
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <param name="agentInstanceId">the context partition id number</param>
        /// <returns>identifier or null if the context partition is not found</returns>
        /// <throws>ArgumentException if a context by that name was not declared</throws>
        ContextPartitionIdentifier GetIdentifier(
            string deploymentId,
            string contextName,
            int agentInstanceId);

        /// <summary>
        ///     Add a context state listener
        /// </summary>
        /// <param name="listener">to add</param>
        void AddContextStateListener(ContextStateListener listener);

        /// <summary>
        ///     Remove a context state listener
        /// </summary>
        /// <param name="listener">to remove</param>
        void RemoveContextStateListener(ContextStateListener listener);

        /// <summary>
        ///     Returns an iterator of context state listeners (read-only)
        /// </summary>
        /// <value>listeners</value>
        IEnumerator<ContextStateListener> ContextStateListeners { get; }

        /// <summary>
        ///     Removes all context state listener
        /// </summary>
        void RemoveContextStateListeners();

        /// <summary>
        ///     Add context partition state listener for the given context
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <param name="listener">to add</param>
        void AddContextPartitionStateListener(
            string deploymentId,
            string contextName,
            ContextPartitionStateListener listener);

        /// <summary>
        ///     Remove a context partition state listener for the given context
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <param name="listener">to remove</param>
        void RemoveContextPartitionStateListener(
            string deploymentId,
            string contextName,
            ContextPartitionStateListener listener);

        /// <summary>
        ///     Returns an iterator of context partition state listeners (read-only) for the given context
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <returns>listeners</returns>
        IEnumerator<ContextPartitionStateListener> GetContextPartitionStateListeners(
            string deploymentId,
            string contextName);

        /// <summary>
        ///     Removes all context partition state listener for the given context
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        void RemoveContextPartitionStateListeners(
            string deploymentId,
            string contextName);

        /// <summary>
        ///     Returns the context properties for a given deployment id, context name and context partition id.
        /// </summary>
        /// <param name="deploymentId">deployment id of context (deployment id of create-context statement)</param>
        /// <param name="contextName">context name</param>
        /// <param name="contextPartitionId">context partition id</param>
        /// <returns>map of built-in properties wherein values representing event are EventBean instances</returns>
        IDictionary<string, object> GetContextProperties(
            string deploymentId,
            string contextName,
            int contextPartitionId);
    }
} // end of namespace