using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.compat.io
{
    public interface DataOutput
    {
        void WriteBoolean(bool value);
        void WriteByte(byte value);
        void WriteShort(short value);
        void WriteInt(int value);
        void WriteLong(long value);
        void WriteFloat(float value);
        void WriteDouble(double value);
        void WriteDecimal(decimal value);
        void WriteUTF(string value);
        void Write(byte[] bytes, int offset, int length);
        void Write(byte[] bytes);
    }
}
