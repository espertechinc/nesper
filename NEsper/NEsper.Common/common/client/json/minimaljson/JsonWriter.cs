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
	public class JsonWriter {

	    private const int CONTROL_CHARACTERS_END = 0x001f;

	    private const char[] QUOT_CHARS = {'\\', '"'};
	    private const char[] BS_CHARS = {'\\', '\\'};
	    private const char[] LF_CHARS = {'\\', 'n'};
	    private const char[] CR_CHARS = {'\\', 'r'};
	    private const char[] TAB_CHARS = {'\\', 't'};
	    // In JavaScript, U+2028 and U+2029 characters count as line endings and must be encoded.
	    // http://stackoverflow.com/questions/2965293/javascript-parse-error-on-u2028-unicode-character
	    private const char[] UNICODE_2028_CHARS = {'\\', 'u', '2', '0', '2', '8'};
	    private const char[] UNICODE_2029_CHARS = {'\\', 'u', '2', '0', '2', '9'};
	    private const char[] HEX_DIGITS = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
	        'a', 'b', 'c', 'd', 'e', 'f'};

	    public readonly Writer writer;

	    JsonWriter(Writer writer) {
	        this.writer = writer;
	    }

	    public void WriteLiteral(string value) {
	        writer.Write(value);
	    }

	    public void WriteNumber(string string) {
	        writer.Write(string);
	    }

	    public void WriteString(string string) {
	        writer.Write('"');
	        WriteJsonString(string);
	        writer.Write('"');
	    }

	    public void WriteArrayOpen() {
	        writer.Write('[');
	    }

	    public void WriteArrayClose() {
	        writer.Write(']');
	    }

	    public void WriteArraySeparator() {
	        writer.Write(',');
	    }

	    public void WriteObjectOpen() {
	        writer.Write('{');
	    }

	    public void WriteObjectClose() {
	        writer.Write('}');
	    }

	    public void WriteMemberName(string name) {
	        writer.Write('"');
	        WriteJsonString(name);
	        writer.Write('"');
	    }

	    public void WriteMemberSeparator() {
	        writer.Write(':');
	    }

	    public void WriteObjectSeparator() {
	        writer.Write(',');
	    }

	    public void WriteJsonString(string string) {
	        int length = string.Length();
	        int start = 0;
	        for (int index = 0; index < length; index++) {
	            char[] replacement = GetReplacementChars(string.CharAt(index));
	            if (replacement != null) {
	                writer.Write(string, start, index - start);
	                writer.Write(replacement);
	                start = index + 1;
	            }
	        }
	        writer.Write(string, start, length - start);
	    }

	    private static char[] GetReplacementChars(char ch) {
	        if (ch > '\\') {
	            if (ch < '\u2028' || ch > '\u2029') {
	                // The lower range contains 'a' .. 'z'. Only 2 checks required.
	                return null;
	            }
	            return ch == '\u2028' ? UNICODE_2028_CHARS : UNICODE_2029_CHARS;
	        }
	        if (ch == '\\') {
	            return BS_CHARS;
	        }
	        if (ch > '"') {
	            // This range contains '0' .. '9' and 'A' .. 'Z'. Need 3 checks to get here.
	            return null;
	        }
	        if (ch == '"') {
	            return QUOT_CHARS;
	        }
	        if (ch > CONTROL_CHARACTERS_END) {
	            return null;
	        }
	        if (ch == '\n') {
	            return LF_CHARS;
	        }
	        if (ch == '\r') {
	            return CR_CHARS;
	        }
	        if (ch == '\t') {
	            return TAB_CHARS;
	        }
	        return new char[]{'\\', 'u', '0', '0', HEX_DIGITS[ch >> 4 & 0x000f], HEX_DIGITS[ch & 0x000f]};
	    }

	}
} // end of namespace
