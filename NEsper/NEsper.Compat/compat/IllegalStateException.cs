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
    /// An exception that occurs when some illegal state occurs.
    /// </summary>

	[Serializable]
	public class IllegalStateException : Exception
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalStateException"/> class.
        /// </summary>
		public IllegalStateException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalStateException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
		public IllegalStateException( string message ) : base( message ) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalStateException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The underlying exception.</param>
        public IllegalStateException(string message, Exception e) : base(message, e) { }
    }
}
