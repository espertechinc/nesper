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
    /// This exception is thrown to indicate a problem isolating statements.
    /// </summary>
    [Serializable]
    public class EPServiceIsolationException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        public EPServiceIsolationException(String message)
            : base(message)
        {
        }
    
        /// <summary>
        /// Ctor for an inner exception and message.
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="inner">inner exception</param>
        public EPServiceIsolationException(String message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
