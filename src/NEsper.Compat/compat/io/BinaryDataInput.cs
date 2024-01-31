///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class BinaryDataInput : DataInput, IDisposable
    {
        private readonly BinaryReader _binaryReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataInput"/> class.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public BinaryDataInput(Stream stream)
        {
            _binaryReader = new BinaryReader(stream, Encoding.UTF8, true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _binaryReader.Dispose();
        }

        public bool ReadBoolean()
        {
            return _binaryReader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return _binaryReader.ReadByte();
        }

        public short ReadShort()
        {
            return _binaryReader.ReadInt16();
        }

        public int ReadInt()
        {
            return _binaryReader.ReadInt32();
        }

        public long ReadLong()
        {
            return _binaryReader.ReadInt64();
        }

        public float ReadFloat()
        {
            return _binaryReader.ReadSingle();
        }

        public double ReadDouble()
        {
            return _binaryReader.ReadDouble();
        }

        public decimal ReadDecimal()
        {
            return _binaryReader.ReadDecimal();
        }

        public char ReadChar()
        {
            return _binaryReader.ReadChar();
        }

        public string ReadUTF()
        {
            return _binaryReader.ReadString();
        }

        public void ReadFully(byte[] bytes)
        {
            var length = _binaryReader.ReadInt32();
            var result = _binaryReader.ReadBytes(length);
            Array.Copy(result, bytes, length);
        }

        public void ReadFully(
            byte[] bytes,
            int offset,
            int askLength)
        {
            var length = _binaryReader.ReadInt32();
            var result = _binaryReader.ReadBytes(length);
            Array.Copy(result, 0, bytes, offset, Math.Min(askLength, length));
        }
    }
}