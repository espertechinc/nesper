///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
	/// <summary>
	/// Output stream that relies on a simple byte array, unchecked.
	/// </summary>
	public class SimpleByteArrayOutputStream {
	    private byte[] buf = null;
	    private int size = 0;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public SimpleByteArrayOutputStream() : this(5 * 1024)
	        {
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="initSize">initial size</param>
	    public SimpleByteArrayOutputStream(int initSize) {
	        this.size = 0;
	        this.buf = new byte[initSize];
	    }

	    private void VerifyBufferSize(int sz) {
	        if (sz > buf.Length) {
	            byte[] old = buf;
	            buf = new byte[Math.Max(sz, 2 * buf.Length)];
	            Array.Copy(old, 0, buf, 0, old.Length);
	        }
	    }

	    public void Write(byte[] b) {
	        VerifyBufferSize(size + b.Length);
	        Array.Copy(b, 0, buf, size, b.Length);
	        size += b.Length;
	    }

	    public void Write(byte[] b, int off, int len) {
	        VerifyBufferSize(size + len);
	        Array.Copy(b, off, buf, size, len);
	        size += len;
	    }

	    public void Write(int b) {
	        VerifyBufferSize(size + 1);
	        buf[size++] = (byte) b;
	    }

	    /// <summary>
	    /// Return the input stream for the output buffer.
	    /// </summary>
	    /// <returns>input stream for existing buffer</returns>
	    public Stream InputStream
	    {
	        get => new SimpleByteArrayInputStream(buf, size);
	    }

	}
} // end of namespace