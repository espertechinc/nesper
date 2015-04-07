///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.dispatch
{
	/// <summary> Provider of implementations for the dispatch service.</summary>
	public class DispatchServiceProvider
	{
		/// <summary> Returns new service.</summary>
		/// <returns> new dispatch service implementation.
		/// </returns>
		public static DispatchService NewService()
		{
			return new DispatchServiceImpl();
		}
	}
}
