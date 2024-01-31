///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    /// This exception is thrown to indicate a problem with a view expression.
    /// </summary>
    public sealed class ViewProcessingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewProcessingException"/> class.
        /// </summary>
        public ViewProcessingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ViewProcessingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ViewProcessingException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        public ViewProcessingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}