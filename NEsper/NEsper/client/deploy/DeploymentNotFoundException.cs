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
    /// Inner exception to <seealso cref="com.espertech.esper.client.deploy.DeploymentActionException" /> available on statement level.
    /// </summary>
    [Serializable]
    public class DeploymentNotFoundException : DeploymentException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        public DeploymentNotFoundException(String message)
            : base(message)
        {
        }
    }
}
