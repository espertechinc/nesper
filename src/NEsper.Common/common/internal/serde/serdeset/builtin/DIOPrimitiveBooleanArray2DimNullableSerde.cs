///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
    public class DIOPrimitiveBooleanArray2DimNullableSerde : DataInputOutputSerdeBase<bool[][]>
    {
        public static readonly DIOPrimitiveBooleanArray2DimNullableSerde INSTANCE =
            new DIOPrimitiveBooleanArray2DimNullableSerde();

        private DIOPrimitiveBooleanArray2DimNullableSerde()
        {
        }

        public override void Write(
            bool[][] @object,
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

        public override bool[][] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new bool[len][];
            for (var i = 0; i < len; i++) {
                array[i] = ReadArray(input);
            }

            return array;
        }

        private void WriteArray(
            bool[] array,
            DataOutput output)
        {
            if (array == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(array.Length);
            foreach (var i in array) {
                output.WriteBoolean(i);
            }
        }

        private bool[] ReadArray(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new bool[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadBoolean();
            }

            return array;
        }
    }
} // end of namespace