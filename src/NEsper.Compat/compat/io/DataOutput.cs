///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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

        void WriteChar(char value);

        void WriteUTF(string value);

        void Write(byte[] bytes, int offset, int length);

        void Write(byte[] bytes);
    }
}