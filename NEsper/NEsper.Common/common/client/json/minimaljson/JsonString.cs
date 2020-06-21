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
	    // use default serial UID
	public class JsonString : JsonValue {

	    private readonly string string;

	    public JsonString(string string) {
	        if (string == null) {
	            throw new NullPointerException("string is null");
	        }
	        this.string = string;
	    }

	    @Override
	    void Write(JsonWriter writer) {
	        writer.WriteString(string);
	    }

	    @Override
	    public bool IsString() {
	        return true;
	    }

	    @Override
	    public string AsString() {
	        return string;
	    }

	    @Override
	    public int HashCode() {
	        return string.HasHCode;
	    }

	    @Override
	    public bool Equals(object object) {
	        if (this == object) {
	            return true;
	        }
	        if (object == null) {
	            return false;
	        }
	        if (GetType() != object.GetType()) {
	            return false;
	        }
	        JsonString other = (JsonString) object;
	        return string.Equals(other.string);
	    }

	}
} // end of namespace
