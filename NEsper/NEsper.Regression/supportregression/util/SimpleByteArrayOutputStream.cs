///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.IO;

namespace com.espertech.esper.supportregression.util
{
	public class SimpleByteArrayOutputStream : OutputStream {
	    private byte[] _buf = null;
	    private int _size = 0;

	    public SimpleByteArrayOutputStream() : this(5 * 1024)
        {
	    }

	    public SimpleByteArrayOutputStream(int initSize) {
	        _size = 0;
	        _buf = new byte[initSize];
	    }

	    /// <summary>
	    /// Ensures that we have a large enough buffer for the given size.
	    /// </summary>
	    private void VerifyBufferSize(int sz) {
	        if (sz > _buf.Length) {
	            byte[] old = _buf;
	            _buf = new byte[Math.Max(sz, 2 * _buf.Length )];
	            Array.Copy(old, 0, _buf, 0, old.Length);
	            old = null;
	        }
	    }

	    public override long Length
	    {
	        get { return _size; }
	    }

	    public byte[] GetByteArray() {
	        return _buf;
	    }

	    public void Write(byte[] b) {
	        VerifyBufferSize(_size + b.Length);
	        Array.Copy(b, 0, _buf, _size, b.Length);
	        _size += b.Length;
	    }

	    public override void Write(byte[] b, int off, int len) {
	        VerifyBufferSize(_size + len);
	        Array.Copy(b, off, _buf, _size, len);
	        _size += len;
	    }

	    public void Write(int b) {
	        VerifyBufferSize(_size + 1);
	        _buf[_size++] = (byte) b;
	    }

	    public void Reset() {
	        _size = 0;
	    }

	    public override void Flush()
	    {
	        throw new NotImplementedException();
	    }

	    public InputStream GetInputStream() {
	        return new SimpleByteArrayInputStream(_buf, _size);
	    }

	}
} // end of namespace
