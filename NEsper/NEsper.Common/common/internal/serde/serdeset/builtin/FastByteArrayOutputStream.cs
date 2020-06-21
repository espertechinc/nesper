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
	/// Output on fast byte array output stream.
	/// </summary>
	public class FastByteArrayOutputStream : OutputStream {
	    /// <summary>
	    /// Buffer initial size.
	    /// </summary>
	    public const int DEFAULT_INITIAL_SIZE = 100;

	    /// <summary>
	    /// Buffer increase size, zero means double.
	    /// </summary>
	    public const int DEFAULT_INCREASE_SIZE = 0;

	    private byte[] bytes;
	    private int length;
	    private int increaseLength;

	    private static readonly byte[] ZERO_LENGTH_BYTE_ARRAY = new byte[0];

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="buffer">to write</param>
	    public FastByteArrayOutputStream(byte[] buffer) {
	        bytes = buffer;
	        increaseLength = DEFAULT_INCREASE_SIZE;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="buffer">to write</param>
	    /// <param name="increase">zero for</param>
	    public FastByteArrayOutputStream(byte[] buffer, int increase) {
	        bytes = buffer;
	        increaseLength = increase;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public FastByteArrayOutputStream() {
	        bytes = new byte[DEFAULT_INITIAL_SIZE];
	        increaseLength = DEFAULT_INCREASE_SIZE;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="initialSize">initial size</param>
	    public FastByteArrayOutputStream(int initialSize) {
	        bytes = new byte[initialSize];
	        increaseLength = DEFAULT_INCREASE_SIZE;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="initialSize">initial size</param>
	    /// <param name="increaseSize">increase size</param>
	    public FastByteArrayOutputStream(int initialSize, int increaseSize) {
	        bytes = new byte[initialSize];
	        increaseLength = increaseSize;
	    }

	    /// <summary>
	    /// Returns buffer size.
	    /// </summary>
	    /// <returns>size</returns>
	    public int Size() {
	        return length;
	    }

	    /// <summary>
	    /// Reset buffer.
	    /// </summary>
	    public void Reset() {
	        length = 0;
	    }

	    /// <summary>
	    /// Write byte.
	    /// </summary>
	    /// <param name="b">byte to write</param>
	    /// <throws>IOException for io errors</throws>
	    public void Write(int b) {
	        WriteFast(b);
	    }

	    public void Write(byte[] fromBuf) {
	        WriteFast(fromBuf);
	    }

	    public void Write(byte[] fromBuf, int offset, int length)
	        {
	        WriteFast(fromBuf, offset, length);
	    }

	    /// <summary>
	    /// Write bytes to another stream.
	    /// </summary>
	    /// <param name="out">other stream</param>
	    /// <throws>IOException if a write exception occurs</throws>
	    public void WriteTo(OutputStream @out) {
	        @out.Write(bytes, 0, length);
	    }

	    public override string ToString() {
	        return new string(bytes, 0, length);
	    }

	    /// <summary>
	    /// Outputs contents.
	    /// </summary>
	    /// <param name="encoding">to use</param>
	    /// <returns>contents</returns>
	    /// <throws>UnsupportedEncodingException when encoding is not supported</throws>
	    public string ToString(string encoding) {
	        return new string(bytes, 0, length, encoding);
	    }

	    /// <summary>
	    /// Returns the byte array.
	    /// </summary>
	    /// <returns>byte array</returns>
	    public byte[] GetByteArrayWithCopy() {
	        if (length == 0) {
	            return ZERO_LENGTH_BYTE_ARRAY;
	        } else {
	            byte[] toBuf = new byte[length];
	            System.Arraycopy(bytes, 0, toBuf, 0, length);

	            return toBuf;
	        }
	    }

	    /// <summary>
	    /// Fast getter checking if the byte array matches the requested length and returnin the buffer itself if it does.
	    /// </summary>
	    /// <returns>byte array without offset</returns>
	    public byte[] GetByteArrayFast() {
	        if (bytes.Length == length) {
	            return bytes;
	        }
	        return ByteArrayWithCopy;
	    }

	    /// <summary>
	    /// Fast write.
	    /// </summary>
	    /// <param name="b">byte to write</param>
	    public void WriteFast(int b) {
	        if (length + 1 > bytes.Length) {
	            Bump(1);
	        }

	        bytes[length++] = (byte) b;
	    }

	    /// <summary>
	    /// Fast write.
	    /// </summary>
	    /// <param name="fromBuf">to write</param>
	    public void WriteFast(byte[] fromBuf) {
	        int needed = length + fromBuf.Length - bytes.Length;
	        if (needed > 0) {
	            Bump(needed);
	        }

	        System.Arraycopy(fromBuf, 0, bytes, length, fromBuf.Length);
	        length += fromBuf.Length;
	    }

	    /// <summary>
	    /// Fast write.
	    /// </summary>
	    /// <param name="fromBuf">buffer to write</param>
	    /// <param name="offset">offset of write from</param>
	    /// <param name="length">length to write</param>
	    public void WriteFast(byte[] fromBuf, int offset, int length) {
	        int needed = this.Length + length - bytes.Length;
	        if (needed > 0)
	            Bump(needed);

	        System.Arraycopy(fromBuf, offset, bytes, this.Length, length);
	        this.Length += length;
	    }

	    /// <summary>
	    /// Returns the buffer itself.
	    /// </summary>
	    /// <returns>buffer</returns>
	    public byte[] GetBufferBytes() {
	        return bytes;
	    }

	    /// <summary>
	    /// Returns the offset, always zero.
	    /// </summary>
	    /// <returns>offset</returns>
	    public int GetBufferOffset() {
	        return 0;
	    }

	    /// <summary>
	    /// Returns the length.
	    /// </summary>
	    /// <returns>length</returns>
	    public int GetBufferLength() {
	        return length;
	    }

	    /// <summary>
	    /// Increase buffer size.
	    /// </summary>
	    /// <param name="sizeNeeded">bytes needed.</param>
	    public void MakeSpace(int sizeNeeded) {
	        int needed = length + sizeNeeded - bytes.Length;
	        if (needed > 0) {
	            Bump(needed);
	        }
	    }

	    /// <summary>
	    /// Add number of bytes to size.
	    /// </summary>
	    /// <param name="sizeAdded">to be added</param>
	    public void AddSize(int sizeAdded) {
	        length += sizeAdded;
	    }

	    private void Bump(int needed) {
	        int bump = (increaseLength > 0) ? increaseLength : bytes.Length;

	        byte[] toBuf = new byte[bytes.Length + needed + bump];

	        System.Arraycopy(bytes, 0, toBuf, 0, length);

	        bytes = toBuf;
	    }
	}
} // end of namespace
