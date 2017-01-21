///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// A general purpose exception for timer events
    /// </summary>

    public class TimerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerException"/> class.
        /// </summary>
        public TimerException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TimerException(string message) : base(message) { }
    }
}
