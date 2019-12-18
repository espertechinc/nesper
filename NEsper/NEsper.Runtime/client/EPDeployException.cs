///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Exception during a deploy operation by <seealso cref="EPDeploymentService.Deploy(EPCompiled)" />
    /// </summary>
    public class EPDeployException : Exception
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        public EPDeployException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="cause">cause</param>
        public EPDeployException(Exception cause)
            : base("Deployment Exception", cause)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        public EPDeployException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }
    }
} // end of namespace