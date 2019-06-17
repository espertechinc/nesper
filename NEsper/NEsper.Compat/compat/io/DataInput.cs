using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.compat.io
{
    public interface DataInput
    {
        bool ReadBoolean();
        byte ReadByte();
        short ReadShort();
        int ReadInt();
        long ReadLong();
        float ReadFloat();
        double ReadDouble();
        decimal ReadDecimal();
        string ReadUTF();

        void ReadFully(byte[] bytes);
        void ReadFully(byte[] bytes, int offset, int length);
    }
}
