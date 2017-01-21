///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Indicates a problem importing classes, aggregation functions and the like.
	/// </summary>
	public class EngineImportException : Exception
	{
	    /// <summary>Ctor.</summary>
	    /// <param name="msg">exception message</param>
	    public EngineImportException(String msg)
			: base(msg)
	    {
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="msg">exception message</param>
	    /// <param name="ex">inner exception</param>
	    public EngineImportException(String msg, Exception ex)
			: base(msg, ex)
	    {
	    }
	}
} // End of namespace
