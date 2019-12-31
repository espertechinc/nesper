///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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