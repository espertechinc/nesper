///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
    public class DIOMultiKeyArrayLongSerde : DataInputOutputSerdeBase<MultiKeyArrayLong>
    {
        public static readonly DIOMultiKeyArrayLongSerde INSTANCE = new DIOMultiKeyArrayLongSerde();

        public override void Write(
            MultiKeyArrayLong mk,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(mk.Keys, output);
        }

        public override MultiKeyArrayLong Read(
            DataInput input,
            byte[] unitKey)
        {
            return new MultiKeyArrayLong(ReadInternal(input));
        }

        private void WriteInternal(
            long[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                output.WriteLong(i);
            }
        }

        private long[] ReadInternal(DataInput input)
        {
            int len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new long[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadLong();
            }

            return array;
        }
    }
} // end of namespace