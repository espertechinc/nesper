///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client.option
{
    /// <summary>
    /// Implement this interface to provide a statement name at runtime for statements when they are deployed.
    /// </summary>
    public delegate string StatementNameRuntimeOption(StatementNameRuntimeContext env);

#if DEPRECATED_INTERFACE
    public interface StatementNameRuntimeOption
    {
        /// <summary>
        /// Returns the statement name to assign to a newly-deployed statement.
        /// <para />Implementations would typically interrogate the context object EPL expression
        /// or module and module item information and determine the right statement name to assign.
        /// <para />When using HA the returned object must implement the Serializable interface.
        /// </summary>
        /// <param name="env">the statement's deployment context</param>
        /// <returns>statement name or null if none needs to be assigned</returns>
        string GetStatementName(StatementNameRuntimeContext env);
    }
#endif

} // end of namespace