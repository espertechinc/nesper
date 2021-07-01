///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     Variables service for reading and writing variables, and for setting a version number for the current thread to
    ///     consider variables for.
    ///     <para />
    ///     See implementation class for further details.
    /// </summary>
    public interface VariableManagementService
    {
        /// <summary>
        ///     Lock for use in atomic writes to the variable space.
        /// </summary>
        /// <returns>read write lock for external coordinated write</returns>
        IReaderWriterLock ReadWriteLock { get; }

        IDictionary<DeploymentIdNamePair, VariableReader> VariableReadersNonCP { get; }

        VariableStateNonConstHandler OptionalStateHandler { get; }

        IDictionary<string, VariableDeployment> DeploymentsWithVariables { get; }

        /// <summary>
        ///     Sets the variable version that subsequent reads consider.
        /// </summary>
        void SetLocalVersion();

        void AddVariable(
            string deploymentId,
            VariableMetaData metaData,
            string optionalDeploymentIdContext,
            DataInputOutputSerde optionalSerde);

        /// <summary>
        ///     Returns a reader that provides access to variable values. The reader considers the
        ///     version currently set via setLocalVersion.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">the variable that the reader should read</param>
        /// <param name="agentInstanceIdAccessor">agent instance id of accessor</param>
        /// <returns>reader</returns>
        VariableReader GetReader(
            string deploymentId,
            string variableName,
            int agentInstanceIdAccessor);

        /// <summary>
        ///     Registers a callback invoked when the variable is written with a new value.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">variable name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="variableChangeCallback">a callback</param>
        void RegisterCallback(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            VariableChangeCallback variableChangeCallback);

        /// <summary>
        ///     Removes a callback.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">variable name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="variableChangeCallback">a callback</param>
        void UnregisterCallback(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            VariableChangeCallback variableChangeCallback);

        /// <summary>
        ///     Writes a new variable value.
        ///     <para />
        ///     Must be followed by either a commit or rollback.
        /// </summary>
        /// <param name="variableNumber">the index number of the variable to write (from VariableReader)</param>
        /// <param name="newValue">the new value</param>
        /// <param name="agentInstanceId">agent instance id</param>
        void Write(
            int variableNumber,
            int agentInstanceId,
            object newValue);

        /// <summary>
        ///     Check type of the value supplied and writes the new variable value.
        ///     <para />
        ///     Must be followed by either a commit or rollback.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">variable name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="newValue">the new value</param>
        void CheckAndWrite(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            object newValue);

        /// <summary>
        ///     Commits the variable outstanding changes.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Rolls back the variable outstanding changes.
        /// </summary>
        void Rollback();

        Variable GetVariableMetaData(
            string deploymentId,
            string variableName);

        /// <summary>
        ///     Removes a variable.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">to remove</param>
        void RemoveVariableIfFound(
            string deploymentId,
            string variableName);

        void Destroy();

        void AllocateVariableState(
            string deploymentId,
            string variableName,
            int agentInstanceId,
            bool recovery,
            NullableObject<object> initialValue,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        void DeallocateVariableState(
            string deploymentId,
            string variableName,
            int agentInstanceId);

        IDictionary<int, VariableReader> GetReadersPerCP(
            string deploymentId,
            string variableName);
    }
} // end of namespace