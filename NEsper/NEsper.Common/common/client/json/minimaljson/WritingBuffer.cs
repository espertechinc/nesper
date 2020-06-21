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
	/// A lightweight writing buffer to reduce the amount of write operations to be performed on the
	/// underlying writer. This implementation is not thread-safe. It deliberately deviates from the
	/// contract of Writer. In particular, it does not flush or close the wrapped writer nor does it
	/// ensure that the wrapped writer is open.
	/// </summary>
	public class WritingBuffer : Writer {

	    private readonly Writer writer;
	    private readonly char[] buffer;
	    private int fill = 0;

	    WritingBuffer(Writer writer) {
	        This(writer, 16);
	    }

	    public WritingBuffer(Writer writer, int bufferSize) {
	        this.writer = writer;
	        buffer = new char[bufferSize];
	    }

	    @Override
	    public void Write(int c) {
	        if (fill > buffer.Length - 1) {
	            Flush();
	        }
	        buffer[fill++] = (char) c;
	    }

	    @Override
	    public void Write(char[] cbuf, int off, int len) {
	        if (fill > buffer.Length - len) {
	            Flush();
	            if (len > buffer.Length) {
	                writer.Write(cbuf, off, len);
	                return;
	            }
	        }
	        System.Arraycopy(cbuf, off, buffer, fill, len);
	        fill += len;
	    }

	    @Override
	    public void Write(string str, int off, int len) {
	        if (fill > buffer.Length - len) {
	            Flush();
	            if (len > buffer.Length) {
	                writer.Write(str, off, len);
	                return;
	            }
	        }
	        str.GetChars(off, off + len, buffer, fill);
	        fill += len;
	    }

	    /// <summary>
	    /// Flushes the internal buffer but does not flush the wrapped writer.
	    /// </summary>
	    @Override
	    public void Flush() {
	        writer.Write(buffer, 0, fill);
	        fill = 0;
	    }

	    /// <summary>
	    /// Does not close or flush the wrapped writer.
	    /// </summary>
	    @Override
	    public void Close() {
	    }

	}
} // end of namespace
