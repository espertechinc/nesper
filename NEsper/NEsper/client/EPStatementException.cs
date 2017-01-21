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
    /// This exception is thrown to indicate a problem in statement creation, such as syntax error or type
    /// checking problem etc.
    /// </summary>

    [Serializable]
    public class EPStatementException : EPException
    {
        /// <summary>
        /// Gets or sets the expression text for statement.
        /// </summary>
        /// <value>The expression.</value>
        public string Expression { get; set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        public override String Message
        {
            get
            {
                var msg = new StringBuilder();
                var baseMessage = base.Message;

                if (String.IsNullOrEmpty(baseMessage)) {
                    msg.Append("Unexpected exception");
                }
                else {
                    msg.Append(baseMessage);
                }

                if (Expression != null)
                {
					msg.Append( " [" ) ;
					msg.Append( Expression ) ;
					msg.Append( ']' ) ;
                }
				
                return msg.ToString();
            }
        }

        /// <summary> Ctor.</summary>
        /// <param name="message">error message
        /// </param>
        /// <param name="expression">expression text
        /// </param>
        public EPStatementException(String message, String expression)
            : base(message)
        {
            Expression = expression;
        }


        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="cause">The inner cause.</param>
        /// <param name="expression">expression text</param>
        public EPStatementException(String message, Exception cause, String expression)
            : base(message, cause)
        {
            Expression = expression;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="expression">expression text</param>
        /// <param name="innerException">The inner exception.</param>
        public EPStatementException(String message, String expression, Exception innerException)
            : base(message, innerException)
        {
            Expression = expression;
        }
    }
}
