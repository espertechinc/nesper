///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// This exception is thrown to indicate a problem in statement creation.
    /// </summary>
    [Serializable]
    public class EPStatementSyntaxException : EPStatementException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        /// <param name="expression">expression text</param>
        public EPStatementSyntaxException(String message, String expression)
            : base(message, expression)
        {
        }
    }
}
