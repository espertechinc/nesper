///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary> Thrown to indicate a validation error in a filter expression.</summary>
    [Serializable]
    public class ExprValidationException : Exception
    {
        /// <summary> Ctor.</summary>
        /// <param name="message">validation error message
        /// </param>
        public ExprValidationException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExprValidationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cause">The cause.</param>
        public ExprValidationException(String message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
