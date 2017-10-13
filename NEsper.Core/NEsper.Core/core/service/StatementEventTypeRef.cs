///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.core.service
{
    /// <summary>Service for maintaining references between statement name and event type. </summary>
    public interface StatementEventTypeRef
    {
        /// <summary>Returns true if the event type is listed as in-use by any statement, or false if not </summary>
        /// <param name="eventTypeName">name</param>
        /// <returns>indicator whether type is in use</returns>
        bool IsInUse(String eventTypeName);
    
        /// <summary>Returns the set of event types that are use by a given statement name. </summary>
        /// <param name="statementName">name</param>
        /// <returns>set of event types or empty set if none found</returns>
        String[] GetTypesForStatementName(String statementName);
    
        /// <summary>Returns the set of statement names that use a given event type name. </summary>
        /// <param name="eventTypeName">name</param>
        /// <returns>set of statements or null if none found</returns>
        ICollection<String> GetStatementNamesForType(String eventTypeName);
    
        /// <summary>Add a reference from a statement name to a set of event types. </summary>
        /// <param name="statementName">name of statement</param>
        /// <param name="eventTypesReferenced">types</param>
        void AddReferences(String statementName, String[] eventTypesReferenced);
    
        /// <summary>Remove all references for a given statement. </summary>
        /// <param name="statementName">statement name</param>
        void RemoveReferencesStatement(String statementName);
    
        /// <summary>Remove all references for a given event type. </summary>
        /// <param name="eventTypeName">event type name</param>
        void RemoveReferencesType(String eventTypeName);
    }
}
