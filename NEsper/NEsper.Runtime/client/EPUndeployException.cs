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
    ///     Exception during an undeploy operation by <seealso cref="EPDeploymentService.Undeploy(string)" />
    /// </summary>
    public class EPUndeployException : Exception
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="message">message</param>
        public EPUndeployException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        public EPUndeployException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }
    }
} // end of namespace