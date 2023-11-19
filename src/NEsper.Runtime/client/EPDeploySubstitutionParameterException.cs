///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Deploy exception to indicate that substitution parameter values have not been provided
    /// </summary>
    public class EPDeploySubstitutionParameterException : EPDeployException
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeploySubstitutionParameterException(
            string message,
            int rolloutItemNumber)
            : this(message, null, rolloutItemNumber)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public EPDeploySubstitutionParameterException(
            string message,
            Exception cause,
            int rolloutItemNumber)
            : base("Substitution parameters have not been provided: " + message, cause, rolloutItemNumber)
        {
        }

        protected EPDeploySubstitutionParameterException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace