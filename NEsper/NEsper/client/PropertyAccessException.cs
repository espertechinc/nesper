///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.client
{
    /// <summary>
    /// This exception is thrown to indicate a problem with a accessing a property of an
    /// <seealso cref="com.espertech.esper.client.EventBean"/>.
    /// </summary>
    [Serializable]
    public sealed class PropertyAccessException : Exception
    {
        private readonly String _expression;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">is the error message</param>
        /// <param name="propertyExpression">property expression</param>
        public PropertyAccessException(String message, String propertyExpression)
            : base(message)
        {
            _expression = propertyExpression;
        }

        /// <summary>
        /// Constructor for an inner exception and message.
        /// </summary>
        /// <param name="message">is the error message</param>
        /// <param name="cause">is the inner exception</param>
        public PropertyAccessException(String message, Exception cause)
            : base(message, cause)
        {
        }

        /// <summary>
        /// Constructor for an inner exception and message.
        /// </summary>
        /// <param name="message">is the error message</param>
        public PropertyAccessException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cause">is the inner exception</param>
        public PropertyAccessException(Exception cause)
            : base(String.Empty, cause)
        {
        }

        public override string Message
        {
            get
            {
                StringBuilder msg;
                if (base.Message != null) {
                    msg = new StringBuilder(base.Message);
                }
                else {
                    msg = new StringBuilder("Unexpected exception");
                }
                if (_expression != null) {
                    msg.Append(" [");
                    msg.Append(_expression);
                    msg.Append(']');
                }
                return msg.ToString();
            }
        }
    }
}
