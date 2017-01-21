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
    /// Implement this interface to provide a custom statement name for the statements deployed 
    /// via the deployment API. 
    /// <para />
    /// Statement names provided by the resolver override the statement name provided via the @Name annotation. 
    /// </summary>
    public interface StatementNameResolver
    {
        /// <summary>
        /// Returns the statement name to assign to a newly-deployed statement. 
        /// <para />
        /// Implementations would typically interrogate the context object EPL expression or module and module 
        /// item information and determine the right statement name to assign.
        /// </summary>
        /// <param name="context">the statement's deployment context</param>
        /// <returns>
        /// statement name or null if none needs to be assigned and the default or @Name annotated name should be used
        /// </returns>
        String GetStatementName(StatementDeploymentContext context);
    }

    public class ProxyStatementNameResolver : StatementNameResolver
    {
        public Func<StatementDeploymentContext, string> ProcGetStatementName { get; set; }

        public string GetStatementName(StatementDeploymentContext context)
        {
            return ProcGetStatementName(context);
        }
    }
}
