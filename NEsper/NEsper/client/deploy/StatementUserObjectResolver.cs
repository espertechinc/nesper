///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Implement this interface to provide a custom user object for the statements deployed via the deployment API.
    /// </summary>
    public interface StatementUserObjectResolver 
    {
        /// <summary>
        /// Returns the user object to assign to a newly-deployed statement.
        /// <para />
        /// Implementations would typically interrogate the context object EPL expression or module and module 
        /// item information and determine the right user object to assign.
        /// </summary>
        /// <param name="context">the statement's deployment context</param>
        /// <returns>
        /// user object or null if none needs to be assigned
        /// </returns>
        Object GetUserObject(StatementDeploymentContext context);
    }

    public class ProxyStatementUserObjectResolver : StatementUserObjectResolver
    {
        public Func<StatementDeploymentContext, Object> ProcGetUserObject { get; set; }

        public Object GetUserObject(StatementDeploymentContext context)
        {
            return ProcGetUserObject(context);
        }
    }
}
