///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.io
{
    public class ByteBuffer
    {
        private readonly byte[] _buffer;

        public ByteBuffer(byte[] source)
        {
            _buffer = source ?? new byte[0];
        }

        public byte[] Array => _buffer;

        public int Length => _buffer.Length;

        protected bool Equals(ByteBuffer other)
        {
            if (_buffer.Length != other._buffer.Length) {
                return false;
            }

            for (int ii = 0; ii < _buffer.Length; ii++) {
                if (_buffer[ii] != other._buffer[ii]) {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((ByteBuffer) obj);
        }

        public override int GetHashCode()
        {
            if (_buffer.Length == 0) {
                return 0;
            }

            int hash = _buffer[0];
            for (int ii = 1; ii < _buffer.Length; ii++) {
                hash = hash * 397 ^ _buffer[1];
            }

            return hash;
        }
    }
}