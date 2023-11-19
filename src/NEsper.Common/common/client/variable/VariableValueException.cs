///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client.variable
{
    /// <summary>
    /// Indicates that a variable value could not be assigned.
    /// </summary>
    public class VariableValueException : EPException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">supplies exception details</param>
        public VariableValueException(string message)
            : base(message)
        {
        }

        protected VariableValueException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}