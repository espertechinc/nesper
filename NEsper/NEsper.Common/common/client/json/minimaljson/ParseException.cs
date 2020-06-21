///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

// ATTRIBUTION NOTICE
// ==================
// MinimalJson is a fast and minimal JSON parser and writer for Java. It's not an object mapper, but a bare-bones library that aims at being:
// - fast: high performance comparable with other state-of-the-art parsers (see below)
// - lightweight: object representation with minimal memory footprint (e.g. no HashMaps)
// - simple: reading, writing and modifying JSON with minimal code (short names, fluent style)
// Minimal JSON can be found at https://github.com/ralfstx/minimal-json.
// Minimal JSON is licensed under the MIT License, see https://github.com/ralfstx/minimal-json/blob/master/LICENSE

namespace com.espertech.esper.common.client.json.minimaljson
{
	/// <summary>
	/// An unchecked exception to indicate that an input does not qualify as valid JSON.
	/// </summary>
	// use default serial UID
	public class ParseException : RuntimeException {

	    private readonly Location location;

	    ParseException(string message, Location location) {
	        Super(message + " at " + location);
	        this.location = location;
	    }

	    /// <summary>
	    /// Returns the location at which the error occurred.
	    /// </summary>
	    /// <returns>the error location</returns>
	    public Location GetLocation() {
	        return location;
	    }

	    /// <summary>
	    /// Returns the absolute character index at which the error occurred. The offset of the first
	    /// character of a document is 0.
	    /// </summary>
	    /// <returns>the character offset at which the error occurred, will be &amp;gt;= 0</returns>
	    /// <unknown>@deprecated Use {@link #getLocation()} instead</unknown>
	    @Deprecated
	    public int GetOffset() {
	        return location.offset;
	    }

	    /// <summary>
	    /// Returns the line number in which the error occurred. The number of the first line is 1.
	    /// </summary>
	    /// <returns>the line in which the error occurred, will be &amp;gt;= 1</returns>
	    /// <unknown>@deprecated Use {@link #getLocation()} instead</unknown>
	    @Deprecated
	    public int GetLine() {
	        return location.line;
	    }

	    /// <summary>
	    /// Returns the column number at which the error occurred, i.e. the number of the character in its
	    /// line. The number of the first character of a line is 1.
	    /// </summary>
	    /// <returns>the column in which the error occurred, will be &amp;gt;= 1</returns>
	    /// <unknown>@deprecated Use {@link #getLocation()} instead</unknown>
	    @Deprecated
	    public int GetColumn() {
	        return location.column;
	    }

	}
} // end of namespace
