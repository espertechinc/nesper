///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Deploy exception to indicate that a precondition is not satisfied
    /// </summary>
    public class EPDeployPreconditionException : EPDeployException
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeployPreconditionException(
            string message,
            int rolloutItemNumber)
            : base("A precondition is not satisfied: " + message, rolloutItemNumber)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeployPreconditionException(
            string message,
            Exception cause,
            int rolloutItemNumber)
            : base("A precondition is not satisfied: " + message, cause, rolloutItemNumber)
        {
        }
    }
} // end of namespace