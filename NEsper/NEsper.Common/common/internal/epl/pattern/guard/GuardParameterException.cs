///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
	/// <summary>Thrown to indicate a validation error in guard parameterization.</summary>
	public class GuardParameterException : Exception
	{
	    /// <summary>Ctor.</summary>
	    /// <param name="message">validation error message</param>
	    public GuardParameterException(String message)
	        : base(message)
	    {
	    }
	}
} // End of namespace