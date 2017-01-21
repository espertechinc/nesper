///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// Variables service for reading and writing variables, and for setting a version number for the current thread to
    /// consider variables for.
    /// <para>
    /// See implementation class for further details.
    /// </para>
    /// </summary>
    public interface VariableService : IDisposable
    {
        /// <summary>Sets the variable version that subsequent reads consider.</summary>
        void SetLocalVersion();

        /// <summary>Lock for use in atomic writes to the variable space.</summary>
        /// <returns>read write lock for external coordinated write</returns>
        IReaderWriterLock ReadWriteLock { get; }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <param name="optionalContextName">Name of the optional context.</param>
        /// <param name="variableName">name of the variable</param>
        /// <param name="type">variable type</param>
        /// <param name="constant">if set to <c>true</c> [constant].</param>
        /// <param name="array">if set to <c>true</c> [array].</param>
        /// <param name="arrayOfPrimitive">if set to <c>true</c> [array of primitive].</param>
        /// <param name="value">initialization value; String values are allowed and parsed according to type</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <throws>VariableExistsException if the variable name is already in use</throws>
        /// <throws>VariableTypeException if the variable type cannot be recognized</throws>
        void CreateNewVariable(
            string optionalContextName,
            string variableName,
            string type,
            bool constant,
            bool array,
            bool arrayOfPrimitive,
            object value,
            EngineImportService engineImportService);

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionalContextName">Name of the optional context.</param>
        /// <param name="variableName">name of the variable</param>
        /// <param name="constant">if set to <c>true</c> [constant].</param>
        /// <param name="value">initialization value; String values are allowed and parsed according to type</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <throws>VariableExistsException if the variable name is already in use</throws>
        /// <throws>VariableTypeException if the variable type cannot be recognized</throws>
        void CreateNewVariable<T>(
            string optionalContextName,
            string variableName,
            bool constant,
            T value,
            EngineImportService engineImportService);

        /// <summary>
        /// Returns a reader that provides access to variable values. The reader considers the
        /// version currently set via setLocalVersion.
        /// </summary>
        /// <param name="variableName">the variable that the reader should read</param>
        /// <param name="agentInstanceIdAccessor">The agent instance id accessor.</param>
        /// <returns>reader</returns>
        VariableReader GetReader(String variableName, int agentInstanceIdAccessor);

        /// <summary>
        /// Registers a callback invoked when the variable is written with a new value.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="agentInstanceId">The agent instance id.</param>
        /// <param name="variableChangeCallback">a callback</param>
        void RegisterCallback(string variableName, int agentInstanceId, VariableChangeCallback variableChangeCallback);

        /// <summary>
        /// Removes a callback.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="agentInstanceId">The agent instance id.</param>
        /// <param name="variableChangeCallback">a callback</param>
        void UnregisterCallback(string variableName, int agentInstanceId, VariableChangeCallback variableChangeCallback);

        /// <summary>
        /// Check type of the value supplied and writes the new variable value.
        /// <para/>
        /// Must be followed by either a commit or rollback.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="agentInstanceId">The agent instance id.</param>
        /// <param name="newValue">the new value</param>
        void CheckAndWrite(string variableName, int agentInstanceId, Object newValue);

        /// <summary>
        /// Writes a new variable value.
        /// <para>
        /// Must be followed by either a commit or rollback.
        /// </para>
        /// </summary>
        /// <param name="variableNumber">the index number of the variable to write (from VariableMetaData)</param>
        /// <param name="agentInstanceId">The agent instance id.</param>
        /// <param name="newValue">the new value</param>
        void Write(int variableNumber, int agentInstanceId, Object newValue);

        /// <summary>Commits the variable outstanding changes.</summary>
        void Commit();

        /// <summary>Rolls back the variable outstanding changes.</summary>
        void Rollback();

        /// <summary>Returns a map of variable name and reader, for thread-safe iteration.</summary>
        /// <returns>variable names and readers</returns>
        IDictionary<String, VariableReader> VariableReadersNonCP { get; }

        VariableMetaData GetVariableMetaData(String variableName);

        /// <summary>Removes a variable. </summary>
        /// <param name="name">to remove</param>
        void RemoveVariableIfFound(String name);

        String IsContextVariable(String propertyName);

        void AllocateVariableState(String variableName, int agentInstanceId, StatementExtensionSvcContext extensionServicesContext, bool isRecoveringResilient);
        void DeallocateVariableState(String variableName, int agentInstanceId);

        ConcurrentDictionary<int, VariableReader> GetReadersPerCP(String variableName);
    }
} // End of namespace
