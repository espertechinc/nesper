///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client.util
{
    public class ExceptionLineItem : Exception
    {
        /// <summary>
        /// Returns the expression.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Returns the line number (if available).
        /// </summary>
        public int LineNumber { get; set; }

        public ExceptionLineItem()
        {
        }

        public ExceptionLineItem(string message) : base(message)
        {
        }

        public ExceptionLineItem(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }

        public ExceptionLineItem(
            string message,
            string expression,
            int lineNumber) : base(message)
        {
            Expression = expression;
            LineNumber = lineNumber;
        }

        public ExceptionLineItem(
            string message,
            Exception innerException,
            string expression,
            int lineNumber) : base(message, innerException)
        {
            Expression = expression;
            LineNumber = lineNumber;
        }

        protected ExceptionLineItem(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}