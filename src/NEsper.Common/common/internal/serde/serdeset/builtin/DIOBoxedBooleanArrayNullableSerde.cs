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
    public class DIOBoxedBooleanArrayNullableSerde : DataInputOutputSerdeBase<bool?[]>
    {
        public static readonly DIOBoxedBooleanArrayNullableSerde INSTANCE = new DIOBoxedBooleanArrayNullableSerde();

        private DIOBoxedBooleanArrayNullableSerde()
        {
        }

        public void Write(
            bool?[] @object,
            DataOutput output)
        {
            WriteInternal(@object, output);
        }

        public bool?[] Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override void Write(
            bool?[] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(@object, output);
        }

        public override bool?[] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        private void WriteInternal(
            bool?[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                DIONullableBooleanSerde.INSTANCE.Write(i, output);
            }
        }

        private bool?[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new bool?[len];
            for (var i = 0; i < len; i++) {
                array[i] = DIONullableBooleanSerde.INSTANCE.Read(input);
            }

            return array;
        }
    }
} // end of namespace