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
	/// <summary>
	/// Controls the formatting of the JSON output. Use one of the available constants.
	/// </summary>
	public abstract class WriterConfig {

	    /// <summary>
	    /// Write JSON in its minimal form, without any additional whitespace. This is the default.
	    /// </summary>
	    public readonly static WriterConfig MINIMAL = new ProxyWriterConfig() {
	        Override
	        ProcCreateWriter = (writer) =>  {
	            return new JsonWriter(writer);
	        },
	    };

	    /// <summary>
	    /// Write JSON in pretty-print, with each value on a separate line and an indentation of two
	    /// spaces.
	    /// </summary>
	    public readonly static WriterConfig PRETTY_PRINT = PrettyPrint.IndentWithSpaces(2);

	    public abstract JsonWriter CreateWriter(Writer writer);

	}
} // end of namespace
