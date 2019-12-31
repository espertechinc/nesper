///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace NEsper.Benchmark.Common
{
    public sealed class ByteBuffer
    {
        private readonly byte[] buffer;
        /// <summary>
        /// Index of the zero byte
        /// </summary>
        private int rIndex;
        /// <summary>
        /// Index where new writes should be written
        /// </summary>
        private int wIndex;
        /// <summary>
        /// Amount of space remaining in the buffer
        /// </summary>
        private int currCapacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBuffer"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ByteBuffer( int capacity )
        {
            buffer = new byte[capacity];
            wIndex = 0;
            rIndex = 0;
            currCapacity = capacity;
        }

        /// <summary>
        /// Gets the capacity.
        /// </summary>
        /// <value>The capacity.</value>
        public int Capacity
        {
            get { return currCapacity; }
        }

        /// <summary>
        /// Gets the number of bytes used.
        /// </summary>
        /// <value>The used.</value>
        public int Length
        {
            get { return buffer.Length - currCapacity; }
        }

        /// <summary>
        /// Gets the <see cref="System.Byte"/> at the specified index.
        /// </summary>
        /// <value></value>
        public byte this[int index]
        {
            get
            {
                int trueIndex = (rIndex + index)%buffer.Length;
                return buffer[trueIndex];
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            rIndex = 0;
            wIndex = 0;
            currCapacity = buffer.Length;
        }

        /// <summary>
        /// Advances the read index in the buffer.  We don't bother to do
        /// sanity checks, we expect you to have already done them.
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        public void UnsafeAdvance( int byteCount )
        {
            rIndex += byteCount;
            rIndex %= buffer.Length;
            currCapacity += byteCount;
        }

        /// <summary>
        /// Writes all bytes to the byteWriter.  Handles underlying issues
        /// in the byte stream.
        /// </summary>
        /// <param name="byteWriter">The byte writer.</param>
        /// <param name="advance">if set to <c>true</c> [advance].</param>
        public void WriteAll( ByteWriter byteWriter, bool advance)
        {
            int byteCount = buffer.Length - currCapacity;

            // Can we copy it all in one shot
            int distToEnd = buffer.Length - rIndex;
            if (distToEnd >= byteCount)
            {
                byteWriter.Invoke(buffer, rIndex, byteCount);
            }
            else
            {
                byteWriter.Invoke(buffer, rIndex, distToEnd);
                byteWriter.Invoke(buffer, 0, byteCount - distToEnd);
            }

            if (advance) {
                rIndex += byteCount;
                rIndex %= buffer.Length;

                currCapacity += byteCount;
            }
        }

        /// <summary>
        /// Reads all.
        /// </summary>
        /// <returns></returns>
        public byte[] ReadAll()
        {
            return ReadAll(true);
        }

        /// <summary>
        /// Reads all.
        /// </summary>
        /// <returns></returns>
        public byte[] ReadAll(bool advance)
        {
            int buffLength = buffer.Length;
            int byteCount = buffLength - currCapacity;
            byte[] data = new byte[byteCount];

            // Can we copy it all in one shot
            int distToEnd = buffLength - rIndex;
            if (distToEnd >= byteCount)
            {
                Array.Copy(buffer, rIndex, data, 0, byteCount);
            }
            else
            {
                Array.Copy(buffer, rIndex, data, 0, distToEnd);
                Array.Copy(buffer, 0, data, distToEnd, byteCount - distToEnd);
            }

            if (advance) {
                rIndex = (rIndex + byteCount)%buffLength;
                currCapacity += byteCount;
            }

            return data;
        }

        /// <summary>
        /// Reads the specified data from the buffer.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="byteCount">The byte count.</param>
        /// <returns></returns>
        public int Read( byte[] data, int offset, int byteCount )
        {
            int buffLength = buffer.Length;
            int maxCount = buffLength - currCapacity;
            if ( byteCount > maxCount ) {
                byteCount = maxCount;
            }

            // Can we copy it all in one shot
            int distToEnd = buffLength - rIndex;
            if ( distToEnd >= byteCount ) {
                Array.Copy(buffer, rIndex, data, offset, byteCount);
            } else {
                Array.Copy(buffer, rIndex, data, offset, distToEnd);
                Array.Copy(buffer, 0, data, distToEnd, byteCount - distToEnd);
            }

            rIndex = (rIndex + byteCount) % buffer.Length;
            currCapacity += byteCount;

            return byteCount;
        }

        /// <summary>
        /// Writes the specified data to the buffer.  If the buffer is full,
        /// the method returns false and writes nothing.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="byteCount">The byte count.</param>
        /// <returns></returns>
        public bool Write( byte[] data, int offset, int byteCount )
        {
            // Can the entire buffer be written?
            if ( byteCount > currCapacity ) {
                return false;
            }

            // Can the entire buffer be written in one shot or do
            // we need to wrap around and write at the beginning?
            int buffLength = buffer.Length;
            int distToEnd = buffLength - wIndex;
            if ( distToEnd >= byteCount ) {
                Array.Copy(data, offset, buffer, wIndex, byteCount);
            } else {
                Array.Copy(data, offset, buffer, wIndex, distToEnd);
                Array.Copy(data, offset + distToEnd, buffer, 0, byteCount - distToEnd);
            }

            wIndex = (wIndex + byteCount)%buffLength;

            // Decrement the current capacity
            currCapacity -= byteCount;

            return true;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return
                "ByteBuffer{" +
                "rIndex=" + rIndex + ", " +
                "wIndex=" + wIndex + ", " +
                "capacity=" + currCapacity + ", " +
                "length=" + Length +
                "}";
        }
    }

    public delegate void ByteWriter( byte[] data, int offset, int length ) ;
}
