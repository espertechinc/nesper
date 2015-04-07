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
    /// Base deployment exception.
    /// </summary>
    [Serializable]
    public class DeploymentException : Exception 
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        public DeploymentException(String message)
            : base(message)
        {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        /// <param name="cause">cause</param>
        public DeploymentException(String message, Exception cause)
            : base(message, cause)
        {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="cause">cause</param>
        public DeploymentException(Exception cause)
            : base(string.Empty, cause)
        {
        }
    }
}
