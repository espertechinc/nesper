///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Thrown to indicate a precondition violation for undeploy.
    /// </summary>
    [Serializable]
    public class UndeployPreconditionException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">validation error message</param>
        public UndeployPreconditionException(string message)
            : base(message)
        {
        }

        protected UndeployPreconditionException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace