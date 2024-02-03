///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    public class DIOPrimitiveByteArray2DimNullableSerde : DataInputOutputSerdeBase<byte[][]>
    {
        public static readonly DIOPrimitiveByteArray2DimNullableSerde INSTANCE =
            new DIOPrimitiveByteArray2DimNullableSerde();

        private DIOPrimitiveByteArray2DimNullableSerde()
        {
        }

        public override void Write(
            byte[][] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                WriteArray(i, output);
            }
        }

        public override byte[][] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new byte[len][];
            for (var i = 0; i < len; i++) {
                array[i] = ReadArray(input);
            }

            return array;
        }

        private void WriteArray(
            byte[] array,
            DataOutput output)
        {
            if (array == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(array.Length);
            foreach (var i in array) {
                output.WriteByte(i);
            }
        }

        private byte[] ReadArray(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new byte[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadByte();
            }

            return array;
        }
    }
} // end of namespace