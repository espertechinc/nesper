///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace net.esper.support.util
{
	public class SimpleByteArrayInputStream
	{
	    protected byte[] buf = null;
	    protected int count = 0;
	    protected int pos = 0;

	    public SimpleByteArrayInputStream(byte[] buf, int count)
	    {
	        this.buf = buf;
	        this.count = count;
	    }

	    public int Available()
	    {
	        return count - pos;
	    }

	    public int Read()
	    {
	        return (pos < count) ? (buf[pos++] & 0xff) : -1;
	    }

	    public int Read(byte[] b, int off, int len)
	    {
	        if (pos >= count)
	        {
	            return -1;
	        }

	        if ((pos + len) > count)
	        {
	            len = (count - pos);
	        }

	        Array.Copy(buf, pos, b, off, len);
	        pos += len;
	        return len;
	    }

	    public long Skip(int n)
	    {
	        if ((pos + n) > count)
	        {
	            n = count - pos;
	        }
	        if (n < 0)
	        {
	            return 0;
	        }
	        pos += n;
	        return n;
	    }
	}
} // End of namespace
