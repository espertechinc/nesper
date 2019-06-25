///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using com.espertech.esper.common.client;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     Exception information.
    /// </summary>
    public class EPCompileExceptionItem : EPException
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="expression">expression text</param>
        /// <param name="lineNumber">line number</param>
        public EPCompileExceptionItem(
            string message,
            string expression,
            int lineNumber)
            : base(message)
        {
            Expression = expression;
            LineNumber = lineNumber;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="cause">inner exception</param>
        /// <param name="expression">expression text</param>
        /// <param name="lineNumber">line number</param>
        public EPCompileExceptionItem(
            string message,
            Exception cause,
            string expression,
            int lineNumber)
            : base(message, cause)
        {
            Expression = expression;
            LineNumber = lineNumber;
        }

        /// <summary>
        ///     Returns expression text for statement.
        /// </summary>
        /// <returns>expression text</returns>
        public string Expression { get; }

        /// <summary>
        ///     Returns the line number.
        /// </summary>
        /// <returns>line number</returns>
        public int LineNumber { get; }

        public string GetMessage()
        {
            StringBuilder msg;
            if (!string.IsNullOrWhiteSpace(base.Message)) {
                msg = new StringBuilder(base.Message);
            }
            else {
                msg = new StringBuilder("Unexpected exception");
            }

            if (Expression != null) {
                msg.Append(" [");
                msg.Append(Expression);
                msg.Append(']');
            }

            return msg.ToString();
        }
    }
} // end of namespace