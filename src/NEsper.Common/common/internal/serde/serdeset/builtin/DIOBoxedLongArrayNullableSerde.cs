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
    public class DIOBoxedLongArrayNullableSerde : DataInputOutputSerdeBase<long?[]>
    {
        public static readonly DIOBoxedLongArrayNullableSerde INSTANCE = new DIOBoxedLongArrayNullableSerde();

        private DIOBoxedLongArrayNullableSerde()
        {
        }

        public void Write(
            long?[] @object,
            DataOutput output)
        {
            WriteInternal(@object, output);
        }

        public long?[] Read(DataInput input)
        {
            return ReadInternal(input);
        }

        public override void Write(
            long?[] @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(@object, output);
        }

        public override long?[] ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return ReadInternal(input);
        }

        private void WriteInternal(
            long?[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                DIONullableLongSerde.INSTANCE.Write(i, output);
            }
        }

        private long?[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new long?[len];
            for (var i = 0; i < len; i++) {
                array[i] = DIONullableLongSerde.INSTANCE.Read(input);
            }

            return array;
        }
    }
} // end of namespace