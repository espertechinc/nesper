///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;

using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.util.serde
{
    /// <summary>
    /// An <see cref="IBufferWriter{T}" /> backed by an array rented from
    /// <see cref="ArrayPool{T}" />. The completed payload is copied out at most once,
    /// via <see cref="ToArray" />, or not at all when streamed with <see cref="WriteTo" />.
    /// Not thread-safe; dispose to return the array to the pool.
    /// </summary>
    public sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private const int DEFAULT_INITIAL_CAPACITY = 1024;

        private byte[] _buffer;
        private int _written;

        public PooledByteBufferWriter(int initialCapacity = DEFAULT_INITIAL_CAPACITY)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        public int WrittenCount => _written;

        public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);

        public void Advance(int count)
        {
            if (count < 0 || _written + count > _buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _written += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureFree(sizeHint);
            return _buffer.AsMemory(_written);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureFree(sizeHint);
            return _buffer.AsSpan(_written);
        }

        /// <summary>Copies the written payload into a right-sized array.</summary>
        public byte[] ToArray()
        {
            return WrittenSpan.ToArray();
        }

        /// <summary>Writes the payload to the output without an intermediate copy.</summary>
        public void WriteTo(DataOutput output)
        {
            output.Write(_buffer, 0, _written);
        }

        private void EnsureFree(int sizeHint)
        {
            if (sizeHint < 1) {
                sizeHint = 1;
            }

            if (_buffer.Length - _written >= sizeHint) {
                return;
            }

            var newBuffer = ArrayPool<byte>.Shared.Rent(Math.Max(_buffer.Length * 2, _written + sizeHint));
            Array.Copy(_buffer, newBuffer, _written);
            var oldBuffer = _buffer;
            _buffer = newBuffer;
            ArrayPool<byte>.Shared.Return(oldBuffer);
        }

        public void Dispose()
        {
            var buffer = _buffer;
            if (buffer != null) {
                _buffer = null;
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
