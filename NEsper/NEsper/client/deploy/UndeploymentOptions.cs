///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Options for use in undeployment of a module to control the behavior of the undeploy operation.
    /// </summary>
    [Serializable]
    public class UndeploymentOptions
    {
        /// <summary>
        /// Returns indicator whether undeploy will destroy any associated statements (true by default).
        /// </summary>
        /// <value>
        /// flag indicating whether undeploy also destroys associated statements
        /// </value>
        public bool IsDestroyStatements { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UndeploymentOptions"/> class.
        /// </summary>
        public UndeploymentOptions()
        {
            IsDestroyStatements = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UndeploymentOptions"/> class.
        /// </summary>
        /// <param name="isDestroyStatements">if set to <c>true</c> [is destroy statements].</param>
        public UndeploymentOptions(bool isDestroyStatements)
        {
            IsDestroyStatements = isDestroyStatements;
        }
    }
}
