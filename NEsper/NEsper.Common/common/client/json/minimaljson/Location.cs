///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

// <summary>
// ATTRIBUTION NOTICE
// ==================
// MinimalJson is a fast and minimal JSON parser and writer for Java. It's not an object mapper, but a bare-bones library that aims at being:
// - fast: high performance comparable with other state-of-the-art parsers (see below)
// - lightweight: object representation with minimal memory footprint (e.g. no HashMaps)
// - simple: reading, writing and modifying JSON with minimal code (short names, fluent style)
// Minimal JSON can be found at https://github.com/ralfstx/minimal-json.
// Minimal JSON is licensed under the MIT License, see https://github.com/ralfstx/minimal-json/blob/master/LICENSE
// </summary>

namespace com.espertech.esper.common.client.json.minimaljson
{
	/// <summary>
	/// An immutable object that represents a location in the parsed text.
	/// </summary>
	public class Location
	{
	    /// <summary>
	    /// The absolute character index, starting at 0.
	    /// </summary>
	    public readonly int offset;

	    /// <summary>
	    /// The line number, starting at 1.
	    /// </summary>
	    public readonly int line;

	    /// <summary>
	    /// The column number, starting at 1.
	    /// </summary>
	    public readonly int column;

	    Location(int offset, int line, int column) {
	        this.offset = offset;
	        this.column = column;
	        this.line = line;
	    }

	    public override string ToString()
	    {
		    return $"{nameof(offset)}: {offset}, {nameof(line)}: {line}, {nameof(column)}: {column}";
	    }

	    protected bool Equals(Location other)
	    {
		    return offset == other.offset && line == other.line && column == other.column;
	    }

	    public override bool Equals(object obj)
	    {
		    if (ReferenceEquals(null, obj)) {
			    return false;
		    }

		    if (ReferenceEquals(this, obj)) {
			    return true;
		    }

		    if (obj.GetType() != this.GetType()) {
			    return false;
		    }

		    return Equals((Location) obj);
	    }

	    public override int GetHashCode()
	    {
		    unchecked {
			    var hashCode = offset;
			    hashCode = (hashCode * 397) ^ line;
			    hashCode = (hashCode * 397) ^ column;
			    return hashCode;
		    }
	    }
	}
} // end of namespace
