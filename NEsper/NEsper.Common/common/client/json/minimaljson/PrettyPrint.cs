///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
	/// <summary>
	/// Enables human readable JSON output by inserting whitespace between values.after commas and
	/// colons. Example:
	/// jsonValue.writeTo(writer, PrettyPrint.singleLine());
	/// </summary>
	public class PrettyPrint : WriterConfig {

	    private readonly char[] indentChars;

	    protected PrettyPrint(char[] indentChars) {
	        this.indentChars = indentChars;
	    }

	    /// <summary>
	    /// Print every value on a separate line. Use tabs (<code>\t</code>) for indentation.
	    /// </summary>
	    /// <returns>A PrettyPrint instance for wrapped mode with tab indentation</returns>
	    public static PrettyPrint SingleLine() {
	        return new PrettyPrint(null);
	    }

	    /// <summary>
	    /// Print every value on a separate line. Use the given number of spaces for indentation.
	    /// </summary>
	    /// <param name="number">the number of spaces to use</param>
	    /// <returns>A PrettyPrint instance for wrapped mode with spaces indentation</returns>
	    public static PrettyPrint IndentWithSpaces(int number) {
	        if (number < 0) {
	            throw new ArgumentException("number is negative");
	        }
	        char[] chars = new char[number];
	        Arrays.Fill(chars, ' ');
	        return new PrettyPrint(chars);
	    }

	    /// <summary>
	    /// Do not break lines, but still insert whitespace between values.
	    /// </summary>
	    /// <returns>A PrettyPrint instance for single-line mode</returns>
	    public static PrettyPrint IndentWithTabs() {
	        return new PrettyPrint(new char[]{'\t'});
	    }

	    @Override
	    public JsonWriter CreateWriter(Writer writer) {
	        return new PrettyPrintWriter(writer, indentChars);
	    }

	    private static class PrettyPrintWriter : JsonWriter {

	        private readonly char[] indentChars;
	        private int indent;

	        private PrettyPrintWriter(Writer writer, char[] indentChars) {
	            Super(writer);
	            this.indentChars = indentChars;
	        }

	        @Override
	        public void WriteArrayOpen() {
	            indent++;
	            writer.Write('[');
	            WriteNewLine();
	        }

	        @Override
	        public void WriteArrayClose() {
	            indent--;
	            WriteNewLine();
	            writer.Write(']');
	        }

	        @Override
	        public void WriteArraySeparator() {
	            writer.Write(',');
	            if (!WriteNewLine()) {
	                writer.Write(' ');
	            }
	        }

	        @Override
	        public void WriteObjectOpen() {
	            indent++;
	            writer.Write('{');
	            WriteNewLine();
	        }

	        @Override
	        public void WriteObjectClose() {
	            indent--;
	            WriteNewLine();
	            writer.Write('}');
	        }

	        @Override
	        public void WriteMemberSeparator() {
	            writer.Write(':');
	            writer.Write(' ');
	        }

	        @Override
	        public void WriteObjectSeparator() {
	            writer.Write(',');
	            if (!WriteNewLine()) {
	                writer.Write(' ');
	            }
	        }

	        private bool WriteNewLine() {
	            if (indentChars == null) {
	                return false;
	            }
	            writer.Write('\n');
	            for (int i = 0; i < indent; i++) {
	                writer.Write(indentChars);
	            }
	            return true;
	        }

	    }

	}
} // end of namespace
