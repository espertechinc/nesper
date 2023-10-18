///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.client.variable
{
    /// <summary>
    ///     Service for variable management.
    /// </summary>
    public interface EPVariableService
    {
        /// <summary>
        ///     Returns the current variable value for a global variable. A null value is a valid value for a variable.
        ///     Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">is the name of the variable to return the value for</param>
        /// <returns>current variable value</returns>
        /// <throws>VariableNotFoundException if a variable by that name has not been declared</throws>
        object GetVariableValue(
            string deploymentId,
            string variableName);

        /// <summary>
        ///     Returns the current variable values for a context-partitioned variable, per context partition.
        ///     A null value is a valid value for a variable.
        ///     Only for use with context-partitioned variables.
        ///     Variable names provided must all be associated to the same context partition.
        /// </summary>
        /// <param name="variableNames">are the names of the variables to return the value for</param>
        /// <param name="contextPartitionSelector">selector for the context partition to return the value for</param>
        /// <returns>current variable value</returns>
        /// <throws>VariableNotFoundException if a variable by that name has not been declared</throws>
        IDictionary<DeploymentIdNamePair, IList<ContextPartitionVariableState>> GetVariableValue(
            ISet<DeploymentIdNamePair> variableNames,
            ContextPartitionSelector contextPartitionSelector);

        /// <summary>
        ///     Returns current variable values for each of the global variable names passed in,
        ///     guaranteeing consistency in the face of concurrent updates to the variables.
        ///     Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="variableNames">is a set of variable names for which to return values</param>
        /// <returns>map of variable name and variable value</returns>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        IDictionary<DeploymentIdNamePair, object> GetVariableValue(ISet<DeploymentIdNamePair> variableNames);

        /// <summary>
        ///     Returns current variable values for all global variables,
        ///     guaranteeing consistency in the face of concurrent updates to the variables.
        ///     Not for use with context-partitioned variables.
        /// </summary>
        /// <returns>map of variable name and variable value</returns>
        IDictionary<DeploymentIdNamePair, object> GetVariableValueAll();

        /// <summary>
        ///     Sets the value of a single global variable.
        ///     <para />
        ///     Note that the thread setting the variable value queues the changes, i.e. it does not itself
        ///     re-evaluate such new variable value for any given statement. The timer thread performs this work.
        ///     Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="variableName">is the name of the variable to change the value of</param>
        /// <param name="variableValue">is the new value of the variable, with null an allowed value</param>
        /// <throws>
        ///     VariableValueException    if the value does not match variable type or cannot be safely coerced to the variable type
        /// </throws>
        /// <throws>VariableNotFoundException if the variable name has not been declared</throws>
        void SetVariableValue(
            string deploymentId,
            string variableName,
            object variableValue);

        /// <summary>
        ///     Sets the value of multiple global variables in one update, applying all or none of the changes
        ///     to variable values in one atomic transaction.
        ///     <para />
        ///     Note that the thread setting the variable value queues the changes, i.e. it does not itself
        ///     re-evaluate such new variable value for any given statement. The timer thread performs this work.
        ///     Not for use with context-partitioned variables.
        /// </summary>
        /// <param name="value">is the map of variable name and variable value, with null an allowed value</param>
        /// <throws>
        ///     VariableValueException    if any value does not match variable type or cannot be safely coerced to the variable type
        /// </throws>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        void SetVariableValue(IDictionary<DeploymentIdNamePair, object> value);

        /// <summary>
        ///     Sets the value of multiple context-partitioned variables in one update, applying all or none of the changes
        ///     to variable values in one atomic transaction.
        ///     <para />
        ///     Note that the thread setting the variable value queues the changes, i.e. it does not itself
        ///     re-evaluate such new variable value for any given statement. The timer thread performs this work.
        ///     Only for use with context-partitioned variables.
        /// </summary>
        /// <param name="value">is the map of variable name and variable value, with null an allowed value</param>
        /// <param name="agentInstanceId">the id of the context partition</param>
        /// <throws>
        ///     VariableValueException    if any value does not match variable type or cannot be safely coerced to the variable type
        /// </throws>
        /// <throws>VariableNotFoundException if any of the variable names has not been declared</throws>
        void SetVariableValue(
            IDictionary<DeploymentIdNamePair, object> value,
            int agentInstanceId);
    }
} // end of namespace