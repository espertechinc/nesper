///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    /// Exception for use with <seealso cref="LockStrategy" />.
    /// </summary>
    public class LockStrategyException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">message</param>
        public LockStrategyException(string message)
            : base(message)
        {
        }
    }
} // end of namespace