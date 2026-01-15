///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>Thrown to indicate a validation error in guard parameterization.</summary>
    public class ObserverParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObserverParameterException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ObserverParameterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObserverParameterException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ObserverParameterException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
} // End of namespace