///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.enummethod
{
	/// <summary>
	/// Interface for state-providing classes for use with enumeration method extension
	/// </summary>
	public interface EnumMethodState
	{
		/// <summary>
		/// Called by the runtime to provide non-lambda expression parameter values
		/// </summary>
		/// <param name="parameterNumber">zero for the first parameter, reflects parameter position</param>
		/// <param name="value">parameter value or null if the parameter expression returned null</param>
		void SetParameter(
			int parameterNumber,
			object value);

	    /// <summary>
	    /// Called by the runtime only if during compile-time the mode indicated early-exit.
	    /// </summary>
	    /// <returns>indicator, true for done, false for more</returns>
	    bool IsCompleted { get; }

	    /// <summary>
	    /// Returns the enumeration result
	    /// </summary>
	    /// <returns>result</returns>
	    object State { get; }
	}
} // end of namespace
