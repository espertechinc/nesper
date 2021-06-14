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
    public class DIOMultiKeyArrayIntSerde : DataInputOutputSerdeBase<MultiKeyArrayInt>
    {
        public static readonly DIOMultiKeyArrayIntSerde INSTANCE = new DIOMultiKeyArrayIntSerde();

        public override MultiKeyArrayInt ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return new MultiKeyArrayInt(ReadInternal(input));
        }

        public override void Write(
            MultiKeyArrayInt mk,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(mk.Keys, output);
        }

        private void WriteInternal(
            int[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                output.WriteInt(i);
            }
        }

        private int[] ReadInternal(DataInput input)
        {
            var len = input.ReadInt();
            if (len == -1) {
                return null;
            }

            var array = new int[len];
            for (var i = 0; i < len; i++) {
                array[i] = input.ReadInt();
            }

            return array;
        }
    }
} // end of namespace