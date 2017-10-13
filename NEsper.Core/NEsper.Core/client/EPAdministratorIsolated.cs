///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Administrative interfae
    /// </summary>
    public interface EPAdministratorIsolated
    {
        /// <summary>
        /// Create and starts an EPL statement.
        /// <para/>
        /// The statement name is optimally a unique name. If a statement of the same name
        /// has already been created, the engine assigns a postfix to create a unique
        /// statement name.
        /// <para/>
        /// Accepts an application defined user data object associated with the statement.
        /// The <em>user object</em> is a single, unnamed field that is stored with every
        /// statement. Applications may put arbitrary objects in this field or a null value.
        /// </summary>
        /// <param name="eplStatement">is the query language statement</param>
        /// <param name="statementName">is the statement name or null if not provided or provided via annotation instead</param>
        /// <param name="userObject">is the application-defined user object, or null if none provided</param>
        /// <returns>
        /// EPStatement to poll data from or to add listeners to, or null if provided via
        /// annotation
        /// </returns>
        /// <throws>com.espertech.esper.client.EPException when the expression was not valid</throws>
        EPStatement CreateEPL(String eplStatement, String statementName, Object userObject);
    
        /// <summary>
        /// Returns the statement names of all started and stopped statements.
        /// <para/>
        /// This excludes the name of destroyed statements.
        /// </summary>
        /// <returns>
        /// statement names
        /// </returns>
        IList<string> StatementNames { get; }

        /// <summary>
        /// Add a statement to the isolated service.
        /// </summary>
        /// <param name="statement">to add</param>
        /// <throws>EPServiceIsolationException if the statement cannot be isolated, typically because it already is isolated</throws>
        void AddStatement(EPStatement statement);

        /// <summary>
        /// Remove a statement from the isolated service. This does not change engine state.
        /// </summary>
        /// <param name="statement">to remove</param>
        /// <throws>EPServiceIsolationException if the statement was not isolated herein</throws>
        void RemoveStatement(EPStatement statement);

        /// <summary>
        /// Add statements to the isolated service.
        /// </summary>
        /// <param name="statements">to add</param>
        /// <throws>EPServiceIsolationException if the statement cannot be isolated, typically because it already is isolated</throws>
        void AddStatement(EPStatement[] statements);

        /// <summary>
        /// Remove statements from the isolated service. This does not change engine state.
        /// </summary>
        /// <param name="statements">to remove</param>
        /// <throws>EPServiceIsolationException if the statement was not isolated herein</throws>
        void RemoveStatement(IList<EPStatement> statements);
    }
}
