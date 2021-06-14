///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.datetimemethod
{
	/// <summary>
	///     For adding a date-time method that reformats the date-time value returning a result of a different type
	///     as the date-time value.
	///     <para />
	///     Make sure to set a return type.
	/// </summary>
	public class DateTimeMethodOpsReformat : DateTimeMethodOps
    {
	    /// <summary>
	    ///     Returns the information how DateTimeEx-reformat is provided
	    /// </summary>
	    /// <value>mode</value>
	    public DateTimeMethodMode DateTimeExOp { get; set; }

	    /// <summary>
	    ///     Returns the information how DateTimeOffset-reformat is provided
	    /// </summary>
	    /// <value>mode</value>
	    public DateTimeMethodMode DateTimeOffsetOp { get; set; }

	    /// <summary>
	    ///     Returns the information how DateTime-reformat is provided
	    /// </summary>
	    /// <value>mode</value>
	    public DateTimeMethodMode DateTimeOp { get; set; }

	    /// <summary>
	    ///     Returns the information how long-reformat is provided
	    /// </summary>
	    /// <value>mode</value>
	    public DateTimeMethodMode LongOp { get; set; }

	    /// <summary>
	    ///     Returns the return type.
	    /// </summary>
	    /// <value>return type</value>
	    public Type ReturnType { get; set; }
    }
} // end of namespace