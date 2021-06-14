///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>Thrown to indicate a validation error in guard parameterization.</summary>
    [Serializable]
    public class GuardParameterException : Exception
    {
        /// <summary>Ctor.</summary>
        /// <param name="message">validation error message</param>
        public GuardParameterException(String message)
            : base(message)
        {
        }

        protected GuardParameterException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // End of namespace