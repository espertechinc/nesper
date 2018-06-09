///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.table;


namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Service for maintaining references between statement name and variables.
    /// </summary>
    public interface StatementVariableRef
    {
        /// <summary>Returns true if the variable is listed as in-use by any statement, or false if not </summary>
        /// <param name="variableName">name</param>
        /// <returns>indicator whether variable is in use</returns>
        bool IsInUse(String variableName);
    
        /// <summary>Returns the set of statement names that use a given variable. </summary>
        /// <param name="variableName">name</param>
        /// <returns>set of statements or null if none found</returns>
        ICollection<String> GetStatementNamesForVar(String variableName);

        /// <summary>
        /// Add a reference from a statement name to a set of variables.
        /// </summary>
        /// <param name="statementName">name of statement</param>
        /// <param name="variablesReferenced">types</param>
        /// <param name="tableNodes">The table nodes.</param>
        void AddReferences(String statementName, ICollection<String> variablesReferenced, ExprTableAccessNode[] tableNodes);

        /// <summary>Add a reference from a statement name to a single variable.</summary>
        /// <param name="statementName">name of statement</param>
        /// <param name="variableReferenced">variable</param>
        void AddReferences(String statementName, String variableReferenced);
    
        /// <summary>Remove all references for a given statement. </summary>
        /// <param name="statementName">statement name</param>
        void RemoveReferencesStatement(String statementName);
    
        /// <summary>Remove all references for a given event type. </summary>
        /// <param name="variableName">variable name</param>
        void RemoveReferencesVariable(String variableName);
    
        /// <summary>Add a preconfigured variable. </summary>
        /// <param name="variableName">name</param>
        void AddConfiguredVariable(String variableName);
    
        /// <summary>Remove a preconfigured variable. </summary>
        /// <param name="variableName">var</param>
        void RemoveConfiguredVariable(String variableName);
    }
}
