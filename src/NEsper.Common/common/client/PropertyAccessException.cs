///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;
using System.Text;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// This exception is thrown to indicate a problem with a accessing a property of an EventBean />.
    /// </summary>
    [Serializable]
    public class PropertyAccessException : Exception
    {
        private readonly string _expression;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">is the error message</param>
        /// <param name="propertyExpression">property expression</param>
        public PropertyAccessException(
            string message,
            string propertyExpression)
            : base(message)
        {
            _expression = propertyExpression;
        }

        /// <summary>
        /// Constructor for an inner exception and message.
        /// </summary>
        /// <param name="message">is the error message</param>
        /// <param name="cause">is the inner exception</param>
        public PropertyAccessException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary>
        /// Constructor for an inner exception and message.
        /// </summary>
        /// <param name="message">is the error message</param>
        public PropertyAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cause">is the inner exception</param>
        public PropertyAccessException(Exception cause)
            : base(string.Empty, cause)
        {
        }

        protected PropertyAccessException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Generates the Not-A-Valid-Property exception
        /// </summary>
        /// <param name="propertyExpression">property expression</param>
        /// <returns>exception exception</returns>
        public static PropertyAccessException NotAValidProperty(string propertyExpression)
        {
            return new PropertyAccessException(
                $"Property named '{propertyExpression}' is not a valid property name for this type");
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message {
            get {
                StringBuilder msg;

                if (!string.IsNullOrEmpty(base.Message)) {
                    msg = new StringBuilder(base.Message);
                }
                else {
                    msg = new StringBuilder("Unexpected exception");
                    if (InnerException != null) {
                        msg.Append(" : ");
                        msg.Append(InnerException.Message);
                    }
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
} // end of namespace