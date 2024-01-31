///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// This exception is thrown to indicate a problem in administration and runtime.
    /// </summary>
    public class EPException : Exception
    {
        private static readonly Type MyType =
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

        /// <summary> Ctor.</summary>
        /// <param name="message">error message
        /// </param>
        public EPException(string message)
            : base(message)
        {
        }

        /// <summary> Ctor for an inner exception and message.</summary>
        /// <param name="message">error message
        /// </param>
        /// <param name="cause">inner exception
        /// </param>
        public EPException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary> Ctor - just an inner exception.</summary>
        /// <param name="cause">inner exception
        /// </param>
        public EPException(Exception cause)
            : base(MyType.FullName + ": " + cause.Message, cause)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected EPException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}