///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>Exception to represent a circular dependency. </summary>
    [Serializable]
    public class GraphCircularDependencyException : Exception
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">supplies the detailed description</param>
        public GraphCircularDependencyException(string message)
            : base(message)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="message">supplies the detailed description</param>
        /// <param name="innerException">the exception cause</param>
        public GraphCircularDependencyException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        protected GraphCircularDependencyException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}