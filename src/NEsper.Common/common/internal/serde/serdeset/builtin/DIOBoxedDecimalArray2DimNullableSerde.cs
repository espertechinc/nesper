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
    public class DIOBoxedDecimalArray2DimNullableSerde : DataInputOutputSerdeBase<decimal?[][]>
    {
        public static readonly DIOBoxedDecimalArray2DimNullableSerde INSTANCE =
            new DIOBoxedDecimalArray2DimNullableSerde();

        private DIOBoxedDecimalArray2DimNullableSerde()
        {
        }

        public void Write(
            decimal?[][] @object,
            DataOutput output)
        {
            WriteInternal(@object, output);
        }

        public decimal?[][] Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override void Write(
            decimal?[][] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(@object, output);
        }

        public override decimal?[][] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        private void WriteInternal(
            decimal?[][] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                DIOBoxedDecimalArrayNullableSerde.INSTANCE.Write(i, output);
            }
        }

        private decimal?[][] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new decimal?[len][];
            for (var i = 0; i < len; i++) {
                array[i] = DIOBoxedDecimalArrayNullableSerde.INSTANCE.Read(input);
            }

            return array;
        }
    }
} // end of namespace