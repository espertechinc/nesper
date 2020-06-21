///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

namespace com.espertech.esper.compat.io
{
    public class BinaryDataOutput : DataOutput, IDisposable
    {
        private readonly BinaryWriter _binaryWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataOutput"/> class.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        public BinaryDataOutput(Stream stream)
        {
            _binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _binaryWriter.Dispose();
        }

        public void WriteBoolean(bool value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteByte(byte value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteShort(short value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteInt(int value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteLong(long value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteFloat(float value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteDouble(double value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteDecimal(decimal value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteChar(char value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteUTF(string value)
        {
            _binaryWriter.Write(value);
        }

        public void Write(
            byte[] bytes,
            int offset,
            int length)
        {
            _binaryWriter.Write(length);
            _binaryWriter.Write(bytes, offset, length);
        }

        public void Write(byte[] bytes)
        {
            _binaryWriter.Write(bytes.Length);
            _binaryWriter.Write(bytes);
        }
    }
}