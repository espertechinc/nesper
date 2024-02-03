///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client.option
{
    /// <summary>
    /// Implement this interface to provide a custom user object at runtime for statements when they are deployed.
    /// </summary>
    public interface StatementUserObjectRuntimeOption
    {
        /// <summary>
        /// Returns the user object to assign to a newly-deployed statement.
        /// <para />Implementations would typically interrogate the context object EPL expression
        /// or module and module item information and determine the right user object to assign.
        /// <para />When using HA the returned object must implement the Serializable interface.
        /// </summary>
        /// <param name="env">the statement's deployment context</param>
        /// <returns>user object or null if none needs to be assigned</returns>
        object GetUserObject(StatementUserObjectRuntimeContext env);
    }

    public class ProxyStatementUserObjectRuntimeOption : StatementUserObjectRuntimeOption
    {
        public Func<StatementUserObjectRuntimeContext, object> ProcGetUserObject { get; set; }
        public object GetUserObject(StatementUserObjectRuntimeContext env)
        {
            return ProcGetUserObject?.Invoke(env);
        }
    }
} // end of namespace