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
	/// Represents a member of a JSON object, a pair of a name and a value.
	/// </summary>
	public class Member {

	    private readonly string name;
	    private readonly JsonValue value;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="name">name</param>
	    /// <param name="value">value</param>
	    public Member(string name, JsonValue value) {
	        this.name = name;
	        this.value = value;
	    }

	    /// <summary>
	    /// Returns the name of this member.
	    /// </summary>
	    /// <returns>the name of this member, never &lt;code&gt;null&lt;/code&gt;</returns>
	    public string GetName() {
	        return name;
	    }

	    /// <summary>
	    /// Returns the value of this member.
	    /// </summary>
	    /// <returns>the value of this member, never &lt;code&gt;null&lt;/code&gt;</returns>
	    public JsonValue GetValue() {
	        return value;
	    }

	    protected bool Equals(Member other)
	    {
		    return name == other.name && Equals(value, other.value);
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

		    return Equals((Member) obj);
	    }

	    public override int GetHashCode()
	    {
		    unchecked {
			    return ((name != null ? name.GetHashCode() : 0) * 397) ^ (value != null ? value.GetHashCode() : 0);
		    }
	    }
	}
} // end of namespace
