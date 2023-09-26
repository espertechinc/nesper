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
    public class DIOPrimitiveDecimalArray2DimNullableSerde : DataInputOutputSerdeBase<decimal[][]>
    {
        public static readonly DIOPrimitiveDecimalArray2DimNullableSerde INSTANCE =
            new DIOPrimitiveDecimalArray2DimNullableSerde();

        private DIOPrimitiveDecimalArray2DimNullableSerde()
        {
        }

        public override void Write(
            decimal[][] @object,
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

        public override decimal[][] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new decimal[len][];
            for (var i = 0; i < len; i++) {
                array[i] = ReadArray(input);
            }

            return array;
        }

        private void WriteArray(
            decimal[] array,
            DataOutput output)
        {
            if (array == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(array.Length);
            foreach (var i in array) {
                output.WriteDecimal(i);
            }
        }

        private decimal[] ReadArray(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new decimal[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadDecimal();
            }

            return array;
        }
    }
} // end of namespace