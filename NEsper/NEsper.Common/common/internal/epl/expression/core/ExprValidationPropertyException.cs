///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Thrown to indicate a validation error in an expression originating from a property resolution error.
    /// </summary>
    public class ExprValidationPropertyException : ExprValidationException
    {
        public ExprValidationPropertyException(string message)
            : base(message)
        {
        }

        public ExprValidationPropertyException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }
    }
}