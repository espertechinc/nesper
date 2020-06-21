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
namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	/// <summary>
	/// Fast byte array input stream, does not synchronize or check buffer overflow.
	/// </summary>
	public class FastByteArrayInputStream : InputStream {
	    private byte[] bytes;
	    private int length;
	    private int offset;
	    private int currentMark;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="buffer">to use</param>
	    public FastByteArrayInputStream(byte[] buffer) {
	        bytes = buffer;
	        length = buffer.Length;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="buffer">buffer to use</param>
	    /// <param name="offset">offset to start at</param>
	    /// <param name="length">length of buffer to use</param>
	    public FastByteArrayInputStream(byte[] buffer, int offset, int length) {
	        bytes = buffer;
	        this.offset = offset;
	        this.Length = offset + length;
	    }

	    /// <summary>
	    /// Returns the buffer.
	    /// </summary>
	    /// <returns>buffer</returns>
	    public byte[] GetBytes() {
	        return bytes;
	    }

	    /// <summary>
	    /// Returns buffer length.
	    /// </summary>
	    /// <returns>length</returns>
	    public int GetLength() {
	        return length;
	    }

	    /// <summary>
	    /// Returns buffer offset.
	    /// </summary>
	    /// <returns>offset</returns>
	    public int GetOffset() {
	        return offset;
	    }

	    /// <summary>
	    /// Sets buffer.
	    /// </summary>
	    /// <param name="bytes">to set</param>
	    public void SetBytes(byte[] bytes) {
	        this.bytes = bytes;
	    }

	    /// <summary>
	    /// Set length of buffer.
	    /// </summary>
	    /// <param name="length">buffer length</param>
	    public void SetLength(int length) {
	        this.Length = length;
	    }

	    /// <summary>
	    /// Sets buffer offset.
	    /// </summary>
	    /// <param name="offset">to set</param>
	    public void SetOffset(int offset) {
	        this.offset = offset;
	    }

	    /// <summary>
	    /// Read bytes to buffer.
	    /// </summary>
	    /// <param name="target">target buffer</param>
	    /// <param name="offset">buffer offset</param>
	    /// <param name="length">buffer length</param>
	    /// <returns>number of bytes read</returns>
	    /// <throws>IOException indicates error</throws>
	    public int Read(byte[] target, int offset, int length) {
	        return ReadFast(target, offset, length);
	    }

	    public int Available() {
	        return length - offset;
	    }

	    public int Read() {
	        return ReadFast();
	    }

	    public int Read(byte[] toBuf) {
	        return ReadFast(toBuf, 0, toBuf.Length);
	    }

	    public void Reset() {
	        offset = currentMark;
	    }

	    public long Skip(long count) {
	        int now = (int) count;
	        if (now + offset > length) {
	            now = length - offset;
	        }
	        SkipFast(now);
	        return now;
	    }

	    public bool MarkSupported() {
	        return true;
	    }

	    public void Mark(int readLimit) {
	        currentMark = offset;
	    }

	    /// <summary>
	    /// Fast skip.
	    /// </summary>
	    /// <param name="count">bytes to skip</param>
	    public void SkipFast(int count) {
	        offset += count;
	    }

	    /// <summary>
	    /// Fast read without sync.
	    /// </summary>
	    /// <returns>read byte</returns>
	    public int ReadFast() {
	        return (offset < length) ? (bytes[offset++] & 0xff) : (-1);
	    }

	    /// <summary>
	    /// Read bytes.
	    /// </summary>
	    /// <param name="target">to fill</param>
	    /// <returns>num bytes</returns>
	    public int ReadFast(byte[] target) {
	        return ReadFast(target, 0, target.Length);
	    }

	    /// <summary>
	    /// Read bytes.
	    /// </summary>
	    /// <param name="target">to fill</param>
	    /// <param name="offset">to fill</param>
	    /// <param name="length">length to use</param>
	    /// <returns>num bytes read</returns>
	    public int ReadFast(byte[] target, int offset, int length) {
	        int available = this.Length - this.offset;
	        if (available <= 0) {
	            return -1;
	        }
	        if (length > available) {
	            length = available;
	        }
	        System.Arraycopy(bytes, this.offset, target, offset, length);
	        this.offset += length;
	        return length;
	    }
	}
} // end of namespace
