///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esperio.message.fix
{
    /// <summary>Indicates an invalid Fix message. </summary>
    public class FixMsgInvalidException : FixMsgParserException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixMsgInvalidException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public FixMsgInvalidException(string message) : base(message)
        {
        }
    }
}
