///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Provides well-known statement properties.
    /// </summary>
    public enum StatementProperty
    {
        /// <summary>
        ///     The statement EPL text.
        /// </summary>
        EPL,

        /// <summary>
        ///     The statement type
        /// </summary>
        STATEMENTTYPE,
        
        /// <summary>
        /// The name of the EPL-object created by the statement, of type, or null if not applicable,
        /// i.e. the name of the name window, table, variable, expression, index, schema or expression created by
        /// the statement.
        /// <para>
        ///     Use together with the statement type to determine the type of object.
        /// </para>
        /// </summary>
        CREATEOBJECTNAME,
        
        /// <summary>
        /// The context name, of type or null if the statement is not associated to a context.
        /// </summary>
        
        CONTEXTNAME,
        
        /// <summary>
        /// The context deployment id, of type or null if the statement is not associated to a context.
        /// </summary>
        CONTEXTDEPLOYMENTID
    }
} // end of namespace