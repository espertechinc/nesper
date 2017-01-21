///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.script
{
    public class ScriptingEngineException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineException"/> class.
        /// </summary>
        public ScriptingEngineException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ScriptingEngineException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ScriptingEngineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected ScriptingEngineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
