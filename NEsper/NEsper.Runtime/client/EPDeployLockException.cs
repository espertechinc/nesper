///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Deploy exception to indicate a problem taking the necessary lock
    /// </summary>
    public class EPDeployLockException : EPDeployException
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        public EPDeployLockException(string message, Exception cause)
            : base(message, cause)
        {
        }
    }
} // end of namespace