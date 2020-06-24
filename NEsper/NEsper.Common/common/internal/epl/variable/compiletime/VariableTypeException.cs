///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    /// <summary>
    ///     Exception indicating a variable type error.
    /// </summary>
    [Serializable]
    public class VariableTypeException : VariableDeclarationException
    {
        /// <summary>Ctor.</summary>
        /// <param name="msg">the exception message.</param>
        public VariableTypeException(string msg)
            : base(msg)
        {
        }

        public VariableTypeException(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }

        protected VariableTypeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // End of namespace