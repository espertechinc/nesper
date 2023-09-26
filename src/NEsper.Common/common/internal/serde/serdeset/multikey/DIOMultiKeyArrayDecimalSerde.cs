///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
    public class DIOMultiKeyArrayDecimalSerde : DIOMultiKeyArraySerde<MultiKeyArrayDecimal>
    {
        public static readonly DIOMultiKeyArrayDecimalSerde INSTANCE = new DIOMultiKeyArrayDecimalSerde();

        public Type ComponentType => typeof(decimal);

        public void Write(
            object @object,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            Write((MultiKeyArrayDecimal) @object, output, unitKey, writer);
        }

        public object Read(
            DataInput input,
            byte[] unitKey)
        {
            return ReadValue(input, unitKey);
        }
        
        public void Write(
            MultiKeyArrayDecimal mk,
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer)
        {
            WriteInternal(mk.Keys, output);
        }

        public MultiKeyArrayDecimal ReadValue(
            DataInput input,
            byte[] unitKey)
        {
            return new MultiKeyArrayDecimal(ReadInternal(input));
        }

        private void WriteInternal(
            decimal[] @object,
            DataOutput output)
        {
            if (@object == null) {
                output.WriteInt(-1);
                return;
            }

            output.WriteInt(@object.Length);
            foreach (var i in @object) {
                output.WriteDecimal(i);
            }
        }

        private decimal[] ReadInternal(DataInput input)
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