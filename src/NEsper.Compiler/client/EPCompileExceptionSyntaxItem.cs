///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     Indicates a syntax exception
    /// </summary>
    [Serializable]
    public class EPCompileExceptionSyntaxItem : EPCompileExceptionItem
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="expression">expression text</param>
        /// <param name="lineNumber">line number</param>
        public EPCompileExceptionSyntaxItem(
            string message,
            string expression,
            int lineNumber)
            : base(message, expression, lineNumber)
        {
        }

        protected EPCompileExceptionSyntaxItem(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace