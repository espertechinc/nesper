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
using com.espertech.esper.collection;
using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// Interface for a plug-in to <seealso cref="VariableService" /> to handle variable persistent state.
    /// </summary>
    public interface VariableStateHandler
    {
        /// <summary>
        /// Returns the current variable state plus true if there is a current state since the variable may have 
        /// the value of null; returns false and null if there is no current state
        /// </summary>
        /// <param name="variableName">variable name</param>
        /// <param name="variableNumber">number of the variable</param>
        /// <param name="agentInstanceId">The agent instance identifier.</param>
        /// <param name="type">type of the variable</param>
        /// <param name="eventType">event type or null if not a variable that represents an event</param>
        /// <param name="statementExtContext">for caches etc.</param>
        /// <param name="isConstant">if set to <c>true</c> [is constant].</param>
        /// <returns>
        /// indicator whether the variable is known and it's state, or whether it doesn't have state (false)
        /// </returns>
        Pair<bool, object> GetHasState(string variableName, int variableNumber, int agentInstanceId, Type type, EventType eventType, StatementExtensionSvcContext statementExtContext, bool isConstant);

        /// <summary>
        /// Sets the new variable value
        /// </summary>
        /// <param name="variableName">name of the variable</param>
        /// <param name="variableNumber">number of the variable</param>
        /// <param name="agentInstanceId">The agent instance identifier.</param>
        /// <param name="newValue">new variable value, null values allowed</param>
        void SetState(String variableName, int variableNumber, int agentInstanceId, Object newValue);

        void RemoveState(String variableName, int variableNumber, int agentInstanceId);

        void RemoveVariable(String name, IEnumerable<int> cps);

    }
}
