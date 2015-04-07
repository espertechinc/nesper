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
	/// This exception is thrown to indicate a problem in administration and runtime.
	/// </summary>
	[Serializable]
	public class EPException : Exception
	{
	    private static readonly Type MyType = 
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

		/// <summary> Ctor.</summary>
		/// <param name="message">error message
		/// </param>
		public EPException(String message)
			: base(message)
		{
		}
		
		/// <summary> Ctor for an inner exception and message.</summary>
		/// <param name="message">error message
		/// </param>
		/// <param name="cause">inner exception
		/// </param>
		public EPException(String message, Exception cause)
			: base(message, cause)
		{
		}
		
		/// <summary> Ctor - just an inner exception.</summary>
		/// <param name="cause">inner exception
		/// </param>
		public EPException(Exception cause)
			: base(MyType.FullName + ": " + cause.Message, cause)
		{
		}
	}
}
