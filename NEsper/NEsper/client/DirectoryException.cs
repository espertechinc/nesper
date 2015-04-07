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
	public class DirectoryException : Exception
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryException"/> class.
        /// </summary>
		public DirectoryException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
		public DirectoryException( string message ) : base( message ) { }

	}
}
