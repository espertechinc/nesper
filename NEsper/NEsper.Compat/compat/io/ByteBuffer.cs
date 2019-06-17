using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.compat.io
{
    public class ByteBuffer
    {
        private byte[] _buffer;

        public ByteBuffer(byte[] source)
        {
            _buffer = source;
        }

        public byte[] Array {
            get => _buffer;
            set => _buffer = value;
        }
    }
}
