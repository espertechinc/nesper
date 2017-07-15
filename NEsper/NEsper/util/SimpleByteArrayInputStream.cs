///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.util
{
    /// <summary>Input stream that relies on a simple byte array, unchecked.</summary>
    public class SimpleByteArrayInputStream : InputStream{
        private byte[] buf = null;
        private int count = 0;
        private int pos = 0;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="buf">is the byte buffer</param>
        /// <param name="count">is the size of the buffer</param>
        public SimpleByteArrayInputStream(byte[] buf, int count) {
            this.buf = buf;
            this.count = count;
        }
    
        public int Available() {
            return count - pos;
        }
    
        public int Read() {
            return (pos < count) ? (buf[pos++] & 0xff) : -1;
        }
    
        public int Read(byte[] b, int off, int len) {
            if (pos >= count) {
                return -1;
            }
    
            if ((pos + len) > count) {
                len = count - pos;
            }
    
            System.Arraycopy(buf, pos, b, off, len);
            pos += len;
            return len;
        }
    
        public long Skip(long n) {
            if ((pos + n) > count) {
                n = count - pos;
            }
            if (n < 0) {
                return 0;
            }
            pos += n;
            return n;
        }
    }
} // end of namespace
