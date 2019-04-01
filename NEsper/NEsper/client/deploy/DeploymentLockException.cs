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
    /// <summary>Exception to indicate a problem taking a lock</summary>
    public class DeploymentLockException : DeploymentException
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">message</param>
        public DeploymentLockException(string message)
            : base(message)
        {
        }
    
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        public DeploymentLockException(string message, Exception cause)
            : base(message, cause)
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="cause">cause</param>
        public DeploymentLockException(Exception cause)
            : base(cause)
        {
        }
    }
} // end of namespace
