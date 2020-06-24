///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     Interface for a plug-in to <seealso cref="VariableManagementService" /> to handle variable persistent state.
    /// </summary>
    public interface VariableStateNonConstHandler
    {
        void AddVariable(
            string deploymentId,
            string variableName,
            Variable variable,
            DataInputOutputSerdeWCollation<object> serde);

        /// <summary>
        ///     Returns the current variable state plus Boolean.TRUE if there is a current state since the variable
        ///     may have the value of null; returns Boolean.FALSE and null if there is no current state
        /// </summary>
        /// <param name="variable">variable</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <returns>indicator whether the variable is known and it's state, or whether it doesn't have state (false)</returns>
        NullableObject<object> GetHasState(
            Variable variable,
            int agentInstanceId);

        /// <summary>
        ///     Sets the new variable value
        /// </summary>
        /// <param name="variable">variable</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="newValue">new variable value, null values allowed</param>
        void SetState(
            Variable variable,
            int agentInstanceId,
            object newValue);

        void RemoveState(
            Variable variable,
            int agentInstanceId);

        void RemoveVariable(
            Variable variable,
            string deploymentId,
            ICollection<int> cps);
    }
} // end of namespace