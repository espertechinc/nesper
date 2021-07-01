///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client.util
{
	/// <summary>
	/// EPL object type.
	/// </summary>
	public enum EPObjectType {
	    /// <summary>
	    /// Context.
	    /// </summary>
	    CONTEXT,
	    /// <summary>
	    /// Named window.
	    /// </summary>
	    NAMEDWINDOW,
	    /// <summary>
	    /// Event type.
	    /// </summary>
	    EVENTTYPE,
	    /// <summary>
	    /// Table.
	    /// </summary>
	    TABLE,
	    /// <summary>
	    /// Variable
	    /// </summary>
	    VARIABLE,
	    /// <summary>
	    /// Expression.
	    /// </summary>
	    EXPRESSION,
	    /// <summary>
	    /// Script.
	    /// </summary>
	    SCRIPT,
	    /// <summary>
	    /// Index.
	    /// </summary>
	    INDEX,
	    /// <summary>
	    /// Application-Inlined Class.
	    /// </summary>
	    CLASSPROVIDED
	}

	public static class EPObjectTypeExtensions
	{
		/// <summary>
		/// Returns the pretty-print name
		/// </summary>
		/// <returns>name</returns>
		public static string GetPrettyName(this EPObjectType value)
		{
			switch (value) {
				case EPObjectType.CONTEXT:
					return "context";

				case EPObjectType.NAMEDWINDOW:
					return "named window";

				case EPObjectType.EVENTTYPE:
					return "event type";

				case EPObjectType.TABLE:
					return "table";

				case EPObjectType.VARIABLE:
					return "variable";

				case EPObjectType.EXPRESSION:
					return "expression";

				case EPObjectType.SCRIPT:
					return "script";

				case EPObjectType.INDEX:
					return "index";

				case EPObjectType.CLASSPROVIDED:
					return "application-inlined class";

				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}
		}
	}
} // end of namespace
