///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    /// Thrown to indicate a validation error in view parameterization.
    /// </summary>
    public class ViewParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewParameterException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ViewParameterException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Ctor.</summary>
        /// <param name="message">validation error message</param>
        public ViewParameterException(string message)
            : base(message)
        {
        }
    }
} // End of namespace