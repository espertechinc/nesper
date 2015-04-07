///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esperio.message.fix
{
    /// <summary>Exception parsing a fix message. </summary>
    public class FixMsgUnrecognizableException : FixMsgParserException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        /// <param name="fixMsgText">fix text</param>
        public FixMsgUnrecognizableException(string message, string fixMsgText)
            : base(message + " for message text '" + fixMsgText + "'")
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        public FixMsgUnrecognizableException(string message)
            : base(message)
        {
        }
    }
}
