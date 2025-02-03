///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Exception during a deploy operation by <seealso cref="EPDeploymentService.Deploy(EPCompiled)" />
    /// </summary>
    public class EPDeployException : Exception
    {
        private readonly int _rolloutItemNumber;

        public int RolloutItemNumber => _rolloutItemNumber;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeployException(int rolloutItemNumber)
        {
            _rolloutItemNumber = rolloutItemNumber;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeployException(
            string message,
            int rolloutItemNumber) : base(message)
        {
            _rolloutItemNumber = rolloutItemNumber;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner / cause exception</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeployException(
            string message,
            Exception innerException,
            int rolloutItemNumber) : base(message, innerException)
        {
            _rolloutItemNumber = rolloutItemNumber;
        }
    }
} // end of namespace