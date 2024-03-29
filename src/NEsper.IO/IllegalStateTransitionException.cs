///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esperio
{
	/// <summary>
	/// Thrown when an illegal Adapter state transition is attempted.
	/// </summary>
	public class IllegalStateTransitionException : EPException
	{
		/// <summary>
		/// <param name="message">an explanation of the cause of the exception</param>
		/// </summary>
		public IllegalStateTransitionException(string message)
			: base( message )
		{
		}
	}
}
